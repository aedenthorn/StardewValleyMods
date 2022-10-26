using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace TakeAll
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static IList<Item> playerItems;
        public static IList<Item> otherItems;

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

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || e.Button != Config.TakeButton || Game1.activeClickableMenu is null)
                return;

            SMonitor.Log($"pressed take button on {Game1.activeClickableMenu.GetType()}");
            playerItems = null;
            otherItems = null;
            foreach (var f in Game1.activeClickableMenu.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var field = f.GetValue(Game1.activeClickableMenu);
                if (field is InventoryMenu && TryTakeItems(field as InventoryMenu))
                {
                    if (Config.CloseAfterTake)
                        Game1.activeClickableMenu.exitThisMenu();
                    return;
                }
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
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Close After Take",
                getValue: () => Config.CloseAfterTake,
                setValue: value => Config.CloseAfterTake = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Take Same By Default",
                tooltip: () => "If true only take items you already have by default",
                getValue: () => Config.TakeSameByDefault,
                setValue: value => Config.TakeSameByDefault = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Take Button",
                getValue: () => Config.TakeButton,
                setValue: value => Config.TakeButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Button",
                tooltip: () => "If down, only take items you already have (or opposite if Take Same By Default is true)",
                getValue: () => Config.ModButton,
                setValue: value => Config.ModButton = value
            );
        }
    }
}