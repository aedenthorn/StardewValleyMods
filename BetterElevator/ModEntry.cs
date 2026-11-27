using System;
using HarmonyLib;
using xTile.Dimensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace BetterElevator
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
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string), typeof(Farmer), typeof(Location) }),
					prefix: new HarmonyMethod(typeof(GameLocation_performAction_Patch), nameof(GameLocation_performAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
					prefix: new HarmonyMethod(typeof(MineShaft_checkAction_Patch), nameof(MineShaft_checkAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.shouldCreateLadderOnThisLevel)),
					postfix: new HarmonyMethod(typeof(MineShaft_shouldCreateLadderOnThisLevel_Patch), nameof(MineShaft_shouldCreateLadderOnThisLevel_Patch.Postfix))
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
				name: () => ModEntry.SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModKey.Name"),
				getValue: () => Config.ModKey,
				setValue: value => Config.ModKey = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Unrestricted.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.Unrestricted_Tooltip"),
				getValue: () => Config.Unrestricted,
				setValue: value => Config.Unrestricted = value
			);
		}
	}
}
