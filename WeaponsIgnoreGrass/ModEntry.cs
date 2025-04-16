using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace WeaponsIgnoreGrass
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private const int MessageID = 15409;

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Grass), nameof(Grass.performToolAction)),
					prefix: new HarmonyMethod(typeof(Grass_performToolAction_Patch), nameof(Grass_performToolAction_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Context.IsPlayerFree)
				return;

			if (Config.ToggleKeys.Keybinds[0].Buttons.Any(button => button == e.Button) && Config.ToggleKeys.Keybinds[0].Buttons.All(button => SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button)))
			{
				Config.ModEnabled = !Config.ModEnabled;
				Helper.WriteConfig(Config);

				if (Config.ModEnabled && !Config.ShowEnabledMessage)
					return;
				if (!Config.ModEnabled && !Config.ShowDisabledMessage)
					return;

				string text = Config.ModEnabled ? Helper.Translation.Get("enabled-message") : Helper.Translation.Get("disabled-message");

				Game1.hudMessages.RemoveAll(m => m.number == MessageID);
				Game1.addHUDMessage(new HUDMessage(text, HUDMessage.error_type) { noIcon = true, number = MessageID });
				Config.ToggleKeys.Keybinds[0].Buttons.ToList().ForEach(button => SHelper.Input.Suppress(button));
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
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.WeaponsIgnoreGrass.Name"),
				getValue: () => Config.WeaponsIgnoreGrass,
				setValue: value => Config.WeaponsIgnoreGrass = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ScythesIgnoreGrass.Name"),
				getValue: () => Config.ScythesIgnoreGrass,
				setValue: value => Config.ScythesIgnoreGrass = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ShowEnabledMessage.Name"),
				getValue: () => Config.ShowEnabledMessage,
				setValue: value => Config.ShowEnabledMessage = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ShowDisabledMessage.Name"),
				getValue: () => Config.ShowDisabledMessage,
				setValue: value => Config.ShowDisabledMessage = value
			);
			configMenu.AddKeybindList(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ToggleKeys.Name"),
				getValue: () => Config.ToggleKeys,
				setValue: value => Config.ToggleKeys = value
			);
		}
	}
}
