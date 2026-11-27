using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using xTile.Layers;

namespace DynamicMapTiles
{
    public interface IDynamicMapTilesApi
    {
        public bool TriggerActions(List<Layer> layers, Farmer farmer, Point tilePos, List<string> suffixes);
    }
    public class DynamicMapTilesApi : IDynamicMapTilesApi
    {
        public bool TriggerActions(List<Layer> layers, Farmer farmer, Point tilePos, List<string> suffixes)
        {
            return ModEntry.TriggerActions(layers, farmer, tilePos, suffixes);
        }
    }
}