using System.Collections.ObjectModel;
using System;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewOpenWorld
{
    internal class MyTileArray : TileArray
    {
        private Layer layer;

        public MyTileArray(Layer layer, Tile[,] tiles) : base(layer, tiles)
        {
        }
        public MyTileArray(Layer layer, TileArray tiles) : base(layer, tiles.Array)
        {
            this.layer = layer;
        }

        public new Tile this[int x, int y]
        {
            get
            {
                Tile result;
                if (x < 0 || x >= ModEntry.openWorldSize || y < 0 || y >= ModEntry.openWorldSize)
                {
                    result = null;
                }
                else
                {
                    result = ModEntry.GetTile(layer, x, y);
                }
                return result;
            }
            set
            {
                ModEntry.SetTile(layer, x, y, value);
            }
        }

    }
}