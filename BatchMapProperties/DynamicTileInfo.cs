using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DynamicMapTiles
{
    public class DynamicTileInfo
    {
        public List<string> locations;
        public List<string> layers;
        public List<string> tileSheets;
        public List<int> indexes;
        public List<Rectangle> rectangles;
        public Dictionary<string, string> properties;
    }
}