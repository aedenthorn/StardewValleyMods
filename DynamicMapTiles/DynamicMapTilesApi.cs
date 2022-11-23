using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Tiles;

namespace DynamicMapTiles
{
    public interface IDynamicMapTilesApi
    {
        public void PushTile(GameLocation location, Tile tile, int dir, Point start, string sound = null);
    }
    public class DynamicMapTilesApi : IDynamicMapTilesApi
    {
        public void PushTile(GameLocation location, Tile tile, int dir, Point start, string sound = null)
        {
            ModEntry.PushTile(location, tile, dir, start, sound);
        }
    }
}