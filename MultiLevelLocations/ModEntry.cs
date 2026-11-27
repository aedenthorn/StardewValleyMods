using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
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
            harmony.PatchAll();

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
            bool tileIsSwitch = tile.Properties.TryGetValue("SwitchLevel", out PropertyValue levelData);
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
            if(!int.TryParse(parts[which], out int newLevel) || newLevel < 1)
            {
                Monitor.Log($"Invalid floor: {newLevel}");
            }

            if (lastPlayerLevel == newLevel)
            {
                Monitor.Log($"Not switching to same level");
                return;
            }
            Monitor.Log($"Switching to level {newLevel}");
            RearrangeLayersForPlayer(newLevel);
            RearrangeObjectsForPlayer(newLevel);
            lastPlayerLevel = newLevel;
        }

        private void RearrangeObjectsForPlayer(int newLevel)
        {

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
            for (int i = 0; i < Game1.player.currentLocation.furniture.Count; i++)
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
        }
        private void RearrangeLayersForPlayer(int newLevel)
        {
            if (!Config.EnableMod)
                return;
            Game1.player.currentLocation.loadMap(Game1.player.currentLocation.mapPath.Value, true);
            Game1.player.currentLocation.temporarySprites.Clear();
            if (newLevel == 1)
            {
                Monitor.Log($"Reverting map for level 1");
                Game1.player.currentLocation.resetForPlayerEntry();
            }
            else
            {
                Layer[] oldLayers = Game1.player.currentLocation.map.Layers.ToArray();
                List<Layer> newLayers = new List<Layer>();
                Dictionary<string, Layer> newLayersById = new Dictionary<string, Layer>();
                Regex rx = new Regex(@"^(?<level>[0-9]*)(?<name>[^0-9]+)(?<which>[0-9]*)", RegexOptions.Compiled);
                int backs = 0;
                foreach (var layer in oldLayers)
                {
                    var match = rx.Match(layer.Id);
                    if (!match.Success) // skip
                        continue;
                    if (!layer.Properties.TryGetValue("MultiLevel", out PropertyValue thisLevel))
                    {
                        thisLevel = layer.Id;
                        Monitor.Log($"Adding layer property to {thisLevel}");
                        layer.Properties.Add("MultiLevel", thisLevel);
                    }
                    Monitor.Log($"Moving layer {thisLevel}");
                    if (match.Groups["level"].Value.Length == 0) // is original, first level
                    {
                        layer.Id = "Back" + (backs > 0 ? backs : "");
                        Monitor.Log($"new id: {layer.Id}");
                        newLayers.Add(layer);
                        newLayersById.Add(layer.Id, layer);
                        backs++;
                        continue;
                    }
                    int level = int.Parse(match.Groups["level"].Value);
                    if (level < newLevel) // is below current level
                    {
                        layer.Id = "Back" + backs;
                        Monitor.Log($"new id: {layer.Id}");
                        newLayers.Add(layer);
                        newLayersById.Add(layer.Id, layer);
                        backs++;
                        continue;
                    }
                    if (level == newLevel) // is current level
                    {
                        if(match.Groups["name"].Value == "Back") // set to top back
                        {
                            int which = 0;
                            if(match.Groups["which"].Value.Length > 0)
                                which = int.Parse(match.Groups["which"].Value);
                            layer.Id = "Back" + (backs + which);
                            backs++;
                        }
                        else
                            layer.Id = match.Groups["name"].Value + match.Groups["which"].Value;
                        Monitor.Log($"new id: {layer.Id}");
                        newLayers.Add(layer);
                        newLayersById.Add(layer.Id, layer);
                        continue;
                    }
                    Monitor.Log($"new id: {layer.Id}");
                    newLayers.Add(layer);
                    newLayersById.Add(layer.Id, layer);
                }
                AccessTools.Field(typeof(Map), "m_readOnlyLayers").SetValue(Game1.player.currentLocation.map, new ReadOnlyCollection<Layer>(newLayers));
                AccessTools.Field(typeof(Map), "m_layersById").SetValue(Game1.player.currentLocation.map, newLayersById);
                foreach(var layer in Game1.player.currentLocation.map.Layers)
                {
                    Monitor.Log($"layer: {layer.Id}");
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }
    }
}