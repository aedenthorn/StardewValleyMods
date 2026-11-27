using Microsoft.Xna.Framework;

namespace DMT.Data
{
    public class DynamicTile
    {
        public List<string>? locations;
        public List<string> Locations
        {
            get => locations ??= [];
            set => locations = value;
        }

        public List<string>? layers;
        public List<string> Layers
        {
            get => layers ??= [];
            set => layers = value;
        }

        public List<string>? tileSheets;
        public List<string> TileSheets
        {
            get => tileSheets ??= [];
            set => tileSheets = value;
        }

        public List<string>? tileSheetPaths;
        public List<string> TileSheetsPaths
        {
            get => tileSheetPaths ??= [];
            set => tileSheetPaths = value;
        }

        public List<int>? indexes;
        public List<int> Indexes
        {
            get => indexes ??= [];
            set => indexes = value;
        }

        public List<Rectangle>? rectangles;
        public List<Rectangle> Rectangles
        {
            get => rectangles ??= [];
            set => rectangles = value;
        }

        public List<Vector2>? tiles;
        public List<Vector2> Tiles
        {
            get => tiles ??= [];
            set => tiles = value;
        }

        public Dictionary<string, string>? properties; //<- Converted to DynamicTileProperty at runtime
        public Dictionary<string, string> Properties
        {
            get => properties ??= [];
            set => properties = value;
        }

        public List<DynamicTileProperty>? actions;
        public List<DynamicTileProperty> Actions
        {
            get => actions ??= [];
            set => actions = value;
        }
    }
}
