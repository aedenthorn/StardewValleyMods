using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace CraftableButterflyHutches
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static ModEntry context;

		internal static ModConfig Config;
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;

		private static string assetDirectory;
		private static string textureFile;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			SMonitor = Monitor;
			SHelper = Helper;
			context = this;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.Player.Warped += Player_Warped;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			assetDirectory = Path.Combine(SHelper.DirectoryPath, "assets");
			textureFile = Path.Combine(assetDirectory, "butterflyHutch.png");

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
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
			{
				if (Context.IsGameLaunched && !Context.IsWorldReady)
				{
					CreateTextureFile();
				}
			}
			if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add("aedenthorn.CraftableButterflyHutches_ButterflyHutch", $"{Config.HutchCost}/Home/aedenthorn.CraftableButterflyHutches_ButterflyHutch/false/{Config.SkillsRequired}");
				});
			}
			if (e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add("aedenthorn.CraftableButterflyHutches_ButterflyHutch", $"Butterfly Hutch/decor/2 3/2 1/1/0/2/[LocalizedText Strings\\Furniture:ButterflyHutch]/0/{SHelper.ModContent.GetInternalAssetName("assets/butterflyHutch").Name.Replace('/', '\\')}");
				});
			}
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			AddButterflies(Game1.player.currentLocation);
		}

		public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			TokensUtility.Register();

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => Helper.WriteConfig(Config)
			);

			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.HutchCost.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.HutchCost.Tooltip"),
				getValue: () => Config.HutchCost,
				setValue: value => {
					Config.HutchCost = value;
					SHelper.GameContent.InvalidateCache("Data/CraftingRecipes");
				}
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SkillsRequired.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.SkillsRequired.Tooltip"),
				getValue: () => Config.SkillsRequired,
				setValue: value => {
					Config.SkillsRequired = value;
					SHelper.GameContent.InvalidateCache("Data/CraftingRecipes");
				}
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MinDensity.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MinDensity.Tooltip"),
				getValue: () => Config.MinDensity,
				setValue: value => {
					Config.MinDensity = value;
					AddButterflies(Game1.currentLocation);
				},
				min: 0f,
				max: 1f,
				interval: 0.01f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxDensity.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxDensity.Tooltip"),
				getValue: () => Config.MaxDensity,
				setValue: value => {
					Config.MaxDensity = value;
					AddButterflies(Game1.currentLocation);
				},
				min: 0f,
				max: 1f,
				interval: 0.01f
			);
		}

		private void Player_Warped(object sender, WarpedEventArgs e)
		{
			AddButterflies(e.NewLocation);
		}
	}
}
