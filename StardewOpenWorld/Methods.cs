using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {

        private static void SetTiles(GameLocation gl)
        {
            Stopwatch s = new Stopwatch();

            var back = gl.Map.GetLayer("Back");
            var mainSheet = gl.Map.GetTileSheet("outdoors");
            Point startTile = mapLocation - new Point(openWorldTileSize / 2, openWorldTileSize / 2);
            s.Start();
            SMonitor.Log($"applying tiles");
            for (int y = 0; y < openWorldTileSize; y++)
            {
                for (int x = 0; x < openWorldTileSize; x++)
                {
                    var tile = backTiles[x + startTile.X, y + startTile.Y];
                    back.Tiles[x, y] = new StaticTile(back, mainSheet, BlendMode.Alpha, tile);
                }
            }
            SMonitor.Log($"applied tiles in {s.ElapsedMilliseconds} ms");
            s.Restart();
            /*
            var objs = openWorldLocation.objects.Pairs.Where(o => rect.Contains(o.Key.X, o.Key.Y));
            foreach (var obj in objs)
            {
                gl.objects[obj.Key - offset] = obj.Value;
            }
            SMonitor.Log($"applied objects in {s.ElapsedMilliseconds} ms");
            s.Restart();
            var tfs = openWorldLocation.terrainFeatures.Pairs.Where(o => rect.Contains(o.Key.X, o.Key.Y));
            foreach (var tf in tfs)
            {
                gl.terrainFeatures[tf.Key - offset] = tf.Value;
            }
            */
            SMonitor.Log($"applied tfs in {s.ElapsedMilliseconds} ms");
            s.Stop();
        }

        private static Rectangle GetTileRect(Vector2 v)
        {
            return new Rectangle((int)v.X * openWorldTileSize, (int)v.Y * openWorldTileSize, openWorldTileSize, openWorldTileSize);
        }

    }
}