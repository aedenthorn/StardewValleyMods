using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using Object = StardewValley.Object;

namespace AnimatedParrotAndPerch
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static IAdvancedLootFrameworkApi advancedLootFrameworkApi = null;
		private static List<object> giftList = new();
		private static readonly Dictionary<string, int> possibleGifts = new()
		{
			{ "BasicObject", 50 },
			{ "Fish", 10 },
			{ "Cooking", 20 },
			{ "Relic", 5 }
		};
		private static readonly string[] fertilizers = new string[]
		{
			"368",
			"369",
			"919"
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
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.Player.Warped += Player_Warped;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

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
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.Name.IsEquivalentTo("Data/CraftingRecipes"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add("aedenthorn.AnimatedParrotAndPerch_JungleParrotPerch", "388 20 771 20/Home/aedenthorn.AnimatedParrotAndPerch_JungleParrotPerch/true/default/");
					data.Add("aedenthorn.AnimatedParrotAndPerch_StoneParrotPerch", "390 20/Home/aedenthorn.AnimatedParrotAndPerch_StoneParrotPerch/true/default/");
					data.Add("aedenthorn.AnimatedParrotAndPerch_WoodenParrotPerch", "388 20/Home/aedenthorn.AnimatedParrotAndPerch_WoodenParrotPerch/true/default/");
				});
			}
			if (e.Name.IsEquivalentTo("Data/BigCraftables"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, BigCraftableData> data = asset.AsDictionary<string, BigCraftableData>().Data;

					data.Add("aedenthorn.AnimatedParrotAndPerch_JungleParrotPerch", new BigCraftableData()
					{
						Name = "Jungle Parrot Perch",
						DisplayName = "[aedenthorn.AnimatedParrotAndPerch_i18n item.jungle-parrot-perch.name]",
						Description = "[aedenthorn.AnimatedParrotAndPerch_i18n item.jungle-parrot-perch.description]",
						Texture = SHelper.ModContent.GetInternalAssetName("assets/jungleParrotPerch").Name,
					});
					data.Add("aedenthorn.AnimatedParrotAndPerch_StoneParrotPerch", new BigCraftableData()
					{
						Name = "Stone Parrot Perch",
						DisplayName = "[aedenthorn.AnimatedParrotAndPerch_i18n item.stone-parrot-perch.name]",
						Description = "[aedenthorn.AnimatedParrotAndPerch_i18n item.stone-parrot-perch.description]",
						Texture = SHelper.ModContent.GetInternalAssetName("assets/stoneParrotPerch").Name,
					});
					data.Add("aedenthorn.AnimatedParrotAndPerch_WoodenParrotPerch", new BigCraftableData()
					{
						Name = "Wooden Parrot Perch",
						DisplayName = "[aedenthorn.AnimatedParrotAndPerch_i18n item.wooden-parrot-perch.name]",
						Description = "[aedenthorn.AnimatedParrotAndPerch_i18n item.wooden-parrot-perch.description]",
						Texture = SHelper.ModContent.GetInternalAssetName("assets/woodenParrotPerch").Name,
					});
				});
			}
		}

		private void Player_Warped(object sender, WarpedEventArgs e)
		{
			ShowParrots(e.NewLocation);
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			ShowParrots(Game1.player.currentLocation);
		}

		public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			TokensUtility.Register();
			advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
			if (advancedLootFrameworkApi != null)
			{
				Monitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
				giftList = advancedLootFrameworkApi.LoadPossibleTreasures(possibleGifts.Keys.ToArray(), -1, 100);
				Monitor.Log($"Got {giftList.Count} possible treasures");
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

			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.DropGiftChance.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.DropGiftChance.Tooltip"),
				getValue: () => Config.DropGiftChance * 100,
				setValue: value => Config.DropGiftChance = value / 100,
				min: 0,
				max: 100,
				interval: 1
			);
		}
	}
}
