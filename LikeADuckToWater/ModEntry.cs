using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace LikeADuckToWater
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;

		public const string swamTodayKey = "aedenthorn.LikeADuckToWater/swamToday";
		public const string checkedIndexKey = "aedenthorn.LikeADuckToWater/swamToday";

		internal static List<Vector2> pickedTiles = new();
		internal static Dictionary<FarmAnimal, Stack<HopInfo>> ducksToCheck = new();
		private static Dictionary<GameLocation, Dictionary<Vector2, List<HopInfo>>> hopTileDictionary = new();
		private static readonly List<int> waterBuildingTiles = new()
		{
			209,
			628,
			629,
			734,
			759,
			1293,
			1318
		};

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.updatePerTenMinutes)),
					postfix: new HarmonyMethod(typeof(FarmAnimal_updatePerTenMinutes_Patch), nameof(FarmAnimal_updatePerTenMinutes_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.MovePosition)),
					prefix: new HarmonyMethod(typeof(FarmAnimal_MovePosition_Patch), nameof(FarmAnimal_MovePosition_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.MovePosition)),
					postfix: new HarmonyMethod(typeof(FarmAnimal_MovePosition_Patch), nameof(FarmAnimal_MovePosition_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.Eat)),
					postfix: new HarmonyMethod(typeof(FarmAnimal_Eat_Patch), nameof(FarmAnimal_Eat_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.pet)),
					postfix: new HarmonyMethod(typeof(FarmAnimal_pet_Patch), nameof(FarmAnimal_pet_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.HandleCollision), new Type[] { typeof(Rectangle) }),
					prefix: new HarmonyMethod(typeof(FarmAnimal_HandleCollision_Patch), nameof(FarmAnimal_HandleCollision_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.HandleCollision), new Type[] { typeof(Rectangle) }),
					postfix: new HarmonyMethod(typeof(FarmAnimal_HandleCollision_Patch), nameof(FarmAnimal_HandleCollision_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.HandleCollision), new Type[] { typeof(Rectangle) }),
					transpiler: new HarmonyMethod(typeof(FarmAnimal_HandleCollision_Patch), nameof(FarmAnimal_HandleCollision_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }),
					prefix: new HarmonyMethod(typeof(GameLocation_isCollidingPosition_Patch), nameof(GameLocation_isCollidingPosition_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
					postfix: new HarmonyMethod(typeof(FarmAnimal_dayUpdate_Patch), nameof(FarmAnimal_dayUpdate_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.HandleHop)),
					postfix: new HarmonyMethod(typeof(FarmAnimal_HandleHop_Patch), nameof(FarmAnimal_HandleHop_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isOpenWater)),
					prefix: new HarmonyMethod(typeof(GameLocation_isOpenWater_Patch), nameof(GameLocation_isOpenWater_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !ducksToCheck.Any())
			{
				pickedTiles.Clear();
				return;
			}

			if (FarmAnimal.NumPathfindingThisTick >= FarmAnimal.MaxPathfindingPerTick || Game1.random.NextDouble() > Config.ChancePerTick)
				return;

			foreach(FarmAnimal farmAnimal in ducksToCheck.Keys.ToArray())
			{
				if (farmAnimal.modData.ContainsKey(swamTodayKey) || ducksToCheck[farmAnimal].Count == 0)
				{
					ducksToCheck.Remove(farmAnimal);
					continue;
				}
				if(CheckDuck(farmAnimal, ducksToCheck[farmAnimal].Pop()))
				{
					ducksToCheck.Remove(farmAnimal);
				}
				break;
			}
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			hopTileDictionary = new();
			foreach (GameLocation location in Game1.locations)
			{
				if (location.IsBuildableLocation())
				{
					hopTileDictionary.Add(location, new());
					RebuildHopSpots(location);
				}
			}
		}

		public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
				name: () => SHelper.Translation.Get("GMCM.EatBeforeSwimming.Name"),
				getValue: () => Config.EatBeforeSwimming,
				setValue: value => Config.EatBeforeSwimming = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SwimAfterAutoPet.Name"),
				getValue: () => Config.SwimAfterAutoPet,
				setValue: value => Config.SwimAfterAutoPet = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxDistance.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxDistance.Tooltip"),
				getValue: () => Config.MaxDistance,
				setValue: value => Config.MaxDistance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ChancePerTick.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ChancePerTick.Tooltip"),
				getValue: () => Config.ChancePerTick,
				setValue: value => Config.ChancePerTick = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FriendshipGain.Name"),
				getValue: () => Config.FriendshipGain,
				setValue: value => Config.FriendshipGain = value
			);
		}
	}
}
