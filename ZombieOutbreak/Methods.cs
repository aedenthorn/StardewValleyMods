using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Characters;

namespace ZombieOutbreak
{
	public partial class ModEntry
	{
		public static void MakeZombieSpeak(ref string dialogue)
		{
			SMonitor.Log($"input {dialogue}");
			const string fork = "%fork";
			const string forkPlaceholder = "?!???!!??!??!!!!?!?!??!??!??!?!!";

			dialogue = dialogue.Replace(fork, forkPlaceholder);

			string[] strs = dialogue.Split('#');

			for (int i = 0; i < strs.Length; i++)
			{
				string str = strs[i];

				if (str.StartsWith("$") || str.StartsWith("%") || str.StartsWith("[") || str.Length == 0)
					continue;
				if (i > 0 && strs[i - 1].StartsWith("$r ") && !zombieFarmerTextures.ContainsKey(Game1.player.UniqueMultiplayerID))
					continue;

				Regex x1 = new(@"\$[hsuln]");
				Regex x2 = new(@"\$neutral");
				Regex x3 = new(@"\$[0-9]+");
				Regex r1 = new(@"[AEIOUÀÁÂÃÄÅĀĂĄÆÈÉÊËĒĖĘÎÏĨĪŌÔÒÓÖÕŒÙÚÛÜŪŮÝŸ]");
				Regex r1a = new(@"[aeiouàáâãäåāăąæèéêëēėęîïĩīōôòóöõœùúûüūůýÿ]");
				Regex r2 = new(@"[RSTLNDCMŔŜŤŁŃĐČĆŹŻ]");
				Regex r2a = new(@"[rstlndcmŕŝťłńđčćźż]");
				Regex r3 = new(@"[GHĜĞ]");
				Regex r3a = new(@"[ghĝğ]");
				Regex r4 = new(@"[BFJKPQVWXYZÇÐĴŁÑŠŽ]");
				Regex r4a = new(@"[bfjkpqvwxyzçðĵłñšž]");

				str = x1.Replace(str, "");
				str = x2.Replace(str, "");
				str = x3.Replace(str, "");
				str = r1.Replace(str, "A");
				str = r2.Replace(str, "R");
				str = r3.Replace(str, "G");
				str = r4.Replace(str, "H");
				str = r1a.Replace(str, "a");
				str = r2a.Replace(str, "r");
				str = r3a.Replace(str, "g");
				str = r4a.Replace(str, "h");
				strs[i] = str;
			}
			dialogue = string.Join("#", strs);
			dialogue = dialogue.Replace(forkPlaceholder, fork);
			SMonitor.Log($"zombified: {dialogue}");
		}

		public static void CheckForInfection()
		{
			if (Game1.characterData is null)
				return;

			if (zombieFarmerTextures.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				int[] buffIds = { 13, 14, 18 };

				foreach (int id in buffIds)
				{
					string uniqueId = $"{SModManifest.UniqueID}_Zombification_{id}";

					if (!Game1.player.buffs.AppliedBuffIds.Contains(uniqueId))
					{
						Buff buff = new(uniqueId, $"{SModManifest.UniqueID}_Zombification", effects: new BuffEffects(new BuffAttributesData() {
							Attack = id == 18 ? -1f : 0f,
							Defense = id == 14 ? -1f : 0f,
							Speed = id == 13 ? -0.5f : 0f,
						}))
						{
							displayName = SHelper.Translation.Get("buff.zombification.name"),
							description = SHelper.Translation.Get("buff.zombification.description"),
							displaySource = SHelper.Translation.Get("buff.zombification.source"),
							iconTexture = Game1.buffsIcons,
							iconSheetIndex = id,
							millisecondsDuration = Buff.ENDLESS,
							totalMillisecondsDuration = Buff.ENDLESS,
						};

						Game1.player.buffs.Apply(buff);
					}
				}
			}
			foreach (string name in Game1.characterData.Keys)
			{
				if (zombieNPCTextures.ContainsKey(name))
				{
					NPC npc1 = Game1.getCharacterFromName(name);

					if (npc1 is null)
						continue;

					foreach (NPC npc2 in npc1.currentLocation.characters)
					{
						float distance = Vector2.Distance(npc1.Position, npc2.Position);

						if (Game1.characterData.ContainsKey(npc2.Name) &&
							!zombieNPCTextures.ContainsKey(npc2.Name) &&
							!curedNPCs.Contains(npc2.Name) &&
							distance < Config.InfectionRadius &&
							Game1.random.NextDouble() < (Config.InfectionChancePerSecond / 100f))
						{
							SMonitor.Log($"{name} turned {npc2.Name} into a zombie! (distance {distance})");
							AddZombieNPC(npc2.Name);
						}
					}
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						float distance = Vector2.Distance(npc1.Position, farmer.Position);

						if (!zombieFarmerTextures.ContainsKey(farmer.UniqueMultiplayerID) &&
							npc1.currentLocation == farmer.currentLocation &&
							!curedFarmers.Contains(farmer.UniqueMultiplayerID) &&
							distance < Config.InfectionRadius &&
							Game1.random.NextDouble() < (Config.InfectionChancePerSecond / 100f))
						{
							SMonitor.Log($"{npc1.Name} turned farmer {farmer.Name} into a zombie! (distance {distance})");
							AddZombieFarmer(farmer.UniqueMultiplayerID);
						}
					}
				}
				else if (zombieFarmerTextures.Count > 0)
				{
					NPC npc = Game1.getCharacterFromName(name);

					if (npc is null)
						continue;

					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						float distance = Vector2.Distance(farmer.Position, npc.Position);

						if (zombieFarmerTextures.ContainsKey(farmer.UniqueMultiplayerID) &&
							farmer.currentLocation == npc.currentLocation &&
							distance < Config.InfectionRadius &&
							Game1.random.NextDouble() < (Config.InfectionChancePerSecond / 100f))
						{
							SMonitor.Log($"farmer {farmer.Name} turned {npc.Name} into a zombie! (distance {distance})");
							AddZombieNPC(npc.Name);
						}
					}
				}
			}
			if (zombieFarmerTextures.Count > 0)
			{
				foreach (Farmer farmer1 in Game1.getAllFarmers().Where(f => zombieFarmerTextures.ContainsKey(f.UniqueMultiplayerID)))
				{
					foreach (Farmer farmer2 in Game1.getAllFarmers())
					{
						float distance = Vector2.Distance(farmer1.Position, farmer2.Position);

						if (!zombieFarmerTextures.ContainsKey(farmer2.UniqueMultiplayerID) &&
							farmer1.currentLocation == farmer2.currentLocation &&
							!curedFarmers.Contains(farmer2.UniqueMultiplayerID) &&
							distance < Config.InfectionRadius &&
							Game1.random.NextDouble() < (Config.InfectionChancePerSecond / 100f))
						{
							SMonitor.Log($"farmer {farmer1.Name} turned {farmer2.Name} into a zombie! (distance {distance})");
							AddZombieNPC(farmer2.Name);
						}
					}
				}
			}
		}

		public static void MakeRandomZombie()
		{
			SMonitor.Log($"Adding random zombie");
			NPC npc = Utility.GetRandomNpc((string name, CharacterData data) => !zombieNPCTextures.ContainsKey(name));

			if (npc is not null)
			{
				AddZombieNPC(npc.Name);
			}
		}

		public static void AddZombieNPC(string name)
		{
			NPC npc = Game1.getCharacterFromName(name);

			if (npc is not null && npc.CanSocialize)
			{
				if (curedNPCs.Contains(name))
				{
					SMonitor.Log($"{name} is immune to zombification today");
					return;
				}
				if (!zombieNPCTextures.ContainsKey(name))
				{
					MakeZombieNPCTexture(name);
					SMonitor.Log($"{name} turned into a zombie!");
				}
			}
		}

		public static void RemoveZombieNPC(string name)
		{
			NPC npc = Game1.getCharacterFromName(name);

			zombieNPCTextures.Remove(name);
			zombieNPCPortraits.Remove(name);
			curedNPCs.Add(name);
			npc.Sprite.spriteTexture = SHelper.GameContent.Load<Texture2D>($"Characters/{name}");
			SHelper.GameContent.InvalidateCache($"Portraits/{name}");
			SMonitor.Log($"{name} was cured of zombification!");
		}

		public static void AddZombieFarmer(long id)
		{
			if (curedFarmers.Contains(id))
			{
				SMonitor.Log($"{id} is immune to zombification today");
				return;
			}
			if (!zombieFarmerTextures.ContainsKey(id))
			{
				MakeZombieFarmerTexture(id);
				SMonitor.Log($"player {id} turned into a zombie!");
			}
		}

		public static void RemoveZombieFarmer(long id)
		{
			Farmer farmer = Game1.GetPlayer(id, true);

			zombieFarmerTextures.Remove(id);
			curedFarmers.Add(id);
			foreach (Buff buff in farmer.buffs.AppliedBuffs.Values)
			{
				if (buff.source == $"{SModManifest.UniqueID}_Zombification")
				{
					farmer.buffs.Remove(buff.id);
				}
			}
			farmer.FarmerRenderer.recolorEyes(SHelper.Reflection.GetField<NetColor>(farmer.FarmerRenderer, "eyes").GetValue().Value);
			farmer.FarmerRenderer.recolorSkin(SHelper.Reflection.GetField<NetInt>(farmer.FarmerRenderer, "skin").GetValue().Value, true);
			farmer.FarmerRenderer.recolorShoes(SHelper.Reflection.GetField<NetString>(farmer.FarmerRenderer, "shoes").GetValue().Value);
			farmer.FarmerRenderer.changeShirt(SHelper.Reflection.GetField<NetString>(farmer.FarmerRenderer, "shirt").GetValue().Value);
			farmer.FarmerRenderer.changePants(SHelper.Reflection.GetField<NetString>(farmer.FarmerRenderer, "pants").GetValue().Value);
			farmer.FarmerRenderer.textureChanged();
			SMonitor.Log($"player {farmer.Name} was cured of zombification!");
		}

		public static void MakeZombieNPCTexture(string name)
		{
			NPC npc = Game1.getCharacterFromName(name);

			if (npc is null)
			{
				SMonitor.Log($"Error getting character from name {name}", LogLevel.Error);
				return;
			}

			Texture2D texture = npc.Sprite.Texture;
			Texture2D texture2 = npc.Portrait;

			if (texture is null || texture2 is null)
			{
				SMonitor.Log($"npc {npc.Name} has no texture or portrait", LogLevel.Error);
				return;
			}

			Color[] data = new Color[texture.Width * texture.Height];
			Color[] data2 = new Color[texture2.Width * texture2.Height];

			texture.GetData(data);
			texture2.GetData(data2);

			float green = Math.Min(Math.Max(0, Config.GreenTint / 100f), 1);

			for (int i = 0; i < data.Length || i < data2.Length; i++)
			{
				if (i < data.Length && data[i] != Color.Transparent)
				{
					data[i].R -= (byte)Math.Round(data[i].R * green / 2);
					data[i].B -= (byte)Math.Round(data[i].B * green);
				}
				if (i < data2.Length && data2[i] != Color.Transparent)
				{
					data2[i].R -= (byte)Math.Round(data2[i].R * green / 2);
					data2[i].B -= (byte)Math.Round(data2[i].B * green);
				}
			}
			texture.SetData(data);
			texture2.SetData(data2);
			zombieNPCTextures[name] = texture;
			zombieNPCPortraits[name] = texture2;
		}

		public static void MakeZombieFarmerTexture(long id)
		{
			Farmer farmer = Game1.GetPlayer(id, true);

			if (farmer is null)
			{
				SMonitor.Log($"Error getting famer from id {id}", LogLevel.Error);
				return;
			}

			Texture2D texture = SHelper.Reflection.GetField<Texture2D>(farmer.FarmerRenderer, "baseTexture").GetValue();

			if (texture is null)
			{
				SMonitor.Log($"farmer {farmer.Name} has no texture", LogLevel.Error);
				return;
			}

			Color[] data = new Color[texture.Width * texture.Height];

			texture.GetData(data);

			float green = Math.Min(Math.Max(0, Config.GreenTint / 100f), 1);

			for (int i = 0; i < data.Length; i++)
			{
				if (i < data.Length && data[i] != Color.Transparent)
				{
					data[i].R -= (byte)Math.Round(data[i].R * green / 2);
					data[i].B -= (byte)Math.Round(data[i].B * green);
				}
			}
			texture.SetData(data);
			zombieFarmerTextures[id] = texture;
		}

		public static void ClearAll()
		{
			zombieNPCTextures.Clear();
			zombieNPCPortraits.Clear();
			zombieFarmerTextures.Clear();
			curedNPCs.Clear();
			curedFarmers.Clear();
		}
	}
}
