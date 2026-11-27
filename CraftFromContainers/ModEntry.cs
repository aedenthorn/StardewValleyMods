using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace CraftAndBuildFromContainers
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

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.Display.MenuChanged += Display_MenuChanged;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Inventory), nameof(Inventory.ContainsId), new Type[] { typeof(string) }),
					postfix: new HarmonyMethod(typeof(Inventory_ContainsId_Patch), nameof(Inventory_ContainsId_Patch.Postfix1))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Inventory), nameof(Inventory.ContainsId), new Type[] { typeof(string), typeof(int) }),
					postfix: new HarmonyMethod(typeof(Inventory_ContainsId_Patch), nameof(Inventory_ContainsId_Patch.Postfix2))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Inventory), nameof(Inventory.CountId)),
					postfix: new HarmonyMethod(typeof(Inventory_CountId_Patch), nameof(Inventory_CountId_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.doesFarmerHaveIngredientsInInventory)),
					postfix: new HarmonyMethod(typeof(CraftingRecipe_doesFarmerHaveIngredientsInInventory_Patch), nameof(CraftingRecipe_doesFarmerHaveIngredientsInInventory_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory)),
					postfix: new HarmonyMethod(typeof(CraftingRecipe_DoesFarmerHaveAdditionalIngredientsInInventory_Patch), nameof(CraftingRecipe_DoesFarmerHaveAdditionalIngredientsInInventory_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.DoesFarmerHaveEnoughResourcesToBuild)),
					postfix: new HarmonyMethod(typeof(CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_Patch), nameof(CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.drawRecipeDescription)),
					prefix: new HarmonyMethod(typeof(CraftingRecipe_drawRecipeDescription_Patch), nameof(CraftingRecipe_drawRecipeDescription_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.ConsumeTradeItem)),
					prefix: new HarmonyMethod(typeof(ShopMenu_ConsumeTradeItem_Patch), nameof(ShopMenu_ConsumeTradeItem_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
					prefix: new HarmonyMethod(typeof(CraftingRecipe_consumeIngredients_Patch), nameof(CraftingRecipe_consumeIngredients_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.ConsumeAdditionalIngredients)),
					prefix: new HarmonyMethod(typeof(CraftingRecipe_ConsumeAdditionalIngredients_Patch), nameof(CraftingRecipe_ConsumeAdditionalIngredients_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.ConsumeResources)),
					prefix: new HarmonyMethod(typeof(CarpenterMenu_ConsumeResources_Patch), nameof(CarpenterMenu_ConsumeResources_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
		{
			cachedContainers = null;
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
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
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ToggleButton.Name"),
					getValue: () => Config.ToggleButton,
					setValue: value => Config.ToggleButton = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.EnableForShopTrading.Name"),
					getValue: () => Config.EnableForShopTrading,
					setValue: value => Config.EnableForShopTrading = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.EnableForCrafting.Name"),
					getValue: () => Config.EnableForCrafting,
					setValue: value => Config.EnableForCrafting = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.EnableForBuilding.Name"),
					getValue: () => Config.EnableForBuilding,
					setValue: value => Config.EnableForBuilding = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.EnableEverywhere.Name"),
					getValue: () => Config.EnableEverywhere,
					setValue: value => Config.EnableEverywhere = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeFridge.Name"),
					getValue: () => Config.IncludeFridge,
					setValue: value => Config.IncludeFridge = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeMiniFridges.Name"),
					getValue: () => Config.IncludeMiniFridges,
					setValue: value => Config.IncludeMiniFridges = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeShippingBin.Name"),
					getValue: () => Config.IncludeShippingBin,
					setValue: value => Config.IncludeShippingBin = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.UnrestrictedShippingBin.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.UnrestrictedShippingBin.Tooltip"),
					getValue: () => Config.UnrestrictedShippingBin,
					setValue: value => Config.UnrestrictedShippingBin = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeMiniShippingBins.Name"),
					getValue: () => Config.IncludeMiniShippingBins,
					setValue: value => Config.IncludeMiniShippingBins = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeJunimoChests.Name"),
					getValue: () => Config.IncludeJunimoChests,
					setValue: value => Config.IncludeJunimoChests = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeAutoGrabbers.Name"),
					getValue: () => Config.IncludeAutoGrabbers,
					setValue: value => Config.IncludeAutoGrabbers = value
				);
			}
		}

	}
}
