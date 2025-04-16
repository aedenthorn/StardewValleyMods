using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Objects;

namespace OverworldChests
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private const string modKey = "aedenthorn.OverworldChests";
		private const string modCoinKey = "aedenthorn.OverworldChests/Coin";
		private static IAdvancedLootFrameworkApi advancedLootFrameworkApi = null;
		private static List<object> treasuresList = new();
		private static Random random;
		private static int daysSinceLastSpawn;
		private static readonly Color[] tintColors = new Color[]
		{
			Color.DarkGray,
			Color.Brown,
			Color.Silver,
			Color.Gold,
			Color.Purple,
		};

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
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(Chest_draw_Patch), nameof(Chest_draw_Patch.Prefix))
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

		private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
		{
			daysSinceLastSpawn = int.MaxValue;
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			if (daysSinceLastSpawn >= Config.RespawnInterval)
			{
				RespawnChests();
				daysSinceLastSpawn = 0;
			}
			daysSinceLastSpawn++;
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			random = new Random();
			RegisterConsoleCommands();

			// Get Mobile Advanced Loot Framework's API
			advancedLootFrameworkApi = Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
			if (advancedLootFrameworkApi is not null)
			{
				Monitor.Log($"Loaded AdvancedLootFramework API");
				UpdateTreasuresList();
				Monitor.Log($"Got {treasuresList.Count} possible treasures");
			}

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
					setValue: value => {
						if (Config.ModEnabled != value)
						{
							if (value)
							{
								SpawnChests(true);
							}
							else
							{
								RemoveChests(true);
							}
						}
						Config.ModEnabled = value;
					}
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeIndoorLocations.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.IncludeIndoorLocations.Tooltip"),
					getValue: () => Config.IncludeIndoorLocations,
					setValue: value => Config.IncludeIndoorLocations = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.RespawnInterval.Name"),
					getValue: () => Config.RespawnInterval,
					setValue: value => {
						Config.RespawnInterval = Math.Max(1, value);
					}
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.RoundNumberOfChestsUp.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.RoundNumberOfChestsUp.Tooltip"),
					getValue: () => Config.RoundNumberOfChestsUp,
					setValue: value => Config.RoundNumberOfChestsUp = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ChestDensity.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ChestDensity.Tooltip"),
					getValue: () => Config.ChestDensity,
					setValue: value => Config.ChestDensity = value,
					min: 0f,
					max: 1f,
					interval: 0.001f
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.RarityChance.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.RarityChance.Tooltip"),
					getValue: () => Config.RarityChance,
					setValue: value => Config.RarityChance = value,
					min: 0f,
					max: 1f,
					interval: 0.01f
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MaxItems.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.MaxItems.Tooltip"),
					getValue: () => Config.MaxItems,
					setValue: value => Config.MaxItems = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Tooltip"),
					getValue: () => Config.ItemsBaseMaxValue,
					setValue: value => Config.ItemsBaseMaxValue = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Mult.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Mult.Tooltip"),
					getValue: () => Config.Mult,
					setValue: value => Config.Mult = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MinItemValue.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.MinItemValue.Tooltip"),
					getValue: () => Config.MinItemValue,
					setValue: value => Config.MinItemValue = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MaxItemValue.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.MaxItemValue.Tooltip"),
					getValue: () => Config.MaxItemValue,
					setValue: value => Config.MaxItemValue = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Tooltip"),
					getValue: () => Config.CoinBaseMin,
					setValue: value => Config.CoinBaseMin = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Tooltip"),
					getValue: () => Config.CoinBaseMax,
					setValue: value => Config.CoinBaseMax = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncreaseRate.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.IncreaseRate.Tooltip"),
					getValue: () => Config.IncreaseRate,
					setValue: value => Config.IncreaseRate = value
				);
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.ItemListChances.Text"),
					tooltip: () => SHelper.Translation.Get("GMCM.ItemListChances.Tooltip")
				);
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
				gmcm.AddNumberOption(
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
		}
	}
}
