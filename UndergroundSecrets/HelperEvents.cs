using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using xTile.Layers;

namespace UndergroundSecrets
{
    public class HelperEvents
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static Vector2 lastLoc = new Vector2(-1,-1);

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }
        public static void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            //monitor.Log($"{e.NewLocation.Name} terrain features: {e.NewLocation.terrainFeatures.Count()}");
        }

        internal static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if(Game1.player.currentLocation is MineShaft)
            {
                Vector2 loc = Game1.player.getTileLocation();
                if (lastLoc != loc)
                {
                    MineShaft shaft = Game1.player.currentLocation as MineShaft;
                    Vector2[] tiles = Utils.getCenteredSpots(loc, true);
                    Layer back = shaft.map.GetLayer("Back");
                    foreach (Vector2 tile in tiles)
                    {
                        if (tile.X < 0 || tile.Y < 0 || tile.X >= back.LayerWidth || tile.Y >= back.LayerHeight)
                            continue;
                        if(shaft.doesTileHaveProperty((int)tile.X, (int)tile.Y, "TouchAction", "Back") == "randomTrap" && Game1.random.NextDouble() < (Game1.player.getEffectiveSkillLevel(3) + Game1.player.getEffectiveSkillLevel(4) + Game1.player.getEffectiveSkillLevel(5)) / 0.3f * Utils.Clamp(0, 1, config.DisarmTrapsChanceModifier))
                        {
                            Game1.player.currentLocation.playSoundAt("openBox", tile);
                            Game1.player.currentLocation.playSoundAt("Cowboy_monsterDie", tile);
                            shaft.removeTileProperty((int)tile.X, (int)tile.Y, "Back", "TouchAction");
                        }
                    }
                    lastLoc = loc;
                }

            }
        }
    }
}