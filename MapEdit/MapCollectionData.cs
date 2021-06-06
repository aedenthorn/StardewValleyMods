using Microsoft.Xna.Framework;
using System.Collections.Generic;
using xTile.Tiles;

namespace MapEdit
{
    public class MapCollectionData
    {
        public Dictionary<string, MapData> mapDataDict { get; set; } = new Dictionary<string, MapData>();
    }

    public class MapData
    {
        public Dictionary<Vector2, TileData> tileDataDict { get; set; } = new Dictionary<Vector2, TileData>();
    }

    public class TileData
    {
        public Dictionary<string, TileLayerData> tileDict { get; set; } = new Dictionary<string, TileLayerData>();
        public TileData()
        {
        }
        public TileData(Dictionary<string, Tile> currentTile)
        {
            foreach (var kvp in currentTile)
            {
                tileDict[kvp.Key] = new TileLayerData(kvp.Value);
            }
        }
    }

    public class TileLayerData
    {
        public int index { get; set; } = -1;
        public string tileSheet { get; set; } = "";
        public BlendMode blendMode { get; set; } = BlendMode.Alpha;
        public Dictionary<string, string> properties { get; set; } = new Dictionary<string, string>();
        public TileLayerData()
        { 
        }
        public TileLayerData(Tile tile)
        {
            index = tile.TileIndex;
            tileSheet = tile.TileSheet.Id;
            blendMode = tile.BlendMode;
            foreach(var prop in tile.TileIndexProperties)
            {
                properties.Add(prop.Key, prop.Value.ToString());
            }
        }
    }
}