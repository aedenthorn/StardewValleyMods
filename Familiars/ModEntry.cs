using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Object = StardewValley.Object;

namespace Familiars
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetLoader, IAssetEditor
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
		public static int JunimoFamiliarEgg = -1;
        internal static bool receivedJunimoEggToday = false;

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
			FamiliarsUtils.Initialize(Monitor, Helper, Config);

			Helper.ConsoleCommands.Add("DispelFamiliars", "Dispel all familiars.", new Action<string, string[]>(DispelFamiliars));
			Helper.ConsoleCommands.Add("DF", "Dispel all familiars.", new System.Action<string, string[]>(DispelFamiliars));
			if (Config.IAmAStinkyCheater)
            {
				Helper.ConsoleCommands.Add("CallFamiliar", "Call a familiar. Usage: CallFamiliar <familiarType>", new System.Action<string, string[]>(CallFamiliar));
				Helper.ConsoleCommands.Add("CF", "Call a familiar. Usage: CF <familiarType>", new System.Action<string, string[]>(CallFamiliar));
			}

			Helper.Events.GameLoop.GameLaunched += FamiliarsHelperEvents.GameLoop_GameLaunched;
			Helper.Events.GameLoop.SaveLoaded += FamiliarsHelperEvents.GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += FamiliarsHelperEvents.GameLoop_Saving;
            Helper.Events.GameLoop.DayStarted += FamiliarsHelperEvents.GameLoop_DayStarted;

			Helper.Events.Player.Warped += FamiliarsHelperEvents.Player_Warped;

			var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.drawAboveFrontLayer)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.GameLocation_drawAboveFrontLayer_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(Object), nameof(Object.performObjectDropInAction)),
				prefix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.Object_performObjectDropInAction_Prefix))
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
				original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.GameLocation_performTouchAction_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(Utility), nameof(Utility.checkForCharacterInteractionAtTile)),
				postfix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.Utility_checkForCharacterInteractionAtTile_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(NPC), nameof(NPC.isVillager)),
				prefix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.NPC_isVillager_Prefix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(Character), nameof(Character.checkForFootstep)),
				prefix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.Character_checkForFootstep_Prefix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(Bush), "shake"),
				prefix: new HarmonyMethod(typeof(FamiliarsPatches), nameof(FamiliarsPatches.Bush_shake_Prefix))
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

		private void CallFamiliar(string arg1, string[] arg2)
		{
			if (Game1.player == null || Game1.player.currentLocation == null)
				return;

			switch (arg2[0].ToLower())
			{
				case "dust":
					Game1.player.currentLocation.characters.Add(new DustSpriteFamiliar(Game1.player.position, Game1.player.UniqueMultiplayerID));
					break;
				case "dino":
					Game1.player.currentLocation.characters.Add(new DinoFamiliar(Game1.player.position, Game1.player.UniqueMultiplayerID));
					break;
				case "bat":
					Game1.player.currentLocation.characters.Add(new BatFamiliar(Game1.player.position, Game1.player.UniqueMultiplayerID));
					break;
				case "junimo":
					Game1.player.currentLocation.characters.Add(new JunimoFamiliar(Game1.player.position, Game1.player.UniqueMultiplayerID));
					break;
			}
		}

	public bool CanLoad<T>(IAssetInfo asset)
	{
		if (!Config.EnableMod)
			return false;

		if (asset.AssetName.EndsWith("BatFamiliar") || asset.AssetName.EndsWith("DinoFamiliar") || asset.AssetName.EndsWith("DustSpiritFamiliar") || asset.AssetName.EndsWith("JunimoFamiliar"))
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

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Strings\\UI") || asset.AssetNameEquals("Data\\Monsters"))
            {
				return true;
            }
			return false;
        }

		public void Edit<T>(IAssetData asset)
		{
			if (asset.AssetNameEquals("Strings\\UI"))
			{
				var editor = asset.AsDictionary<string, string>();
				editor.Data["Chat_DinoFamiliarEgg"] = Helper.Translation.Get("chat-dino-familiar-egg");
			}
			else if (asset.AssetNameEquals("Data\\Monsters"))
			{
				var editor = asset.AsDictionary<string, string>();

				editor.Data["Junimo"] = "40/6/0/0/false/1000/382 .5 433 .01 336 .001 84 .02 414 .02 97 .005 99 .001/2/.00/4/3/.00/true/2/Junimo";
			}
		}
	}
}