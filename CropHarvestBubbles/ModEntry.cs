using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace CropHarvestBubbles
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
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

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.draw)),
					postfix: new HarmonyMethod(typeof(Crop_draw_Patch), nameof(Crop_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.drawWithOffset)),
					postfix: new HarmonyMethod(typeof(Crop_drawWithOffset_Patch), nameof(Crop_drawWithOffset_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
			if (!Config.ModEnabled)
				return;
			if(Config.KeyPressToggle && Config.PressKeys.JustPressed())
			{
				Config.Toggled = !Config.Toggled;
				Helper.WriteConfig(Config);
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
				name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM_Option_IgnoreFlowers_Name"),
				getValue: () => Config.IgnoreFlowers,
				setValue: value => Config.IgnoreFlowers = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM_Option_IgnorePots_Name"),
				getValue: () => Config.IgnorePots,
				setValue: value => Config.IgnorePots = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM_Option_RequireKeyPress_Name"),
				getValue: () => Config.RequireKeyPress,
				setValue: value => Config.RequireKeyPress = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM_Option_KeyPressToggle_Name"),
				getValue: () => Config.KeyPressToggle,
				setValue: value => Config.KeyPressToggle = value
			);
			configMenu.AddKeybindList(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM_Option_PressKeys_Name"),
				getValue: () => Config.PressKeys,
				setValue: value => Config.PressKeys = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM_Option_OpacityPercent_Name"),
				getValue: () => Config.OpacityPercent,
				setValue: value => Config.OpacityPercent = value,
				min: 1,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM_Option_SizePercent_Name"),
				getValue: () => Config.SizePercent,
				setValue: value => Config.SizePercent = value,
				min: 1,
				max: 100
			);
		}
	}
}
