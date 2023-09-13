using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace ContentPatcherEditor
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

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            Helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(Config.ModEnabled && Config.ShowButton && e.Button == SButton.MouseLeft && Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu is null)
            {
                var rect = new Rectangle(42, Game1.viewport.Height - 192, 64, 64);
                if (rect.Contains(Game1.getMousePosition()))
                {
                    Game1.playSound("bigSelect");
                    TitleMenu.subMenu = new ContentPatcherMenu();
                    SHelper.Input.Suppress(e.Button);
                }
            }
        }


        private void Display_RenderedActiveMenu(object sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e)
        {
            if (Config.ModEnabled && Config.ShowButton && Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu is null)
            {
                var rect = new Rectangle(42, Game1.viewport.Height - 192, 64, 64);
                e.SpriteBatch.Draw(Game1.mouseCursors, rect, new Rectangle(330, 373, 16, 16), rect.Contains(Game1.getMousePosition()) ? Color.White : Color.White * 0.5f);
            }
                
        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is null && Config.ModEnabled && Config.MenuButton.JustPressed())
            {
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = new ContentPatcherMenu();
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ShowButton_Name"),
                    getValue: () => Config.ShowButton,
                    setValue: value => Config.ShowButton = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Backup_Name"),
                    getValue: () => Config.Backup,
                    setValue: value => Config.Backup = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_OpenModsFolderAfterZip_Name"),
                    getValue: () => Config.OpenModsFolderAfterZip,
                    setValue: value => Config.OpenModsFolderAfterZip = value
                );
                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_MenuButton_Name"),
                    getValue: () => Config.MenuButton,
                    setValue: value => Config.MenuButton = value
                );

            }

        }

    }
}