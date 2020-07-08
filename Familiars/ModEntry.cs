using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;

namespace Familiars
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetLoader
	{
		public static ModEntry context;
		public static ModConfig Config;
		public static IModHelper SHelper;
		public static IMonitor SMonitor;
		public static List<Type> familiarTypes = new List<Type>()
		{
			typeof(DustSpiritFamiliar),
			typeof(DinoFamiliar),
			typeof(BatFamiliar)
		};
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			SHelper = helper;
			SMonitor = Monitor;

			if (!Config.EnableMod)
				return;

			Helper.ConsoleCommands.Add("DispelFamiliars", "Dispel all familiars.", new System.Action<string, string[]>(DispelFamiliars));
			Helper.ConsoleCommands.Add("DF", "Dispel all familiars.", new System.Action<string, string[]>(DispelFamiliars));
			Helper.ConsoleCommands.Add("CallFamiliar", "Call a familiar. Usage: CallFamiliar <familiarType>", new System.Action<string, string[]>(CallFamiliar));
			Helper.ConsoleCommands.Add("CF", "Call a familiar. Usage: CF <familiarType>", new System.Action<string, string[]>(CallFamiliar));
			Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			Helper.Events.Player.Warped += Player_Warped;
		}

		private void DispelFamiliars(string arg1, string[] arg2)
		{
			if (Game1.player == null || Game1.player.currentLocation == null)
				return;

			for (int i = Game1.player.currentLocation.characters.Count - 1; i >= 0; i--)
			{
				NPC npc = Game1.player.currentLocation.characters[i];
				if (familiarTypes.Contains(npc.GetType()))
				{
					Game1.player.currentLocation.characters.RemoveAt(i);
				}
			}
		}

		private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
		{
			if (e.OldLocation.characters == null)
				return;
			Monitor.Log($"Warping");

			for (int i = e.OldLocation.characters.Count - 1; i >= 0; i--)
			{
				NPC npc = e.OldLocation.characters[i];
				if (familiarTypes.Contains(npc.GetType()))
				{
					Farmer owner = Helper.Reflection.GetField<Farmer>(npc, "owner").GetValue();
					if (owner == Game1.player)
					{
						Monitor.Log($"Warping {npc.GetType()}");
						Game1.warpCharacter(npc, e.NewLocation.Name, Game1.player.getTileLocationPoint());
					}
				}
			}
		}

		private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
		{
		}

		private void CallFamiliar(string arg1, string[] arg2)
		{
			if (Game1.player == null || Game1.player.currentLocation == null)
				return;

			switch (arg2[0].ToLower())
			{
				case "dust":
					Game1.player.currentLocation.characters.Add(new DustSpiritFamiliar(Game1.player.position, Game1.player));
					break;
				case "dino":
					Game1.player.currentLocation.characters.Add(new DinoFamiliar(Game1.player.position, Game1.player));
					break;
				case "bat":
					Game1.player.currentLocation.characters.Add(new BatFamiliar(Game1.player.position, Game1.player));
					break;
			}
		}

	public bool CanLoad<T>(IAssetInfo asset)
	{
		if (!Config.EnableMod)
			return false;

		if (asset.AssetName.EndsWith("BatFamiliar") || asset.AssetName.EndsWith("DinoFamiliar") || asset.AssetName.EndsWith("DustSpiritFamiliar"))
		{
			return true;
		}

		return false;
	}

		/// <summary>Load a matched asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public T Load<T>(IAssetInfo asset)
		{
			string name = asset.AssetName.Replace("Characters\\Monsters\\", "").Replace("Characters/Monsters/", "");
			return (T)(object)Helper.Content.Load<Texture2D>($"assets/{name}.png");
		}
	}
}