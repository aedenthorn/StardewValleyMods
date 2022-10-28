using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Diagnostics;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) })]
        public class Farmer_draw_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || !__instance.currentLocation.Name.StartsWith(namePrefix))
                    return;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.UpdateViewPort))]
        public class Game1_UpdateViewPort_Patch
        {
            public static void Prefix(ref bool overrideFreeze)
            {
                if (!Config.ModEnabled || !Game1.currentLocation.Name.StartsWith(namePrefix))
                    return;
                overrideFreeze = true;
                Game1.forceSnapOnNextViewportUpdate = true;
                Game1.currentLocation.forceViewportPlayerFollow = true;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public class Game1_loadForNewGame_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;


                openWorldLocation = new GameLocation("StardewOpenWorldMap", namePrefix) { IsOutdoors = true, IsFarm = true, IsGreenhouse = false };
                SMonitor.Log("Created new game location");
                var back = openWorldLocation.Map.GetLayer("Back");
                var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");

                backTiles = new int[50000, 50000];
                Stopwatch s = new Stopwatch();
                s.Start();
                SMonitor.Log($"applying tiles");
                for (int y = 0; y < 50000; y++)
                {
                    for (int x = 0; x < 50000; x++)
                    {
                        int idx = 351;
                        var which = Game1.random.NextDouble();
                        if (which < 0.025f)
                        {
                            idx = 304;
                        }
                        else if (which < 0.05f)
                        {
                            idx = 305;
                        }
                        else if (which < 0.15f)
                        {
                            idx = 300;
                        }
                        backTiles[x, y] = idx;
                    }
                }
                SMonitor.Log($"created all tiles in {s.ElapsedMilliseconds} ms");
                Game1.locations.Add(openWorldLocation);
            }
        }
    }
}