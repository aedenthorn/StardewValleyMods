using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public class WorldTile
    {
        public Dictionary<Point, Object> objects = new Dictionary<Point, Object>();
        public Dictionary<Point, TerrainFeature> terrainFeatures = new Dictionary<Point, TerrainFeature>();
        public Dictionary<Point, Tile> back = new Dictionary<Point, Tile>();
        public Dictionary<Point, Tile> buildings = new Dictionary<Point, Tile>();
        public Dictionary<Point, Tile> front = new Dictionary<Point, Tile>();
        public Dictionary<Point, Tile> alwaysFront = new Dictionary<Point, Tile>();
        public int priority;
    }
}