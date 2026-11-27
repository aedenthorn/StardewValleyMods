using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace UndergroundSecrets
{
    internal class MushroomTrees
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            int max = (int)Math.Round(clearSpots.Count * Math.Min(Math.Max(0, config.MushroomTreesMaxPortion), 1));

            int num = Math.Min(clearSpots.Count - 1, Game1.random.Next(0, max));

            monitor.Log($"adding {num} mushroom trees");

            List<Vector2> rSpots = Utils.ShuffleList(clearSpots);

            for (int i = num - 1; i >= 0; i--)
            {
                shaft.terrainFeatures.Add(rSpots[i], new Tree(7, Game1.random.Next(1,6)));

                //shaft.setMapTileIndex((int)rSpots[i].X, (int)rSpots[i].Y,0,"Back");
                //monitor.Log($"adding random trap at {(int)rSpots[i].X},{(int)rSpots[i].Y}");
                foreach (Vector2 v in Utils.GetSurroundingTiles(rSpots[i], 3))
                {
                    superClearCenters.Remove(v);
                    if (Math.Abs(v.X - rSpots[i].X) < 2 && Math.Abs(v.Y - rSpots[i].Y) < 2)
                        clearCenters.Remove(v);
                }
                clearSpots.Remove(rSpots[i]);
            }
            shaft.terrainFeatures.OnValueRemoved += TerrainFeatures_OnValueRemoved;
        }

        private static void TerrainFeatures_OnValueRemoved(Vector2 key, TerrainFeature value)
        {
            monitor.Log($"value removed, {value.GetType()} {Environment.StackTrace}");
        }
    }
}