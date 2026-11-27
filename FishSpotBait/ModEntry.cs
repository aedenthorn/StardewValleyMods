using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace FishSpotBait
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static readonly PerScreen<Farmer> player = new(() => null);
		private static readonly PerScreen<int> direction = new(() => -1);

		public static Farmer Player
		{
			get => player.Value;
			set => player.Value = value;
		}

		public static int Direction
		{
			get => direction.Value;
			set => direction.Value = value;
		}

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
					prefix: new HarmonyMethod(typeof(GameLocation_checkAction_Patch), nameof(GameLocation_checkAction_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private static void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
		{
			if (Player is not null && Direction >= 0)
			{
				Player.FacingDirection = Direction;
				Player = null;
				Direction = -1;
			}
			SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
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
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModKey.Name"),
				getValue: () => Config.ModKey,
				setValue: value => Config.ModKey = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RandomRadius.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.RandomRadius.Tooltip"),
				getValue: () => Config.RandomRadius,
				setValue: value => Config.RandomRadius = value,
				min: 0,
				max: 8
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxRange.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxRange.Tooltip"),
				getValue: () => Config.MaxRange,
				setValue: value => Config.MaxRange = value,
				min: 4,
				max: 8
			);
		}
	}
}
