using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace WikiLinks
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
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady)
				return;

			if (Config.OpenWikiPageKeys.Keybinds[0].Buttons.Any(button => button == e.Button) && Config.OpenWikiPageKeys.Keybinds[0].Buttons.All(button => SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button)))
			{
				if (ReceiveOpenWikiPageKeys())
				{
					Config.OpenWikiPageKeys.Keybinds[0].Buttons.ToList().ForEach(button => SHelper.Input.Suppress(button));
				}
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
			configMenu.AddKeybindList(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.OpenWikiPageKeys.Name"),
				getValue: () => Config.OpenWikiPageKeys,
				setValue: value => Config.OpenWikiPageKeys = value
			);
		}
	}
}
