using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using static StardewValley.Projectiles.BasicProjectile;
using static System.Net.Mime.MediaTypeNames;

namespace DynamicMapTiles
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string stepKey = "steppable";
        public static string explodableKey = "explodable";
        public static string pushKey = "pushable";
        public static string pushedKey = "pushed";
        public static Dictionary<string, List<PushedTile>> pushingDict = new Dictionary<string, List<PushedTile>>();

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
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.Delete)
            {
                var tile = Game1.currentLocation.Map.GetLayer("Buildings").Tiles[51, 86];
                if(tile != null)
                {
                    tile.Properties[explodableKey] = "T";
                    tile.Properties[pushKey] = "3";
                    tile.Properties[pushedKey] = "1";
                }
                tile = Game1.currentLocation.Map.GetLayer("Back").Tiles[20, 57];
                if(tile != null)
                {
                    tile.Properties[stepKey] = "380";
                }
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !pushingDict.TryGetValue(Game1.currentLocation.Name, out List<PushedTile> tiles))
                return;
            for(int i = tiles.Count - 1; i >= 0; i--)
            {
                var tile = tiles[i];
                switch (tile.dir)
                {
                    case 0:
                        tile.position += new Point(0, -1);
                        break;
                    case 1:
                        tile.position += new Point(1, 0);
                        break;
                    case 2:
                        tile.position += new Point(0, 1);
                        break;
                    case 3:
                        tile.position += new Point(-1, 0);
                        break;
                }
                if(tile.position == tile.destination)
                {
                    var pushed = tile.tile.Properties[pushedKey];
                    tile.tile.Properties[pushedKey] = tile.tile.Properties[pushKey];
                    tile.tile.Properties[pushKey] = pushed;

                    Game1.currentLocation.Map.GetLayer("Buildings").Tiles[tile.destination.X / 64, tile.destination.Y / 64] = tile.tile;
                    tiles.RemoveAt(i);
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
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
        }
    }
}