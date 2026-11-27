using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace AdvancedCooking
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

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

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.populateClickableComponentList)),
					postfix: new HarmonyMethod(typeof(IClickableMenu_populateClickableComponentList_Patch), nameof(IClickableMenu_populateClickableComponentList_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Constructor(typeof(CraftingPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(List<IInventory>) }),
					prefix: new HarmonyMethod(typeof(CraftingPage_Patch), nameof(CraftingPage_Patch.Prefix)),
					postfix: new HarmonyMethod(typeof(CraftingPage_Patch), nameof(CraftingPage_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.receiveLeftClick)),
					prefix: new HarmonyMethod(typeof(CraftingPage_receiveLeftClick_Patch), nameof(CraftingPage_receiveLeftClick_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.receiveRightClick)),
					prefix: new HarmonyMethod(typeof(CraftingPage_receiveRightClick_Patch), nameof(CraftingPage_receiveRightClick_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.gameWindowSizeChanged)),
					postfix: new HarmonyMethod(typeof(CraftingPage_gameWindowSizeChanged_Patch), nameof(CraftingPage_gameWindowSizeChanged_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.emergencyShutDown)),
					prefix: new HarmonyMethod(typeof(CraftingPage_emergencyShutDown_Patch), nameof(CraftingPage_emergencyShutDown_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.drawDialogueBox), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) }),
					postfix: new HarmonyMethod(typeof(Game1_drawDialogueBox_Patch), nameof(Game1_drawDialogueBox_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
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

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AllowUnknownRecipes.Name"),
				getValue: () => Config.AllowUnknownRecipes,
				setValue: value => Config.AllowUnknownRecipes = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.LearnUnknownRecipes.Name"),
				getValue: () => Config.LearnUnknownRecipes,
				setValue: value => Config.LearnUnknownRecipes = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CookAllModKey.Name"),
				getValue: () => Config.CookAllModKey,
				setValue: value => Config.CookAllModKey = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.HoldCookedItem.Name"),
				getValue: () => Config.HoldCookedItem,
				setValue: value => Config.HoldCookedItem = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ConsumeExtraIngredientsOnSucceed.Name"),
				getValue: () => Config.ConsumeExtraIngredientsOnSucceed,
				setValue: value => Config.ConsumeExtraIngredientsOnSucceed = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ConsumeIngredientsOnFail.Name"),
				getValue: () => Config.ConsumeIngredientsOnFail,
				setValue: value => Config.ConsumeIngredientsOnFail = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.GiveTrashOnFail.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.GiveTrashOnFail.Tooltip"),
				getValue: () => Config.GiveTrashOnFail,
				setValue: value => Config.GiveTrashOnFail = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowProductInfo.Name"),
				getValue: () => Config.ShowProductInfo,
				setValue: value => Config.ShowProductInfo = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowCookTooltip.Name"),
				getValue: () => Config.ShowCookTooltip,
				setValue: value => Config.ShowCookTooltip = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowProductsInTooltip.Name"),
				getValue: () => Config.ShowProductsInTooltip,
				setValue: value => Config.ShowProductsInTooltip = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxItemsInTooltip.Name"),
				getValue: () => Config.MaxItemsInTooltip,
				setValue: value => Config.MaxItemsInTooltip = Math.Max(1, Math.Min(value, 100))
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.YOffset.Name"),
				getValue: () => Config.YOffset,
				setValue: value => Config.YOffset = value
			);
		}
	}
}
