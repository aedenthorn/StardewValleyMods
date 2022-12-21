using Microsoft.Xna.Framework;
using System.Collections.Generic;
using xTile;

namespace StardewOpenWorld
{
    public class Biome
    {
        public Dictionary<Point, ObjData> terrainFeatures;
        public Dictionary<Point, ObjData> objects;
        public Dictionary<Point, TileData> back;
        public Dictionary<Point, TileData> buildings;
        public Dictionary<Point, TileData> front;
        public Dictionary<Point, TileData> alwaysFront;
    }

    public class TileData
    {
        public string tileSheet;
        public int index;
    }

    public class ObjData
    {
        public string type;
        public Dictionary<string, object> fields;
        public Dictionary<string, object> properties;
    }
}