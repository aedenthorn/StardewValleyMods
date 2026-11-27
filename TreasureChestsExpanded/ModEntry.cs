using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley.Objects;

namespace TreasureChestsExpanded
{
	public partial class ModEntry : Mod
	{
		private const string modKey = "aedenthorn.TreasureChestsExpanded";
		private const string modCoinKey = "aedenthorn.TreasureChestsExpanded/Coin";
		private static ModConfig Config;
		private static IMonitor SMonitor;
		private static IModHelper SHelper;
		private static List<object> treasuresList = new();
		private static IAdvancedLootFrameworkApi advancedLootFrameworkApi = null;
		private static readonly Color[] tintColors = new Color[]
		{
			Color.DarkGray,
			Color.Brown,
			Color.Silver,
			Color.Gold,
			Color.Purple,
		};

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			SMonitor = Monitor;
			SHelper = Helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(Chest_draw_Patch), nameof(Chest_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(MineShaft), "addLevelChests"),
					postfix: new HarmonyMethod(typeof(MineShaft_addLevelChests_Patch), nameof(MineShaft_addLevelChests_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.loadLevel)),
					transpiler: new HarmonyMethod(typeof(MineShaft_loadLevel_Patch), nameof(MineShaft_loadLevel_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Chest), nameof(Chest.ShowMenu), Array.Empty<Type>()),
					postfix: new HarmonyMethod(typeof(Chest_showMenu_Patch), nameof(Chest_showMenu_Patch.Postfix))
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
			advancedLootFrameworkApi = Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
			if (advancedLootFrameworkApi != null)
			{
				Monitor.Log($"Loaded AdvancedLootFramework API", LogLevel.Debug);
				UpdateTreasuresList();
				Monitor.Log($"Got {treasuresList.Count} possible treasures");
			}

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
				getValue: () => Config.EnableMod,
				setValue: value => Config.EnableMod = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ChanceForTreasureRoom.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ChanceForTreasureRoom.Tooltip"),
				getValue: () => Config.ChanceForTreasureRoom,
				setValue: value => Config.ChanceForTreasureRoom = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxItems.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxItems.Tooltip"),
				getValue: () => Config.MaxItems,
				setValue: value => Config.MaxItems = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Tooltip"),
				getValue: () => Config.ItemsBaseMaxValue,
				setValue: value => Config.ItemsBaseMaxValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MinItemValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MinItemValue.Tooltip"),
				getValue: () => Config.MinItemValue,
				setValue: value => Config.MinItemValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxItemValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxItemValue.Tooltip"),
				getValue: () => Config.MaxItemValue,
				setValue: value => Config.MaxItemValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Tooltip"),
				getValue: () => Config.CoinBaseMin,
				setValue: value => Config.CoinBaseMin = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Tooltip"),
				getValue: () => Config.CoinBaseMax,
				setValue: value => Config.CoinBaseMax = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.IncreaseRate.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.IncreaseRate.Tooltip"),
				getValue: () => Config.IncreaseRate,
				setValue: value => Config.IncreaseRate = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.ItemListChances.Text"),
				tooltip: () => SHelper.Translation.Get("GMCM.ItemListChances.Tooltip")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesHat.Name"),
				getValue: () => Config.ItemListChances["Hat"],
				setValue: value => {
					Config.ItemListChances["Hat"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesShirt.Name"),
				getValue: () => Config.ItemListChances["Shirt"],
				setValue: value => {
					Config.ItemListChances["Shirt"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesPants.Name"),
				getValue: () => Config.ItemListChances["Pants"],
				setValue: value => {
					Config.ItemListChances["Pants"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBoots.Name"),
				getValue: () => Config.ItemListChances["Boots"],
				setValue: value => {
					Config.ItemListChances["Boots"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesMeleeWeapon.Name"),
				getValue: () => Config.ItemListChances["MeleeWeapon"],
				setValue: value => {
					Config.ItemListChances["MeleeWeapon"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesRing.Name"),
				getValue: () => Config.ItemListChances["Ring"],
				setValue: value => {
					Config.ItemListChances["Ring"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesRelic.Name"),
				getValue: () => Config.ItemListChances["Relic"],
				setValue: value => {
					Config.ItemListChances["Relic"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesMineral.Name"),
				getValue: () => Config.ItemListChances["Mineral"],
				setValue: value => {
					Config.ItemListChances["Mineral"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesCooking.Name"),
				getValue: () => Config.ItemListChances["Cooking"],
				setValue: value => {
					Config.ItemListChances["Cooking"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesFish.Name"),
				getValue: () => Config.ItemListChances["Fish"],
				setValue: value => {
					Config.ItemListChances["Fish"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesSeed.Name"),
				getValue: () => Config.ItemListChances["Seed"],
				setValue: value => {
					Config.ItemListChances["Seed"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBasicObject.Name"),
				getValue: () => Config.ItemListChances["BasicObject"],
				setValue: value => {
					Config.ItemListChances["BasicObject"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBigCraftable.Name"),
				getValue: () => Config.ItemListChances["BigCraftable"],
				setValue: value => {
					Config.ItemListChances["BigCraftable"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
		}

		private static void UpdateTreasuresList()
		{
			treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
		}
	}
}
