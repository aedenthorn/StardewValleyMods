using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using static StardewValley.Minigames.MineCart.Whale;
using Object = StardewValley.Object;

namespace MultiStorageMenu
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
            if (Game1.activeClickableMenu is StorageMenu && ((Game1.activeClickableMenu as StorageMenu).locationText.Selected || (Game1.activeClickableMenu as StorageMenu).renameBox.Selected))
            {
                if(e.Button.ToString().Length == 1)
                {
                    SHelper.Input.Suppress(e.Button);
                }
            }
            if (e.Button == Config.MenuKey && (Config.ModKey == SButton.None || Helper.Input.IsDown(Config.ModKey)))
            {
                OpenMenu();
            }
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var phoneAPI = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (phoneAPI != null)
            {
                phoneAPI.AddApp("aedenthorn.MultiStorageMenu", "Mailbox", OpenMenu, Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "icon.png")));
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
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Menu Key",
                getValue: () => Config.MenuKey,
                setValue: value => Config.MenuKey = value
            );
        }

    }
}