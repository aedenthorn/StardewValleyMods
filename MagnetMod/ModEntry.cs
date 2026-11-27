using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EnhancedLootMagnet
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
					original: AccessTools.Method(typeof(Debris), "playerInRange"),
					prefix: new HarmonyMethod(typeof(Debris_playerInRange_Patch), nameof(Debris_playerInRange_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Debris), nameof(Debris.updateChunks)),
					postfix: new HarmonyMethod(typeof(Debris_updateChunks_Patch), nameof(Debris_updateChunks_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
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
				name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.RangeMultiplier.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.RangeMultiplier.Tooltip"),
				getValue: () => Config.RangeMultiplier,
				setValue: value => Config.RangeMultiplier = value,
				min: 0f,
				max: 10f,
				interval: 0.5f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.SpeedMultiplier.Name"),
				getValue: () => Config.SpeedMultiplier,
				setValue: value => Config.SpeedMultiplier = value,
				min: 1,
				max: 10
			);
		}
	}
}
