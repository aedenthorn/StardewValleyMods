using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace AdditionalMineMaps
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;

		internal static string dictPath = "aedenthorn.AdditionalMineMaps/dictionary";
		internal static string mapPathKey = "aedenthorn.AdditionalMineMaps/mapPath";
		internal static string mapTypeKey = "aedenthorn.AdditionalMineMaps/mapType";

		internal static Dictionary<string, MapData> mapDict;
		internal static Dictionary<int, MapData> forcedMapDict = new();

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.loadLevel)),
					prefix: new HarmonyMethod(typeof(MineShaft_loadLevel_Patch), nameof(MineShaft_loadLevel_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.updateMap)),
					prefix: new HarmonyMethod(typeof(GameLocation_updateMap_Patch), nameof(GameLocation_updateMap_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.DeclaredPropertyGetter(typeof(MineShaft), nameof(MineShaft.isQuarryArea)),
					prefix: new HarmonyMethod(typeof(MineShaft_isQuarryArea_Get_Patch), nameof(MineShaft_isQuarryArea_Get_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.DeclaredPropertyGetter(typeof(MineShaft), nameof(MineShaft.isDinoArea)),
					prefix: new HarmonyMethod(typeof(MineShaft_isDinoArea_Get_Patch), nameof(MineShaft_isDinoArea_Get_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.DeclaredPropertyGetter(typeof(MineShaft), nameof(MineShaft.isMonsterArea)),
					prefix: new HarmonyMethod(typeof(MineShaft_isMonsterArea_Get_Patch), nameof(MineShaft_isMonsterArea_Get_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.DeclaredPropertyGetter(typeof(MineShaft), nameof(MineShaft.isSlimeArea)),
					prefix: new HarmonyMethod(typeof(MineShaft_isSlimeArea_Get_Patch), nameof(MineShaft_isSlimeArea_Get_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;
			mapDict = Helper.GameContent.Load<Dictionary<string, MapData>>(dictPath);
			foreach(var map in mapDict.Values)
			{
				if(map.forceLevel > -1)
				{
					forcedMapDict[map.forceLevel] = map;
				}
			}
			Monitor.Log($"loaded {mapDict.Count} custom maps and {forcedMapDict.Count} forced maps");
		}

		private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
			{
				e.LoadFrom(() => new Dictionary<string, MapData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
				name: () => SHelper.Translation.Get("GMCM.AllowVanillaMaps.Name"),
				getValue: () => Config.AllowVanillaMaps,
				setValue: value => Config.AllowVanillaMaps = value
			);
		}
	}
}
