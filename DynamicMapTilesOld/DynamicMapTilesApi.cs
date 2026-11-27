using System.Collections.Generic;
using Microsoft.Xna.Framework;
using xTile.Layers;
using StardewValley;

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
