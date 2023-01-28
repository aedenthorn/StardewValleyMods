using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using Object = StardewValley.Object;

namespace ImmersiveSprinklers
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string sprinklerKey = "aedenthorn.ImmersiveSprinklers/sprinkler";
        public static Dictionary<string, Object> sprinklerDict = new();

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
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.Button == Config.PickupButton && Context.CanPlayerMove && Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out var tf) && tf is HoeDirt)
            {
                int which = GetMouseCorner();
                if (ReturnSprinkler(Game1.player, Game1.currentLocation, tf, Game1.currentCursorTile, which))
                {
                    Helper.Input.Suppress(e.Button);
                }
            }
            if (e.Button == Config.ActivateButton && Context.CanPlayerMove)
            {
                int which = GetMouseCorner();
                Vector2 tile = Game1.currentCursorTile;
                string sprinklerString;
                if (GetSprinklerTileBool(Game1.currentLocation, ref tile, ref which, out sprinklerString))
                {
                    var obj = GetSprinkler(sprinklerString);
                    if (obj is not null)
                    {
                        ActivateSprinkler(Game1.currentLocation, tile, obj, which, false);
                        Helper.Input.Suppress(e.Button);
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            sprinklerDict.Clear();
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Scale",
                getValue: () => Config.Scale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Scale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Alpha",
                getValue: () => Config.Alpha + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Alpha = f; } }
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Pickup Key",
                getValue: () => Config.PickupButton,
                setValue: value => Config.PickupButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Activate Key",
                getValue: () => Config.ActivateButton,
                setValue: value => Config.ActivateButton = value
            );

        }

    }
}
