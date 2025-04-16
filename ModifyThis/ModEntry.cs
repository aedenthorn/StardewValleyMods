using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ModifyThis
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		private static Vector2 cursorTile;
		private static object thing;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();
			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
		}

		private static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.CanPlayerMove)
				return;

			if (e.Button == Config.WizardKey)
			{
				StartWizard();
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
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.WizardKey.Name"),
				getValue: () => Config.WizardKey,
				setValue: value => Config.WizardKey = value
			);
		}
	}
}
