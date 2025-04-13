using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace WateringCanTweaks
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

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Tool), nameof(Tool.draw), new Type[] {typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(typeof(Tool_draw_Patch), nameof(Tool_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(WateringCan), nameof(WateringCan.DoFunction)),
					prefix: new HarmonyMethod(typeof(WateringCan_DoFunction_Patch), nameof(WateringCan_DoFunction_Patch.Prefix)),
					postfix: new HarmonyMethod(typeof(WateringCan_DoFunction_Patch), nameof(WateringCan_DoFunction_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.canStrafeForToolUse)),
					postfix: new HarmonyMethod(typeof(Farmer_canStrafeForToolUse_Patch), nameof(Farmer_canStrafeForToolUse_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.toolPowerIncrease)),
					prefix: new HarmonyMethod(typeof(Farmer_toolPowerIncrease_Patch), nameof(Farmer_toolPowerIncrease_Patch.Prefix))
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
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.WaterAdjacentTiles.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.WaterAdjacentTiles.Tooltip"),
				getValue: () => Config.WaterAdjacentTiles,
				setValue: value => Config.WaterAdjacentTiles = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.WateringTileMultiplier.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.WateringTileMultiplier.Tooltip"),
				getValue: () => Config.WateringTileMultiplier,
				setValue: value => Config.WateringTileMultiplier = Math.Max(0, Math.Min(value, 1000))
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.VolumeMultiplier.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.VolumeMultiplier.Tooltip"),
				getValue: () => Config.VolumeMultiplier,
				setValue: value => Config.VolumeMultiplier = Math.Max(0, Math.Min(value, 1000))
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.StaminaUseMultiplier.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.StaminaUseMultiplier.Tooltip"),
				getValue: () => Config.StaminaUseMultiplier,
				setValue: value => Config.StaminaUseMultiplier = Math.Max(0, Math.Min(value, 1000))
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.StrafeWhileWatering.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.StrafeWhileWatering.Tooltip"),
				getValue: () => Config.StrafeWhileWatering,
				setValue: value => Config.StrafeWhileWatering = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AutoEndWateringAnimation.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AutoEndWateringAnimation.Tooltip"),
				getValue: () => Config.AutoEndWateringAnimation,
				setValue: value => Config.AutoEndWateringAnimation = value
			);
		}
	}
}
