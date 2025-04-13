using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace BuffFramework
{
	public partial class ModEntry
	{
		internal static List<ICue> pausedSounds = new();
		internal static PerScreen<Color> pausedGlowingColor = new();

		internal static Color PausedGlowingColor
		{
			get => pausedGlowingColor.Value;
			set => pausedGlowingColor.Value = value;
		}

		public static void HandleEventAndFestivalStart()
		{
			if (!Config.ModEnabled)
				return;

			foreach ((_, ICue cue) in soundBuffs.Values)
			{
				if (cue is not null && cue.IsPlaying)
				{
					cue.Pause();
					pausedSounds.Add(cue);
				}
			}
			if (Game1.player.isGlowing)
			{
				PausedGlowingColor = Game1.player.glowingColor;
				Game1.player.stopGlowing();
			}
		}

		public static void HandleEventAndFestivalFinished()
		{
			if (!Config.ModEnabled)
				return;

			foreach (ICue sound in pausedSounds)
			{
				float volume = sound.Volume;

				sound.Resume();
				sound.Volume = 0f;
				DelayedAction.functionAfterDelay(() => {
					sound.Volume = volume;
				}, 500);
			}
			pausedSounds.Clear();
			if (PausedGlowingColor != Color.White)
			{
				Game1.player.startGlowing(PausedGlowingColor, false, GetGlowRate());
				PausedGlowingColor = Color.White;
			}
		}

		public static float GetGlowRate()
		{
			if (!Config.ModEnabled || GlowRateBuffs.Count == 0)
			{
				return Buff.glowRate;
			}
			else
			{
				return GlowRateBuffs.Values.Select(value => GetFloat(value)).Average();
			}
		}

		public static void ApplyBuffsOnEat(Farmer who)
		{
			if (!Config.ModEnabled || !who.IsLocalPlayer)
				return;

			foreach (KeyValuePair<string, Dictionary<string, object>> entry in buffDictionary)
			{
				string id = GetBuffId(entry.Key, entry.Value);

				if (id is not null)
				{
					string consume = null;

					foreach (KeyValuePair<string, object> property in entry.Value)
					{
						switch (property.Key.ToLower())
						{
							case "consume":
								consume = GetString(property.Value);
								break;
						}
						if (consume is not null)
							break;
					}
					if (consume is not null && Game1.player.isEating && Game1.player.itemToEat is Object @object)
					{
						bool isCategory = int.TryParse(consume, out int category);

						if (@object.QualifiedItemId.Equals(consume) || @object.ItemId.Equals(consume) || @object.Name.Equals(consume) || (isCategory && category.Equals(consume)) || @object.HasContextTag(consume))
						{
							CreateOrUpdateBuff(who, id, entry.Value);
						}
					}
				}
			}
		}

		public static void ApplyBuffsOnEquip()
		{
			if (!Config.ModEnabled)
				return;

			Dictionary<string, (Dictionary<string, object>, string)> buffsToAdd = new();
			Dictionary<string, (string, List<string>)> buffsToRemove = new();

			foreach (KeyValuePair<string, Dictionary<string, object>> entry in buffDictionary)
			{
				string id = GetBuffId(entry.Key, entry.Value);
				bool isValid = true;

				if (id is not null)
				{
					string currentItem = null;
					string inventoryContains = null;
					string hat = null;
					string shirt = null;
					string pants = null;
					string boots = null;
					string ring = null;

					foreach (KeyValuePair<string, object> property in entry.Value)
					{
						switch (property.Key.ToLower())
						{
							case "helditem":
							case "currentitem":
								currentItem = GetString(property.Value);
								break;
							case "inventorycontains":
								inventoryContains = GetString(property.Value);
								break;
							case "hat":
								hat = GetString(property.Value);
								break;
							case "shirt":
								shirt = GetString(property.Value);
								break;
							case "pants":
								pants = GetString(property.Value);
								break;
							case "boots":
								boots = GetString(property.Value);
								break;
							case "ring":
								ring = GetString(property.Value);
								break;
						}
						if (currentItem is not null && inventoryContains is not null && hat is not null && shirt is not null && pants is not null && boots is not null && ring is not null)
							break;
					}
					if (currentItem is not null || inventoryContains is not null || hat is not null || shirt is not null || pants is not null || boots is not null || ring is not null)
					{
						static bool isValidRing(Ring ring, string name)
						{
							if (ring.Name == name)
							{
								return true;
							}
							else if (ring is CombinedRing)
							{
								foreach (Ring r in (ring as CombinedRing).combinedRings)
								{
									if (r.Name == name)
									{
										return true;
									}
								}
							}
							return false;
						}

						if (currentItem is not null)
						{
							isValid = currentItem switch
							{
								"TypeTool" => Game1.player.CurrentItem is Tool && (Game1.player.CurrentItem is not MeleeWeapon mw || mw.isScythe()) && Game1.player.CurrentItem is not Slingshot,
								"TypeEnchantableTool" => Game1.player.CurrentItem is Pickaxe or Axe or Hoe or WateringCan or FishingRod or Pan,
								"TypePickaxe" => Game1.player.CurrentItem is Pickaxe,
								"TypeAxe" => Game1.player.CurrentItem is Axe,
								"TypeHoe" => Game1.player.CurrentItem is Hoe,
								"TypeScythe" => Game1.player.CurrentItem is MeleeWeapon mw && mw.isScythe(),
								"TypeWateringCan" => Game1.player.CurrentItem is WateringCan,
								"TypeFishingRod" => Game1.player.CurrentItem is FishingRod,
								"TypePan" => Game1.player.CurrentItem is Pan,
								"TypeShears" => Game1.player.CurrentItem is Shears,
								"TypeMilkPail" => Game1.player.CurrentItem is MilkPail,
								"TypeWand" => Game1.player.CurrentItem is Wand,
								"TypeLantern" => Game1.player.CurrentItem is Lantern,
								"TypeRaft" => Game1.player.CurrentItem is Raft,
								"TypeGenericTool" => Game1.player.CurrentItem is GenericTool,
								"TypeWeapon" => (Game1.player.CurrentItem is MeleeWeapon mw && !mw.isScythe()) || Game1.player.CurrentItem is Slingshot,
								"TypeMeleeWeapon" => Game1.player.CurrentItem is MeleeWeapon mw && !mw.isScythe(),
								"TypeSword" => Game1.player.CurrentItem is MeleeWeapon mw && !mw.isScythe() && (mw.type.Value == 0 || mw.type.Value == 3),
								"TypeStabbingSword" => Game1.player.CurrentItem is MeleeWeapon mw && !mw.isScythe() && mw.type.Value == 0,
								"TypeSlashingSword" or "TypeDefenseSword" => Game1.player.CurrentItem is MeleeWeapon mw && !mw.isScythe() && mw.type.Value == 3,
								"TypeDagger" => Game1.player.CurrentItem is MeleeWeapon mw && !mw.isScythe() && mw.type.Value == 1,
								"TypeClub" or "TypeHammer" => Game1.player.CurrentItem is MeleeWeapon mw && !mw.isScythe() && mw.type.Value == 2,
								"TypeSlingshot" => Game1.player.CurrentItem is Slingshot,
								_ => Game1.player.CurrentItem is not null && Game1.player.CurrentItem.Name == currentItem
							};
						}
						if (isValid && inventoryContains is not null && (Game1.player.Items is null || !Game1.player.Items.Any(item => item is not null && (item.Name == inventoryContains || (item is Tool tool && tool.attachments.Any(attachment => attachment is not null && attachment.Name == inventoryContains))))))
						{
							isValid = false;
						}
						if (isValid && hat is not null && (Game1.player.hat.Value is null || Game1.player.hat.Value.Name != hat))
						{
							isValid = false;
						}
						if (isValid && shirt is not null && (Game1.player.shirtItem.Value is null || Game1.player.shirtItem.Value.Name != shirt))
						{
							isValid = false;
						}
						if (isValid && pants is not null && (Game1.player.pantsItem.Value is null || Game1.player.pantsItem.Value.Name != pants))
						{
							isValid = false;
						}
						if (isValid && boots is not null && (Game1.player.boots.Value is null || Game1.player.boots.Value.Name != boots))
						{
							isValid = false;
						}
						if (isValid && ring is not null && (Game1.player.leftRing.Value is null || !isValidRing(Game1.player.leftRing.Value, ring)) && (Game1.player.rightRing.Value is null || !isValidRing(Game1.player.rightRing.Value, ring)))
						{
							isValid = false;
						}
						if (isValid)
						{
							buffsToAdd[entry.Key] = new(entry.Value, id);
						}
						buffsToRemove[entry.Key] = (id, GetAdditionalBuffsAsTupleList(entry.Value)?.Select(t => t.Item1).ToList());
					}
				}
			}
			foreach ((string key, (string id, List<string> additionalBuffsIds)) in buffsToRemove)
			{
				if (!buffsToAdd.ContainsKey(key))
				{
					if (Game1.player.hasBuff(id))
					{
						Game1.player.buffs.Remove(id);
						if (additionalBuffsIds is not null)
						{
							foreach (string additionalBuffsId in additionalBuffsIds)
							{
								Game1.player.buffs.Remove(additionalBuffsId);
							}
						}
					}
				}
			}
			foreach ((_, (Dictionary<string, object> value, string id)) in buffsToAdd)
			{
				CreateOrUpdateBuff(Game1.player, id, value);
			}
		}

		public static void ApplyBuffsOther()
		{
			if (!Config.ModEnabled)
				return;

			foreach (KeyValuePair<string, Dictionary<string, object>> entry in buffDictionary)
			{
				string id = GetBuffId(entry.Key, entry.Value);

				if (id is not null)
				{
					string consume = null;
					string currentItem = null;
					string inventoryContains = null;
					string hat = null;
					string shirt = null;
					string pants = null;
					string boots = null;
					string ring = null;

					foreach (KeyValuePair<string, object> property in entry.Value)
					{
						switch (property.Key.ToLower())
						{
							case "consume":
								consume = GetString(property.Value);
								break;
							case "helditem":
							case "currentitem":
								currentItem = GetString(property.Value);
								break;
							case "inventorycontains":
								inventoryContains = GetString(property.Value);
								break;
							case "hat":
								hat = GetString(property.Value);
								break;
							case "shirt":
								shirt = GetString(property.Value);
								break;
							case "pants":
								pants = GetString(property.Value);
								break;
							case "boots":
								boots = GetString(property.Value);
								break;
							case "ring":
								ring = GetString(property.Value);
								break;
						}
						if (consume is not null || currentItem is not null || inventoryContains is not null || hat is not null || shirt is not null || pants is not null || boots is not null || ring is not null)
							break;
					}
					if (consume is null && currentItem is null && inventoryContains is null && hat is null && shirt is null && pants is null && boots is null && ring is null)
					{
						CreateOrUpdateBuff(Game1.player, id, entry.Value);
					}
				}
			}
		}

		public static void UpdateBuffs()
		{
			if (!Config.ModEnabled)
				return;

			Dictionary<string, Dictionary<string, object>> oldBuffDict = buffDictionary;

			SHelper.GameContent.InvalidateCache(dictionaryKey);
			buffDictionary = SHelper.GameContent.Load<Dictionary<string, Dictionary<string, object>>>(dictionaryKey);
			foreach (BuffFrameworkAPI instance in APIInstances)
			{
				foreach ((string key, (Dictionary<string, object> value, Func<bool> function)) in instance.dictionary)
				{
					if (function is null || function())
					{
						buffDictionary.TryAdd(key, value);
					}
					else
					{
						buffDictionary.Remove(key);
					}
				}
			}
			foreach (KeyValuePair<string, Dictionary<string, object>> entry in oldBuffDict)
			{
				if (!buffDictionary.ContainsKey(entry.Key))
				{
					string id = GetBuffId(entry.Key, entry.Value);

					if (id is not null)
					{
						List<string> additionalBuffsIds = GetAdditionalBuffsAsTupleList(entry.Value)?.Select(t => t.Item1).ToList();

						if (Game1.player.hasBuff(id))
						{
							Game1.player.buffs.Remove(id);
							if (additionalBuffsIds is not null)
							{
								foreach (string additionalBuffsId in additionalBuffsIds)
								{
									Game1.player.buffs.Remove(additionalBuffsId);
								}
							}
						}
					}
				}
			}
			ApplyBuffsOnEquip();
			ApplyBuffsOther();
		}

		public static string GetBuffId(string key, Dictionary<string, object> value)
		{
			string which = null;
			string buffId = null;
			string id;

			foreach (KeyValuePair<string, object> property in value)
			{
				switch (property.Key.ToLower())
				{
					case "which":
						which = GetIntAsString(property.Value);
						break;
					case "id":
					case "buffid":
						buffId = GetString(property.Value);
						break;
				}
			}
			if (which is not null && int.Parse(which) >= 0)
			{
				id = which;
			}
			else
			{
				if (buffId is not null)
				{
					id = buffId;
				}
				else
				{
					buffDictionary.Remove(key);
					SMonitor.Log($"{key}: Which and Id (or BuffId) fields are both missing", LogLevel.Error);
					return null;
				}
			}
			return id;
		}

		public static void CreateOrUpdateBuff(Farmer who, string id, Dictionary<string, object> value)
		{
			Buff buff = CreateBuff(id, value);
			List<Buff> additionalBuffs = CreateAdditionalBuffs(buff, value);

			if (who.buffs.IsApplied(buff.id))
			{
				who.buffs.AppliedBuffs[buff.id].millisecondsDuration = who.buffs.AppliedBuffs[buff.id].totalMillisecondsDuration;
			}
			else
			{
				who.buffs.Apply(buff);
			}
			if (additionalBuffs is not null)
			{
				foreach (Buff additionalBuff in additionalBuffs)
				{
					if (who.buffs.IsApplied(additionalBuff.id))
					{
						if (additionalBuff.totalMillisecondsDuration == Buff.ENDLESS)
						{
							who.buffs.AppliedBuffs[additionalBuff.id].millisecondsDuration = additionalBuff.totalMillisecondsDuration;
						}
						else if (who.buffs.AppliedBuffs[additionalBuff.id].totalMillisecondsDuration != Buff.ENDLESS)
						{
							who.buffs.AppliedBuffs[additionalBuff.id].millisecondsDuration = Math.Max(additionalBuff.totalMillisecondsDuration, who.buffs.AppliedBuffs[additionalBuff.id].totalMillisecondsDuration);
						}
						who.buffs.AppliedBuffs[additionalBuff.id].visible = who.buffs.AppliedBuffs[additionalBuff.id].visible && additionalBuff.visible;
					}
					else
					{
						who.buffs.Apply(additionalBuff);
					}
				}
			}
		}

		private static Buff CreateBuff(string id, Dictionary<string, object> value)
		{
			string iconTexture = null;
			int? duration = null;
			int? maxDuration = null;

			foreach (KeyValuePair<string, object> property in value)
			{
				switch (property.Key.ToLower())
				{
					case "texturepath":
					case "icontexture":
						iconTexture = GetString(property.Value);
						break;
					case "duration":
						duration = GetInt(property.Value) * 1000;
						break;
					case "maxduration":
						maxDuration = GetInt(property.Value) * 1000;
						break;
				}
			}

			Buff buff = new(id);

			if (duration.HasValue)
			{
				int millisecondsDuration = duration.Value;

				if (maxDuration.HasValue && maxDuration > 0 && maxDuration > duration && duration != Buff.ENDLESS && maxDuration != Buff.ENDLESS)
				{
					millisecondsDuration = Game1.random.Next(duration.Value, maxDuration.Value + 1);
				}
				buff.millisecondsDuration = millisecondsDuration;
				buff.totalMillisecondsDuration = millisecondsDuration;
			}
			else if (buff.millisecondsDuration <= 0 && buff.millisecondsDuration != Buff.ENDLESS)
			{
				buff.millisecondsDuration = Buff.ENDLESS;
				buff.totalMillisecondsDuration = Buff.ENDLESS;
			}
			if (!string.IsNullOrEmpty(iconTexture))
			{
				Texture2D texture = SHelper.GameContent.Load<Texture2D>(iconTexture);
				int textureX = 0;
				int textureY = 0;
				int textureWidth = texture.Width;
				int textureHeight = texture.Height;

				foreach (KeyValuePair<string, object> property in value)
				{
					switch (property.Key.ToLower())
					{
						case "texturex":
							textureX = GetInt(property.Value);
							break;
						case "texturey":
							textureY = GetInt(property.Value);
							break;
						case "texturewidth":
							textureWidth = GetInt(property.Value);
							break;
						case "textureheight":
							textureHeight = GetInt(property.Value);
							break;
					}
				}
				buff.iconTexture = ResizeTexture(ExtractTexture(texture, textureX, textureY, textureWidth, textureHeight), 16, 16);
			}
			else
			{
				buff.iconTexture ??= ExtractTexture(Game1.mouseCursors, 320, 496, 16, 16);
			}

			foreach (KeyValuePair<string, object> property in value)
			{
				switch (property.Key.ToLower())
				{
					case "iconspriteindex":
					case "sheetindex":
					case "iconsheetindex":
						buff.iconSheetIndex = Math.Max(0, GetInt(property.Value));
						if (string.IsNullOrEmpty(iconTexture))
						{
							buff.iconTexture = Game1.buffsIcons;
						}
						break;
					case "name":
					case "displayname":
						buff.displayName = GetString(property.Value, true);
						break;
					case "displaydescription":
					case "description":
						buff.description = GetString(property.Value, true);
						break;
					case "source":
						buff.source = GetString(property.Value);
						break;
					case "displaysource":
						buff.displaySource = GetString(property.Value, true);
						break;
					case "visibility":
					case "visible":
						buff.visible = GetBool(property.Value);
						break;
					case "farming":
					case "farminglevel":
						buff.effects.FarmingLevel.Value = GetFloat(property.Value);
						break;
					case "mining":
					case "mininglevel":
						buff.effects.MiningLevel.Value = GetFloat(property.Value);
						break;
					case "fishing":
					case "fishinglevel":
						buff.effects.FishingLevel.Value = GetFloat(property.Value);
						break;
					case "foraging":
					case "foraginglevel":
						buff.effects.ForagingLevel.Value = GetFloat(property.Value);
						break;
					case "combat":
					case "combatlevel":
						buff.effects.CombatLevel.Value = GetFloat(property.Value);
						break;
					case "attack":
						buff.effects.Attack.Value = GetFloat(property.Value);
						break;
					case "attackmultiplier":
						buff.effects.AttackMultiplier.Value = GetFloat(property.Value);
						break;
					case "criticalchancemultiplier":
						buff.effects.CriticalChanceMultiplier.Value = GetFloat(property.Value);
						break;
					case "criticalpowermultiplier":
						buff.effects.CriticalPowerMultiplier.Value = GetFloat(property.Value);
						break;
					case "weaponprecisionmultiplier":
						buff.effects.WeaponPrecisionMultiplier.Value = GetFloat(property.Value);
						break;
					case "weaponspeedmultiplier":
						buff.effects.WeaponSpeedMultiplier.Value = GetFloat(property.Value);
						break;
					case "weightmultiplier":
					case "knockbackmultiplier":
						buff.effects.KnockbackMultiplier.Value = GetFloat(property.Value);
						break;
					case "defense":
						buff.effects.Defense.Value = GetFloat(property.Value);
						break;
					case "immunity":
						buff.effects.Immunity.Value = GetFloat(property.Value);
						break;
					case "maxenergy":
					case "maxstamina":
						buff.effects.MaxStamina.Value = GetFloat(property.Value);
						break;
					case "luck":
						buff.effects.LuckLevel.Value = GetFloat(property.Value);
						break;
					case "magneticradius":
						buff.effects.MagneticRadius.Value = GetFloat(property.Value);
						break;
					case "speed":
						buff.effects.Speed.Value = GetFloat(property.Value);
						break;
					case "glowcolor":
					case "glow":
						if (property.Value is JObject j)
						{
							buff.glow = new Color((byte)(long)j["R"], (byte)(long)j["G"], (byte)(long)j["B"], (byte)(long)j["A"]);
						}
						else
						{
							Color? c = Utility.StringToColor(property.Value.ToString());

							if (c.HasValue)
							{
								buff.glow = c.Value;
							}
						}
						break;
					case "healthregen":
					case "healthregeneration":
						HealthRegenerationBuffs.TryAdd(id, GetFloatAsString(property.Value));
						break;
					case "energyregen":
					case "energyregeneration":
					case "staminaregen":
					case "staminaregeneration":
						StaminaRegenerationBuffs.TryAdd(id, GetFloatAsString(property.Value));
						break;
					case "glowrate":
						GlowRateBuffs.TryAdd(id, GetFloatAsString(property.Value));
						break;
					case "sound":
						soundBuffs.TryAdd(id, (GetString(property.Value), null));
						break;
				}
			}
			return buff;
		}

		public static List<Buff> CreateAdditionalBuffs(Buff buff, Dictionary<string, object> value)
		{
			List<(string, bool)> additionalBuffsAsTupleList = GetAdditionalBuffsAsTupleList(buff, value);

			if (additionalBuffsAsTupleList is not null)
			{
				List<Buff> additionalBuffs = new();

				foreach ((string id, bool visible) in additionalBuffsAsTupleList)
				{
					additionalBuffs.Add(new Buff(id)
					{
						millisecondsDuration = buff.millisecondsDuration,
						totalMillisecondsDuration = buff.totalMillisecondsDuration,
						visible = visible,
						source = buff.source,
						displaySource = buff.displaySource
					});
				}
				return additionalBuffs;
			}
			return null;
		}

		public static List<(string, bool)> GetAdditionalBuffsAsTupleList(Buff buff, Dictionary<string, object> value)
		{
			return GetAdditionalBuffsAsTupleList(value, buff.visible);
		}

		public static List<(string, bool)> GetAdditionalBuffsAsTupleList(Dictionary<string, object> value, bool visibility = true)
		{
			foreach (KeyValuePair<string, object> property in value)
			{
				switch (property.Key.ToLower())
				{
					case "additionalbuffs":
						return GetAdditionalBuffsAsTupleListInternal(property.Value, visibility);
				}
			}

			static List<(string, bool)> GetAdditionalBuffsAsTupleListInternal(object value, bool defaultVisibility)
			{
				List<object> additionalBuffsList = (value as JArray)?.ToObject<List<object>>();

				if (additionalBuffsList is not null)
				{
					List<(string, bool)> additionalBuffsAsTupleList = new();

					foreach (object additionalBuff in additionalBuffsList)
					{
						Dictionary<string, object> additionalBuffDictionary = (additionalBuff as JObject)?.ToObject<Dictionary<string, object>>();

						if (additionalBuffDictionary is not null)
						{
							string which = null;
							string buffId = null;
							bool visible = defaultVisibility;
							string id = null;

							foreach (KeyValuePair<string, object> property in additionalBuffDictionary)
							{
								switch (property.Key.ToLower())
								{
									case "which":
										which = GetIntAsString(property.Value);
										break;
									case "id":
									case "buffid":
										buffId = GetString(property.Value);
										break;
									case "visibility":
									case "visible":
										visible = GetBool(property.Value);
										break;
								}
							}
							if (which is not null && int.Parse(which) >= 0)
							{
								id = which;
							}
							else if (buffId is not null)
							{
								id = buffId;
							}
							if (id is not null)
							{
								additionalBuffsAsTupleList.Add((id, visible));
							}
						}
					}
					return additionalBuffsAsTupleList;
				}
				return null;
			}

			return null;
		}

		public static int GetInt(object value)
		{
			if (value is int i)
			{
				return i;
			}
			else if (value is long l)
			{
				return (int)l;
			}
			else if (value is float f)
			{
				return (int)f;
			}
			else if (value is double d)
			{
				return (int)d;
			}
			else if (value is string s && int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
			{
				return i;
			}
			else
			{
				return 0;
			}
		}

		public static float GetFloat(object value)
		{
			if (value is int i)
			{
				return i;
			}
			else if (value is long l)
			{
				return l;
			}
			else if (value is float f)
			{
				return f;
			}
			else if (value is double d)
			{
				return (float)d;
			}
			else if (value is string s && float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f))
			{
				return f;
			}
			else
			{
				return 0f;
			}
		}

		public static bool GetBool(object value)
		{
			if (value is int i)
			{
				return i != 0;
			}
			else if (value is long l)
			{
				return l != 0;
			}
			else if (value is float f)
			{
				return f != 0f;
			}
			else if (value is double d)
			{
				return d != 0d;
			}
			else if (value is bool b)
			{
				return b;
			}
			else if (value is string s)
			{
				string sToLower = s.ToLower();

				if (sToLower.Equals("t") || sToLower.Equals("true"))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static string GetString(object value, bool tokenizable = false)
		{
			if (value is null)
			{
				return string.Empty;
			}
			else if (value is string s)
			{
				return tokenizable ? TokenParser.ParseText(s) : s;
			}
			else
			{
				return tokenizable ? TokenParser.ParseText(value.ToString()) : value.ToString();
			}
		}

		public static string GetIntAsString(object value)
		{
			if (value is int i)
			{
				return i.ToString();
			}
			else if (value is long l)
			{
				return ((int)l).ToString();
			}
			else if (value is string s)
			{
				return s;
			}
			else
			{
				return "0";
			}
		}

		public static string GetFloatAsString(object value)
		{
			if (value is int i)
			{
				return i.ToString();
			}
			else if (value is long l)
			{
				return ((int)l).ToString();
			}
			if (value is float f)
			{
				return f.ToString();
			}
			else if (value is double d)
			{
				return ((float)d).ToString();
			}
			else if (value is string s)
			{
				return s;
			}
			else
			{
				return "0";
			}
		}

		public static Texture2D ExtractTexture(Texture2D sourceTexture, int x, int y, int width, int height)
		{
			if (x == 0 && y == 0 && width == sourceTexture.Width && height == sourceTexture.Height)
				return sourceTexture;

			Texture2D extractedTexture = new(sourceTexture.GraphicsDevice, width, height);
			Color[] data = new Color[width * height];

			sourceTexture.GetData(0, new Rectangle(x, y, width, height), data, 0, data.Length);
			extractedTexture.SetData(data);
			return extractedTexture;
		}

		public static Texture2D ResizeTexture(Texture2D sourceTexture, int newWidth, int newHeight)
		{
			if (sourceTexture.Width == newWidth && sourceTexture.Height == newHeight)
				return sourceTexture;

			Texture2D resizedTexture = new(sourceTexture.GraphicsDevice, newWidth, newHeight);
			Color[] sourceData = new Color[sourceTexture.Width * sourceTexture.Height];
			Color[] resizedData = new Color[newWidth * newHeight];

			sourceTexture.GetData(sourceData);

			float scaleX = (float)sourceTexture.Width / newWidth;
			float scaleY = (float)sourceTexture.Height / newHeight;

			for (int y = 0; y < newHeight; y++)
			{
				for (int x = 0; x < newWidth; x++)
				{
					float sourceX = x * scaleX;
					float sourceY = y * scaleY;
					int sourceXFloor = (int)Math.Floor(sourceX);
					int sourceYFloor = (int)Math.Floor(sourceY);
					int sourceXCeil = Math.Min(sourceXFloor + 1, sourceTexture.Width - 1);
					int sourceYCeil = Math.Min(sourceYFloor + 1, sourceTexture.Height - 1);
					float weightX = sourceX - sourceXFloor;
					float weightY = sourceY - sourceYFloor;

					Color topLeft = sourceData[sourceYFloor * sourceTexture.Width + sourceXFloor];
					Color topRight = sourceData[sourceYFloor * sourceTexture.Width + sourceXCeil];
					Color bottomLeft = sourceData[sourceYCeil * sourceTexture.Width + sourceXFloor];
					Color bottomRight = sourceData[sourceYCeil * sourceTexture.Width + sourceXCeil];
					Color top = Color.Lerp(topLeft, topRight, weightX);
					Color bottom = Color.Lerp(bottomLeft, bottomRight, weightX);
					Color finalColor = Color.Lerp(top, bottom, weightY);
					resizedData[y * newWidth + x] = finalColor;
				}
			}
			resizedTexture.SetData(resizedData);
			return resizedTexture;
		}

		public static void ClearAll()
		{
			Game1.player.buffs.Clear();
			HealthRegenerationBuffs.Clear();
			StaminaRegenerationBuffs.Clear();
			GlowRateBuffs.Clear();
			soundBuffs.Clear();
			pausedSounds.Clear();
			PausedGlowingColor = Color.White;
		}
	}
}
