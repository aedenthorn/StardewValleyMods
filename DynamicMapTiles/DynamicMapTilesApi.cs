using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Tiles;

namespace DynamicMapTiles
{
    public interface IDynamicMapTilesApi
    {
        public void PushTile(GameLocation location, Tile tile, int dir, Point start, Farmer farmer);
    }
    public class DynamicMapTilesApi : IDynamicMapTilesApi
    {
        public void PushTile(GameLocation location, Tile tile, int dir, Point start, Farmer farmer)
        {
            ModEntry.PushTile(location, tile, dir, start, farmer);
        }
    }
}