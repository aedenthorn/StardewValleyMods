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

        private static void UnloadWorldTile(int x, int y)
        {
            if (!cachedWorldTiles.ContainsKey(new Point(x, y)))
                return;
            bool keep = false;
            foreach(var f in Game1.getAllFarmers())
            {
                if (f.currentLocation.Name.Contains(locName) && GetPlayerTile(f) == new Point(x, y))
                {
                    keep = true;
                    break;
                }
            }
            if (!keep)
                cachedWorldTiles.Remove(new Point(x, y));
        }

        private static Point GetPlayerTile(Farmer f)
        {
            return new Point(f.getTileX() / openWorldTileSize, f.getTileY() / openWorldTileSize);
        }

        private static void LoadWorldTile(int x, int y)
        {
            WorldTile tile = null;
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
            WorldTile outTile = new WorldTile();
            List<WorldTile> tiles = new List<WorldTile>();
            foreach (var biome in biomes) 
            {
                var tile = biome.Value.Invoke(randomSeed, x, y);
                if(tile != null)
                    tiles.Add(tile);
            }
            if (!tiles.Any())
                return null;
            tiles.Sort(delegate (WorldTile a, WorldTile b) { return a.priority.CompareTo(b.priority); });
            foreach(var tile in tiles)
            {
                foreach (var kvp in tile.objects)
                    outTile.objects[kvp.Key] = kvp.Value;
                foreach (var kvp in tile.terrainFeatures)
                    outTile.terrainFeatures[kvp.Key] = kvp.Value;
                foreach (var kvp in tile.back)
                    outTile.back[kvp.Key] = kvp.Value;
                foreach (var kvp in tile.buildings)
                    outTile.buildings[kvp.Key] = kvp.Value;
                foreach (var kvp in tile.front)
                    outTile.front[kvp.Key] = kvp.Value;
                foreach (var kvp in tile.alwaysFront)
                    outTile.alwaysFront[kvp.Key] = kvp.Value;
            }
            return outTile;
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