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

        public static void PlayerTileChanged(Point oldPoint, Point newPoint)
        {
            if(newPoint.X / openWorldTileSize < oldPoint.X / openWorldTileSize)
            {
                UnloadWorldTile(oldPoint.X / openWorldTileSize + 1, oldPoint.Y);
                LoadWorldTile(oldPoint.X / openWorldTileSize - 1, oldPoint.Y);
            }
            else if(newPoint.X / openWorldTileSize > oldPoint.X / openWorldTileSize)
            {
                UnloadWorldTile(oldPoint.X / openWorldTileSize - 1, oldPoint.Y);
                LoadWorldTile(oldPoint.X / openWorldTileSize + 1, oldPoint.Y);
            }
            if(newPoint.Y / openWorldTileSize < oldPoint.Y / openWorldTileSize)
            {
                UnloadWorldTile(oldPoint.X, oldPoint.Y / openWorldTileSize + 1);
                LoadWorldTile(oldPoint.X, oldPoint.Y / openWorldTileSize - 1);
            }
            else if(newPoint.Y / openWorldTileSize > oldPoint.Y / openWorldTileSize)
            {
                UnloadWorldTile(oldPoint.X, oldPoint.Y / openWorldTileSize - 1);
                LoadWorldTile(oldPoint.X, oldPoint.Y / openWorldTileSize + 1);
            }
        }

        private static void UnloadWorldTile(int v, int y)
        {
            throw new NotImplementedException();
        }

        private static void LoadWorldTile(int x, int y)
        {
            WorldTile tile= new WorldTile();
            if(!cachedWorldTiles.TryGetValue(new Point(x, y), out tile))
            {
                tile = CreateTile(x, y);
            }
            LoadTileData(tile);
        }

        private static void LoadTileData(WorldTile tile)
        {
            throw new NotImplementedException();
        }

        private static WorldTile CreateTile(int x, int y)
        {
            foreach(var biome in biomes) 
            { 

            }
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