using System.Collections.Generic;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace Alarms
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;
		internal const string modKey = "aedenthorn.Alarms/alarms";

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
			Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
		}

		private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
		{
			if(!Config.ModEnabled)
                return;
            ClockSoundMenu.ReloadSounds();
        }

		private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
		{
			if (Context.CanPlayerMove && Game1.activeClickableMenu is null && Config.ModEnabled && Config.MenuButton.JustPressed())
			{
				Game1.playSound("bigSelect");
				Game1.activeClickableMenu = new ClockSoundMenu();
			}
		}

		private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
		{
			CheckForSound(e.NewTime);
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
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.DefaultSound.Name"),
				getValue: () => Config.DefaultSound,
				setValue: value => Config.DefaultSound = value
			);
			configMenu.AddKeybindList(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MenuButton.Name"),
				getValue: () => Config.MenuButton,
				setValue: value => Config.MenuButton = value
			);
		}
	}
}
