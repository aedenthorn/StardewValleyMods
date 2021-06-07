using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using xTile.Tiles;

namespace MapEdit
{
    public class MapCollectionDataOld
    {
        public Dictionary<string, MapDataOld> mapDataDict { get; set; } = new Dictionary<string, MapDataOld>();
    }

    public class MapDataOld
    {
        public Dictionary<Vector2, TileDataOld> tileDataDict { get; set; } = new Dictionary<Vector2, TileDataOld>();
    }

    public class TileDataOld
    {
        public Dictionary<string, TileLayerDataOld> tileDict { get; set; } = new Dictionary<string, TileLayerDataOld>();
        public TileDataOld()
        {
        }
    }

    public class TileLayerDataOld
    {
        public int tiles = 0;
        public int index = 0;
        public string tileSheet = "";
        public BlendMode blendMode = BlendMode.Alpha;
        public Dictionary<string, string> properties = new Dictionary<string, string>();
        public TileLayerDataOld()
        { 
        }
    }
}