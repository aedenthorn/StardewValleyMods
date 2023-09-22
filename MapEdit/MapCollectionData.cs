using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using xTile.Tiles;

namespace MapEdit
{
    public class MapCollectionData
    {
        public Dictionary<string, MapData> mapDataDict = new Dictionary<string, MapData>();
    }

    public class MapData
    {
        public Dictionary<string, TileSheetData> customSheets = new Dictionary<string, TileSheetData>();
        public Dictionary<Vector2, TileLayers> tileDataDict = new Dictionary<Vector2, TileLayers>();
    }
    
    public class TileSheetData
    {
        public string path;
        public int width;
        public int height;
    }

    public class TileLayers
    {
        public Dictionary<string, TileLayerData> tileDict = new Dictionary<string, TileLayerData>();
        public TileLayers()
        {
        }
        public TileLayers(Dictionary<string, Tile> currentTile)
        {
            foreach (var kvp in currentTile)
            {
                tileDict[kvp.Key] = new TileLayerData(kvp.Value);
            }
        }
    }

    public class TileLayerData
    {
        public List<TileInfo> tiles = new List<TileInfo>();
        public long frameInterval;
        public TileLayerData()
        { 
        }
        public TileLayerData(Tile tile)
        {
            if(tile is StaticTile)
            {
                tiles.Add(MakeTileInfo(tile));
            }
            else if (tile is AnimatedTile)
            {
                foreach(StaticTile frame in (tile as AnimatedTile).TileFrames)
                {
                    tiles.Add(MakeTileInfo(frame));
                }
                frameInterval = (tile as AnimatedTile).FrameInterval;
            }
        }

        private TileInfo MakeTileInfo(Tile tile)
        {
            TileInfo ti = new TileInfo()
            {
                tileIndex = tile.TileIndex,
                tileSheet = tile.TileSheet.Id,
                blendMode = tile.BlendMode
            };
            foreach (var prop in tile.TileIndexProperties)
            {
                ti.properties.Add(prop.Key, prop.Value.ToString());
            }
            return ti;
        }
    }

    public class TileInfo
    {
        public TileInfo()
        {
        }

        public int tileIndex = -1;
        public string tileSheet = "";
        public BlendMode blendMode = BlendMode.Alpha;
        public Dictionary<string, string> properties = new Dictionary<string, string>();

    }
}