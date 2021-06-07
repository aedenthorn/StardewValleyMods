using Microsoft.Xna.Framework;
using System;
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
        public List<TileInfo> tiles { get; set; } = new List<TileInfo>();
        public long frameInterval { get; set; }
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

        public int tileIndex { get; set; } = -1;
        public string tileSheet { get; set; } = "";
        public BlendMode blendMode { get; set; } = BlendMode.Alpha;
        public Dictionary<string, string> properties { get; set; } = new Dictionary<string, string>();

    }
}