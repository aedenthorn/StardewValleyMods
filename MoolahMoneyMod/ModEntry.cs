using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace MoolahMoneyMod
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		const string moolahKey = "aedenthorn.MoolahMoneyMod/moocha";
		const string totalMoolahEarnedKey = "aedenthorn.MoolahMoneyMod/totalMoolahEarned";
		private static readonly PerScreen<MoneyDialData> moneyDialData = new();
		private static readonly PerScreen<List<MoneyDialData>> moneyDialDataList = new();
		private static readonly PerScreen<Item[]> shippingBin = new();
		private static readonly PerScreen<List<BigInteger>> categoryTotals = new();
		private static readonly PerScreen<Dictionary<Item, BigInteger>> itemValues = new();
		private static readonly PerScreen<Dictionary<Item, BigInteger>> singleItemValues = new();

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
			helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.PropertySetter(typeof(Farmer), nameof(Farmer._money)),
					prefix: new HarmonyMethod(typeof(Farmer__money_Setter_Patch), nameof(Farmer__money_Setter_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addUnearnedMoney)),
					prefix: new HarmonyMethod(typeof(Farmer_addUnearnedMoney_Patch), nameof(Farmer_addUnearnedMoney_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), "_newDayAfterFade"),
					prefix: new HarmonyMethod(typeof(Game1_newDayAfterFade_Patch), nameof(Game1_newDayAfterFade_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox)),
					prefix: new HarmonyMethod(typeof(DayTimeMoneyBox_drawMoneyBox_Patch), nameof(DayTimeMoneyBox_drawMoneyBox_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(InventoryPage), nameof(InventoryPage.draw), new Type[] { typeof(SpriteBatch) }),
					transpiler: new HarmonyMethod(typeof(InventoryPage_draw_Patch), nameof(InventoryPage_draw_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(MoneyDial), nameof(MoneyDial.draw)),
					prefix: new HarmonyMethod(typeof(MoneyDial_draw_Patch), nameof(MoneyDial_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Constructor(typeof(ShippingMenu), new Type[] { typeof(IList<Item>) }),
					prefix: new HarmonyMethod(typeof(ShippingMenu_Patch), nameof(ShippingMenu_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ShippingMenu), nameof(ShippingMenu.draw), new Type[] { typeof(SpriteBatch) }),
					postfix: new HarmonyMethod(typeof(ShippingMenu_draw_Patch), nameof(ShippingMenu_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object) }),
					prefix: new HarmonyMethod(typeof(LocalizedContentManager_LoadString_Patch), nameof(LocalizedContentManager_LoadString_Patch.Prefix1))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object), typeof(object) }),
					prefix: new HarmonyMethod(typeof(LocalizedContentManager_LoadString_Patch), nameof(LocalizedContentManager_LoadString_Patch.Prefix2))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object), typeof(object), typeof(object) }),
					prefix: new HarmonyMethod(typeof(LocalizedContentManager_LoadString_Patch), nameof(LocalizedContentManager_LoadString_Patch.Prefix3))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object[]) }),
					prefix: new HarmonyMethod(typeof(LocalizedContentManager_LoadString_Patch), nameof(LocalizedContentManager_LoadString_Patch.Prefix4))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Utility), nameof(Utility.getNumberWithCommas), new Type[] { typeof(int) }),
					prefix: new HarmonyMethod(typeof(Utility_getNumberWithCommas_Patch), nameof(Utility_getNumberWithCommas_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>), typeof(Texture2D), typeof(Rectangle?), typeof(Color?), typeof(Color?), typeof(float), typeof(int), typeof(int) }),
					transpiler: new HarmonyMethod(typeof(IClickableMenu_drawHoverText_Patch), nameof(IClickableMenu_drawHoverText_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.draw), new Type[] { typeof(SpriteBatch) }),
					transpiler: new HarmonyMethod(typeof(ShopMenu_draw_Patch), nameof(ShopMenu_draw_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(LoadGameMenu.SaveFileSlot), "drawSlotMoney", new Type[] { typeof(SpriteBatch), typeof(int) }),
					transpiler: new HarmonyMethod(typeof(SaveFileSlot_drawSlotMoney_Patch), nameof(SaveFileSlot_drawSlotMoney_Patch.Transpiler))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			shippingBin.Value = Array.Empty<Item>();
			moneyDialData.Value = new();
		}

		public override object GetApi()
		{
			return new MoolahMoneyModAPI();
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			moneyDialData.Value = new();

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
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Separator.Name"),
					getValue: () => Config.Separator,
					setValue: value => Config.Separator = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SeparatorX.Name"),
					getValue: () => Config.SeparatorX,
					setValue: value => Config.SeparatorX = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SeparatorY.Name"),
					getValue: () => Config.SeparatorY,
					setValue: value => Config.SeparatorY = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SeparatorInterval.Name"),
					getValue: () => Config.SeparatorInterval,
					setValue: value => Config.SeparatorInterval = Math.Max(1, value)
				);
			}
		}
	}
}
