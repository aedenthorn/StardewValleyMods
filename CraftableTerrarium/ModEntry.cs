using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace CraftableTerrarium
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static string assetDirectory;
		private static string textureFile;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.Player.Warped += Player_Warped;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			assetDirectory = Path.Combine(SHelper.DirectoryPath, "assets");
			textureFile = Path.Combine(assetDirectory, "terrarium.png");

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Constructor(typeof(CraftingRecipe), new Type[] { typeof(string), typeof(bool) }),
					postfix: new HarmonyMethod(typeof(CraftingRecipe_Constructor_Patch), nameof(CraftingRecipe_Constructor_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.GetItemData)),
					prefix: new HarmonyMethod(typeof(CraftingRecipe_GetItemData_Patch), nameof(CraftingRecipe_GetItemData_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingPage), "spaceOccupied"),
					postfix: new HarmonyMethod(typeof(CraftingPage_spaceOccupied_Patch), nameof(CraftingPage_spaceOccupied_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingPage), "layoutRecipes"),
					transpiler: new HarmonyMethod(typeof(CraftingPage_layoutRecipes_Patch), nameof(CraftingPage_layoutRecipes_Patch.Transpiler))
					{
						after = new string[] { "spacechase0.SpaceCore" }
					}
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), "loadDescription"),
					prefix: new HarmonyMethod(typeof(Furniture_loadDescription_Patch), nameof(Furniture_loadDescription_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
					postfix: new HarmonyMethod(typeof(Object_placementAction_Patch), nameof(Object_placementAction_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
					postfix: new HarmonyMethod(typeof(Object_performRemoveAction_Patch), nameof(Object_performRemoveAction_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
					prefix: new HarmonyMethod(typeof(Object_checkForAction_Patch), nameof(Object_checkForAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(SebsFrogs), nameof(SebsFrogs.update)),
					transpiler: new HarmonyMethod(typeof(SebsFrogs_update_Patch), nameof(SebsFrogs_update_Patch.Transpiler))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add("aedenthorn.CraftableTerrarium_Terrarium", "338 20 390 10 771 10/Home/aedenthorn.CraftableTerrarium_Terrarium/false/default");
				});
			}
			if (e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add("aedenthorn.CraftableTerrarium_Terrarium", $"Terrarium/decor/3 3/3 2/1/0/2/[aedenthorn.CraftableTerrarium_i18n item.terrarium.name]/0/{SHelper.ModContent.GetInternalAssetName("assets/terrarium").Name.Replace('/', '\\')}");
				});
			}
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			ShowFrogs(Game1.player.currentLocation);
		}

		private static void GameLoop_OneTickAfterGameLaunched(object sender, UpdateTickedEventArgs e)
		{
			SHelper.Events.GameLoop.UpdateTicked -= GameLoop_OneTickAfterGameLaunched;
			SHelper.Events.GameLoop.UpdateTicked += GameLoop_TwoTicksAfterGameLaunched;
		}

		private static void GameLoop_TwoTicksAfterGameLaunched(object sender, UpdateTickedEventArgs e)
		{
			CreateTextureFile();
			SHelper.Events.GameLoop.UpdateTicked -= GameLoop_TwoTicksAfterGameLaunched;
		}

		public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			TokensUtility.Register();
			SHelper.Events.GameLoop.UpdateTicked += GameLoop_OneTickAfterGameLaunched;

			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => Helper.WriteConfig(Config)
				);

				// Main section
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Frogs.Name"),
					getValue: () => Config.Frogs,
					setValue: value => Config.Frogs = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Sound.Name"),
					getValue: () => Config.Sound,
					setValue: value => Config.Sound = value
				);
			}
		}

		private void Player_Warped(object sender, WarpedEventArgs e)
		{
			ShowFrogs(e.NewLocation);
		}
	}
}
