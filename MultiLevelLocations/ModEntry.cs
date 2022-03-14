using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MultiLevelLocations
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static bool startedWalking;
        public static ModEntry lastWalking;
        public static bool wasOnSwitchTile;
        public static Point lastSwitchTile = new Point(-1,-1);
        public static string lastSwitchData;
        public static int lastPlayerLevel = 1;
        public static GameLocation lastSwitchGameLocation;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Player.Warped += Player_Warped;
            var harmony = new Harmony(ModManifest.UniqueID);

            // Game1 Patches

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.updateMap)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_updateMap_Prefix))
            );
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            lastPlayerLevel = 1;
            ResetSwitchData();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            lastPlayerLevel = 1;
            ResetSwitchData();
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.EnableMod || !Context.CanPlayerMove)
                return;

            if(lastSwitchGameLocation != null && Game1.player.currentLocation != lastSwitchGameLocation)
            {
                ResetSwitchData();
                return;
            }

            Tile tile = Game1.player.currentLocation.map.GetLayer("Back")?.PickTile(new Location(Game1.player.getTileX() * 64, Game1.player.getTileY() * 64), Game1.viewport.Size);
            if (tile == null)
                return;
            int newLevel = 1;
            bool tileIsSwitch = tile.Properties.TryGetValue("SwitchMapLevel", out PropertyValue levelData);
            bool setMapLevel = tile.Properties.TryGetValue("SetMapLevel", out PropertyValue newLevelString) && int.TryParse(newLevelString, out newLevel);
            if (setMapLevel)
            {
                lastPlayerLevel = newLevel;
            }
            if (tileIsSwitch)
            {
                wasOnSwitchTile = true;
                lastSwitchTile = Game1.player.getTileLocationPoint();
                lastSwitchData = levelData;
                lastSwitchGameLocation = Game1.player.currentLocation;
            }
            else if (wasOnSwitchTile && !tileIsSwitch)
            {
                int which;
                if (Game1.player.getTileLocationPoint() == lastSwitchTile + new Point(0, -1))
                {
                    which = 0;
                }
                else if (Game1.player.getTileLocationPoint() == lastSwitchTile + new Point(1, 0))
                {
                    which = 1;
                }
                else if (Game1.player.getTileLocationPoint() == lastSwitchTile + new Point(0, 1))
                {
                    which = 2;
                }
                else if (Game1.player.getTileLocationPoint() == lastSwitchTile + new Point(-1, 0))
                {
                    which = 3;
                }
                else
                {
                    ResetSwitchData();
                    return;
                }
                SwitchLevel(lastSwitchData, which);
                ResetSwitchData();
            }
        }

        private void ResetSwitchData()
        {
            wasOnSwitchTile = false;
            lastSwitchTile = Point.Zero;
            lastSwitchData = "";
            lastSwitchGameLocation = null;
        }

        private void SwitchLevel(string data, int which)
        {
            string[] parts = data.Split(' ');
            if (parts.Length != 4)
            {
                Monitor.Log($"Invalid switch data: {data}");
                return;
            }
            if(!parts[which].Contains(":"))
            {
                Monitor.Log($"Invalid switch: {parts[which]}");
                return;
            }
            parts = parts[which].Split(':');
            if(!int.TryParse(parts[0], out int newLevel) || newLevel < 1)
            {
                Monitor.Log($"Invalid floor: {newLevel}");
            }
            if (!parts[1].Contains(","))
            {
                Monitor.Log($"Invalid position: {parts[1]}");
                return;
            }
            var newPos = parts[1].Split(',');
            if(!int.TryParse(newPos[0],out int newX) || !int.TryParse(newPos[1], out int newY))
            {
                Monitor.Log($"Invalid position: {parts[1]}");
                return;
            }
            Game1.player.position.Value = new Vector2(newX, newY) * 64;
            if (lastPlayerLevel == newLevel)
            {
                Monitor.Log($"Not switching to same level");
                return;
            }
            Game1.player.currentLocation.updateMap();

            foreach (var v in Game1.player.currentLocation.Objects.Keys.ToList())
            {
                Vector2 newVector;
                if (v.X >= 0 && v.X < 1000)
                {
                    newVector = v + new Vector2(1000 * lastPlayerLevel, 0);
                }
                else if (v.X >= 1000 * newLevel && v.X < 1000 * newLevel + 1000)
                {
                    newVector = v - new Vector2(1000 * newLevel, 0);
                }
                else continue;
                if (!Game1.player.currentLocation.Objects.ContainsKey(newVector))
                {
                    Game1.player.currentLocation.Objects[newVector] = Game1.player.currentLocation.Objects[v];
                    Game1.player.currentLocation.Objects.Remove(v);
                    if (Game1.player.currentLocation.Objects[newVector].lightSource != null)
                    {
                        Game1.player.currentLocation.removeLightSource(Game1.player.currentLocation.Objects[newVector].lightSource.Identifier);
                    }
                    Game1.player.currentLocation.Objects[newVector].initializeLightSource(Game1.player.currentLocation.Objects[newVector].TileLocation, false);
                }
            }
            foreach (var v in Game1.player.currentLocation.terrainFeatures.Keys.ToList())
            {
                Vector2 newVector;
                if (v.X >= 0 && v.X < 1000)
                {
                    newVector = v + new Vector2(1000 * lastPlayerLevel, 0);
                }
                else if (v.X >= 1000 * newLevel && v.X < 1000 * newLevel + 1000)
                {
                    newVector = v - new Vector2(1000 * newLevel, 0);
                }
                else continue;
                if (!Game1.player.currentLocation.terrainFeatures.ContainsKey(newVector))
                {
                    Game1.player.currentLocation.terrainFeatures[newVector] = Game1.player.currentLocation.terrainFeatures[v];
                    Game1.player.currentLocation.terrainFeatures.Remove(v);
                }
            }
            for(int i = 0; i < Game1.player.currentLocation.furniture.Count; i++)
            {
                var f = Game1.player.currentLocation.furniture[i];
                Vector2 v = Game1.player.currentLocation.furniture[i].TileLocation;
                Vector2 newVector;
                if (v.X >= 0 && v.X < 1000)
                {
                    newVector = v + new Vector2(1000 * lastPlayerLevel, 0);
                }
                else if (v.X >= 1000 * newLevel && v.X < 1000 * newLevel + 1000)
                {
                    newVector = v - new Vector2(1000 * newLevel, 0);
                }
                else continue;

                Game1.player.currentLocation.furniture[i].RemoveLightGlow(Game1.player.currentLocation);
                Game1.player.currentLocation.furniture[i].removeLights(Game1.player.currentLocation);

                Game1.player.currentLocation.furniture[i].TileLocation = newVector;
                if (!Game1.player.currentLocation.furniture[i].isGroundFurniture())
                {
                    newVector.Y = Game1.player.currentLocation.furniture[i].GetModifiedWallTilePosition(Game1.player.currentLocation, (int)newVector.X, (int)newVector.Y);
                }
                Game1.player.currentLocation.furniture[i].boundingBox.Value = new Rectangle((int)newVector.X * 64, (int)newVector.Y * 64, Game1.player.currentLocation.furniture[i].boundingBox.Width, Game1.player.currentLocation.furniture[i].boundingBox.Height);
                Game1.player.currentLocation.furniture[i].updateDrawPosition();
                Game1.player.currentLocation.furniture[i].resetOnPlayerEntry(Game1.player.currentLocation, false);
            }
            lastPlayerLevel = newLevel;
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
        }
    }
}