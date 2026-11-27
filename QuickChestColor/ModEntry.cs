using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;
using System.Reflection;

namespace QuickChestColor
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static PerScreen<Color> copyColor = new(() => Color.Black);
		public static PerScreen<bool> copyHalf = new(() => false);
        public static string modKey = "aedenthorn.QuickChestColor/half";

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
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			
            Harmony harmony = new(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        private void Input_ButtonPressed(object? sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if(e.Button == Config.PrevKey)
            {
                if (IncrementColor(false))
                    SHelper.Input.Suppress(e.Button);
            }
            else if (e.Button == Config.NextKey)
            {
                if (IncrementColor(true))
                    SHelper.Input.Suppress(e.Button);
            }
            else if (e.Button == Config.CopyPasteKey)
            {
                if (SHelper.Input.IsDown(Config.ModKey))
                {
                    if (PasteColor())
                        SHelper.Input.Suppress(e.Button);
                }
                else
                {
                    if (CopyColor())
                        SHelper.Input.Suppress(e.Button);
                }
            }
        }

        private void Input_MouseWheelScrolled(object? sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if (e.Delta < 0)
            {
                if (IncrementColor(false))
                    SHelper.Input.SuppressScrollWheel();
            }
            else if (e.Delta > 0)
            {
                if (IncrementColor(true))
                    SHelper.Input.SuppressScrollWheel();
            }
        }

        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is not null)
			{

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
                    name: () => SHelper.Translation.Get("GMCM.PrevKey.Name"),
                    getValue: () => Config.PrevKey,
                    setValue: value => Config.PrevKey = value
                );
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.NextKey.Name"),
                    getValue: () => Config.NextKey,
                    setValue: value => Config.NextKey = value
                );
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.CopyPasteKey.Name"),
                    getValue: () => Config.CopyPasteKey,
                    setValue: value => Config.CopyPasteKey = value
                );
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.ModKey.Name"),
                    getValue: () => Config.ModKey,
                    setValue: value => Config.ModKey = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.HalfByDefault.Name"),
                    getValue: () => Config.HalfByDefault,
                    setValue: value => Config.HalfByDefault = value
                );
            }
		}
	}
}
