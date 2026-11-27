using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace Survivors
{
    public class WorldChunk
    {
        public Dictionary<Vector2, Object> objects = new();
        public Dictionary<Vector2, Object> overlayObjects = new();
        public Dictionary<Vector2, MonsterSpawn> monsters = new();
        public Dictionary<Vector2,TerrainFeature> terrainFeatures = new();
        public Dictionary<Vector2,LargeTerrainFeature> largeTerrainFeatures = new();
        public Dictionary<Vector2,ResourceClump> resourceClumps = new();
        public Dictionary<string, Tile[,]> tiles = new();
        public int priority;
        public bool cached;
        public bool built;
    }
}