using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using static StardewValley.Network.NetAudio;

namespace UndergroundSecrets
{
    internal class Traps
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int traps = 3;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            int max = (int)Math.Round(clearSpots.Count * Math.Min(Math.Max(0, config.TrapsMaxPortion), 1));

            int num = Math.Min(clearSpots.Count, Game1.random.Next(0, max));

            monitor.Log($"adding {num} traps of {max} max");

            List<Vector2> rSpots = Utils.ShuffleList(clearSpots);

            for (int i = 0; i < num; i++)
            {
                monitor.Log($"adding collapsing floor at {(int)rSpots[i].X},{(int)rSpots[i].Y}");
                shaft.setTileProperty((int)rSpots[i].X, (int)rSpots[i].Y, "Back", "TouchAction", "randomTrap");
                //shaft.setMapTileIndex((int)rSpots[i].X, (int)rSpots[i].Y,0,"Back");

                foreach (Vector2 v in Utils.GetSurroundingTiles(rSpots[i], 3))
                {
                    superClearCenters.Remove(v);
                    if (Math.Abs(v.X - rSpots[i].X) < 2 && Math.Abs(v.Y - rSpots[i].Y) < 2)
                        clearCenters.Remove(v);
                }
                clearSpots.Remove(rSpots[i]);
            }
        }


        public static void TriggerRandomTrap(MineShaft shaft, Vector2 position)
        {
            int which = Game1.random.Next(0, traps);
            switch (which)
            {
                case 0:
                    FireballTrap(shaft, position);
                    break;
                case 1:
                    ExplosionTrap(shaft, position);
                    break;
                case 2:
                    SlimeTrap(shaft, position);
                    break;
            }
            shaft.removeTileProperty((int)position.X, (int)position.Y, "Back", "TouchAction");
        }

        public static void FireballTrap(MineShaft shaft, Vector2 position)
        {
            shaft.playSound("furnace", SoundContext.Default);

            foreach (Vector2 v in Utils.getCenteredSpots(position))
            {
                try
                {
                    float x = position.X - v.X;
                    float y = position.Y - v.Y;
                    BasicProjectile projectile = new BasicProjectile(shaft.mineLevel / 3, 10, 0, 1, 0.5f, x, y, v * Game1.tileSize, "", "", false, false, shaft, Game1.player, false, null);
                    projectile.ignoreTravelGracePeriod.Value = true;
                    projectile.maxTravelDistance.Value = 100;
                    shaft.projectiles.Add(projectile);
                }
                catch(Exception ex) 
                { 
                    //monitor.Log($"error creating fire: {ex}"); 
                }
            }

        }
        public static void ExplosionTrap(MineShaft shaft, Vector2 position)
        {
            shaft.playSound("explosion", SoundContext.Default);

            shaft.explode(position, Game1.random.Next(2,8), Game1.player);
        }
        public static void SlimeTrap(MineShaft shaft, Vector2 position)
        {
            shaft.playSound("slime", SoundContext.Default);
            foreach (Vector2 v in Utils.getCenteredSpots(position, true))
            {
                try
                {
                    if (Game1.random.NextDouble() < 0.2)
                    {
                        shaft.characters.Add(new BigSlime(v * Game1.tileSize, shaft.getMineArea(-1)));
                    }
                    else
                    {
                        shaft.characters.Add(new GreenSlime(v * Game1.tileSize, shaft.mineLevel));
                    }
                }
                catch { }
            }

        }
    }
}