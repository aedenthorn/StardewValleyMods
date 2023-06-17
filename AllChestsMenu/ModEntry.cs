using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.IO;

namespace AllChestsMenu
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Game1.activeClickableMenu is StorageMenu)
            {
                if (Game1.options.snappyMenus && Game1.options.gamepadControls && e.Button == Config.SwitchButton)
                {
                    Game1.playSound("shwip");
                    if (!(Game1.activeClickableMenu as StorageMenu).focusBottom)
                        (Game1.activeClickableMenu as StorageMenu).lastTopSnappedCC = Game1.activeClickableMenu.currentlySnappedComponent;
                    (Game1.activeClickableMenu as StorageMenu).focusBottom = !(Game1.activeClickableMenu as StorageMenu).focusBottom;
                    Game1.activeClickableMenu.currentlySnappedComponent = null;
                    Game1.activeClickableMenu.snapToDefaultClickableComponent();
                }
                if (((Game1.activeClickableMenu as StorageMenu).locationText.Selected || (Game1.activeClickableMenu as StorageMenu).renameBox.Selected) && e.Button.ToString().Length == 1)
                {
                    SHelper.Input.Suppress(e.Button);
                }
            }
            if (e.Button == Config.MenuKey && (Config.ModKey == SButton.None || !Config.ModToOpen || Helper.Input.IsDown(Config.ModKey)))
            {
                OpenMenu();
            }
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var phoneAPI = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (phoneAPI != null)
            {
                phoneAPI.AddApp("aedenthorn.AllChestsMenu", "Mailbox", OpenMenu, Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "icon.png")));
            }
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_LimitToCurrentLocation_Name"),
                getValue: () => Config.LimitToCurrentLocation,
                setValue: value => Config.LimitToCurrentLocation = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_MenuKey_Name"),
                getValue: () => Config.MenuKey,
                setValue: value => Config.MenuKey = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModToOpen_Name"),
                getValue: () => Config.ModToOpen,
                setValue: value => Config.ModToOpen = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModKey_Tooltip"),
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModKey2_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModKey2_Tooltip"),
                getValue: () => Config.ModKey2,
                setValue: value => Config.ModKey2 = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SwitchButton_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SwitchButton_Tooltip"),
                getValue: () => Config.SwitchButton,
                setValue: value => Config.SwitchButton = value
            );
        }

    }
}