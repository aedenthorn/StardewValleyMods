using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Survivors
{
    public partial class ModEntry
    {

        public static Tile GetMonsterPlayerTile(Tile tile, Monster m, GameLocation location)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || location != openWorldLocation)
                return tile;
            var cp = GetPlayerChunk(m.Player);
            if (!cachedChunks.TryGetValue(cp, out var chunk))
            {
                chunk = CacheChunk(cp, false);
            }
            var pp = m.Player.TilePoint;
            return chunk.tiles["Back"][IntToLocalTileXNoDrawCheck(pp.X), IntToLocalTileXNoDrawCheck(pp.Y)];
        }
        private static void DontRemoveMonster(NetCollection<NPC> npcs, Monster monster, GameLocation location)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || location != openWorldLocation || monster.Position.X < 0f || monster.Position.X > Config.OpenWorldSize * 64 || monster.Position.Y < 0f || monster.Position.Y > Config.OpenWorldSize * 64)
                npcs.Remove(monster);
        }
        public static int IntToLocalX(int value)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation != openWorldLocation)
                return value;
            int v = value % (openWorldChunkSize * 64);
            return v;
        }
        public static int IntToLocalTileXNoDrawCheck(int value)
        {
            int v = value % (openWorldChunkSize);
            return v;
        }
        public static int IntToLocalY(int value)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation != openWorldLocation)
                return value; 
            int v = value % (openWorldChunkSize * 64) + ChunkDisplayOffset(value);
            return v;
        }
        
        public static float FloatToLocalX(float value)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation != openWorldLocation)
                return value;
            float v = value % (openWorldChunkSize * 64);
            return v;
        }
        public static float FloatToLocalXTile(float value)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation != openWorldLocation)
                return value;
            float v = value % openWorldChunkSize;
            return v;
        }
        public static float FloatToLocalYTile(float value)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation != openWorldLocation)
                return value;
            float v = value % openWorldChunkSize + ChunkDisplayOffsetTile(value);
            return v;
        }
        public static float FloatToLocalY(float value)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation != openWorldLocation)
                return value;
            float v = value % (openWorldChunkSize * 64) + ChunkDisplayOffset(value);
            return v;
        }

        public static float GetBushDrawLayer(float value, Bush bush)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || bush.Location != openWorldLocation)
                return value;
            return (IntToLocalY(bush.getBoundingBox().Center.Y) + 48) / 10000f - FloatToLocalX(bush.Tile.X) / 1000000f;
        }
        public static float GetChestDrawLayer(float value, Chest chest, int x, int y)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || chest.Location != openWorldLocation)
                return value;
            float draw_x = (float)x;
            float draw_y = (float)y;
            if (chest.localKickStartTile != null)
            {
                draw_x = Utility.Lerp(chest.localKickStartTile.Value.X, draw_x, chest.kickProgress);
                draw_y = Utility.Lerp(chest.localKickStartTile.Value.Y, draw_y, chest.kickProgress);
            }
            return Math.Max(0f, ((draw_y % openWorldChunkSize + 1f) * 64f + ChunkDisplayOffset(value) - 24f) / 10000f) + draw_x % openWorldChunkSize * 1E-05f;
        }
        public static float GetObjectDrawLayer(float value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || obj.Location != openWorldLocation)
                return value;
            return Math.Max(0f, (float)((y % openWorldChunkSize + 1) * 64 - 24) / 10000f) + (float)x % openWorldChunkSize * 1E-05f;
        }
        public static float GetObjectDrawLayer2(float value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || obj.Location != openWorldLocation)
                return value;
            return Math.Max(0f, (float)((y % openWorldChunkSize + 1) * 64 + ChunkDisplayOffset(y) + 2) / 10000f) + (float)x % openWorldChunkSize / 1000000f;
        }
        public static float GetObjectDrawLayer3(float value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || obj.Location != openWorldLocation)
                return value;
            return (float)((y % openWorldChunkSize + ChunkDisplayOffset(y) + 1) * 64) / 10000f + (obj.TileLocation.X % openWorldChunkSize) / 50000f; ;
        }
        public static Rectangle GetObjectBoundingBox(Rectangle value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || obj.Location != openWorldLocation)
                return value;
            value.Location = new(value.Location.X % openWorldChunkSize, value.Location.Y % openWorldChunkSize + ChunkDisplayOffset(y));
            return value;
        }

        public void ResetChunkTiles()
        {
            int size = Config.OpenWorldSize / openWorldChunkSize;
            cachedChunks = new();
        }

        public static Tile[,] GetChunkTiles(string layer, int cx, int cy)
        {
            if (cachedChunks.TryGetValue(new Point(cx, cy), out var chunk) && chunk.tiles.TryGetValue(layer, out var tiles))
            {
                return tiles;
            }
            return null;
        }
    }
}