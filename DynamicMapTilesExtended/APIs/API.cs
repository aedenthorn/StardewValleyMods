using DMT.Data;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace DMT.APIs
{
    public class API : IApi
    {
        public bool TriggerActions(IEnumerable<Layer> layers, Farmer? who, GameLocation location, Point tilePosition, IEnumerable<string> triggers)
        {
            return Utils.TriggerActions([.. layers], who, location, tilePosition, [.. triggers]);
        }

        public bool AddGlobalTrigger(string regex) => Triggers.GlobalTriggers.Add(regex);

        public bool RegisterAction(string key, Action<Farmer, string, Tile, Point> handler)
        {
            if (!Data.Actions.ModKeys.Add(key))
                return false;
            ModActions.Add(key, handler);
            return true;
        }
    }
}
