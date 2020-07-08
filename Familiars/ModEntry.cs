using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.IO;
using Object = StardewValley.Object;

namespace Familiars
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetLoader
	{
		public static ModEntry context;
		public static ModConfig Config;
		public static IModHelper SHelper;
		public static IMonitor SMonitor;
		public static Multiplayer mp;
        internal static IJsonAssetsApi JsonAssets;
		public static int BatFamiliarEgg = -1;
		public static int DustFamiliarEgg = -1;
		public static int DinoFamiliarEgg = -1;

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
			mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

			FamiliarsPatches.Initialize(Monitor, Helper, Config);
			FamiliarsHelperEvents.Initialize(Monitor, Helper, Config);

			Helper.ConsoleCommands.Add("DispelFamiliars", "Dispel all familiars.", new Action<string, string[]>(DispelFamiliars));
			if (Config.IAmAStinkyCheater)
            {
				Helper.ConsoleCommands.Add("DF", "Dispel all familiars.", new System.Action<string, string[]>(DispelFamiliars));
				Helper.ConsoleCommands.Add("CallFamiliar", "Call a familiar. Usage: CallFamiliar <familiarType>", new System.Action<string, string[]>(CallFamiliar));
				Helper.ConsoleCommands.Add("CF", "Call a familiar. Usage: CF <familiarType>", new System.Action<string, string[]>(CallFamiliar));
			}

			Helper.Events.GameLoop.GameLaunched += FamiliarsHelperEvents.GameLoop_GameLaunched;
			Helper.Events.GameLoop.SaveLoaded += FamiliarsHelperEvents.GameLoop_SaveLoaded;
			Helper.Events.Player.Warped += Player_Warped;

			var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.drawAboveFrontLayer)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.GameLocation_drawAboveFrontLayer_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(Object), nameof(Object.performObjectDropInAction)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.Object_performObjectDropInAction_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(Object), nameof(Object.minutesElapsed)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.Object_minutesElapsed_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.GameLocation_checkAction_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(Utility), nameof(Utility.checkForCharacterInteractionAtTile)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.Utility_checkForCharacterInteractionAtTile_Postfix))
			);
		}

		private void DispelFamiliars(string arg1, string[] arg2)
		{
			if (Game1.player == null || Game1.player.currentLocation == null)
				return;

			for (int i = Game1.player.currentLocation.characters.Count - 1; i >= 0; i--)
			{
				NPC npc = Game1.player.currentLocation.characters[i];
				if (npc is Familiar)
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
				if (npc is Familiar)
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

		private void CallFamiliar(string arg1, string[] arg2)
		{
			if (Game1.player == null || Game1.player.currentLocation == null)
				return;

			switch (arg2[0].ToLower())
			{
				case "dust":
					Game1.player.currentLocation.characters.Add(new DustSpriteFamiliar(Game1.player.position, Game1.player));
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
				Monitor.Log($"can load familiar {asset.AssetName}");
				return true;
		}
		if (asset.AssetName.StartsWith("Monsters"))
		{
				Monitor.Log($"can load Monster {asset.AssetName}");
				return true;
		}

		return false;
	}

		/// <summary>Load a matched asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public T Load<T>(IAssetInfo asset)
		{
			if (asset.AssetName.StartsWith("Monsters"))
			{
				string path = Path.Combine("Characters",asset.AssetName);
				return (T)(object)Helper.Content.Load<Texture2D>(path,ContentSource.GameContent);
			}
			string name = asset.AssetName.Replace("Characters\\Monsters\\", "").Replace("Characters/Monsters/", "");
			return (T)(object)Helper.Content.Load<Texture2D>($"assets/{name}.png");
		}
	}
}