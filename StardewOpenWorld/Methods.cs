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

        private static void SetTiles(GameLocation gl, Point delta)
        {

            var oldTiles = (Tile[,])grassTiles.Clone();
            for (int y = 0; y < openWorldTileSize; y++)
            {
                var ty = (openWorldTileSize + y + delta.Y) % openWorldTileSize;
                for (int x = 0; x < openWorldTileSize; x++)
                {
                    var tx = (openWorldTileSize + x + delta.X) % openWorldTileSize;
                    var tile = oldTiles[tx, ty];
                    grassTiles[x, y] = tile;
                }
            }
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
            SMonitor.Log($"applied tfs in {s.ElapsedMilliseconds} ms");
            s.Stop();
            */
        }

        private static Rectangle GetTileRect(Vector2 v)
        {
            return new Rectangle((int)v.X * openWorldTileSize, (int)v.Y * openWorldTileSize, openWorldTileSize, openWorldTileSize);
        }
        public static Tile GetTile(Layer layer, int x, int y)
        {
            return null;
        }

        public static void SetTile(Layer layer, int x, int y, Tile value)
        {
        }

    }
}