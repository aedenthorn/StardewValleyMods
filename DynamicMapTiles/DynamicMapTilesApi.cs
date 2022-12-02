using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using xTile.Tiles;

namespace DynamicMapTiles
{
    public interface IDynamicMapTilesApi
    {
        public bool PushTiles(GameLocation location, List<(Point, Tile)> tiles, int dir, Farmer farmer);
    }
    public class DynamicMapTilesApi : IDynamicMapTilesApi
    {
        public bool PushTiles(GameLocation location, List<(Point, Tile)> tiles, int dir, Farmer farmer)
        {
            return ModEntry.PushTiles(location, tiles, dir, farmer);
        }
    }
}