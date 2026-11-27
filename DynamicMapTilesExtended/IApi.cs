using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace DMT
{
    public interface IApi
    {
        /// <summary>
        /// Trigger all actions with the given triggers in the current location
        /// </summary>
        /// <param name="layers">The layers on which to check the tiles</param>
        /// <param name="who">The player which triggered the action and on who the action will be performed, may be null</param>
        /// <param name="location">The game location of the tile</param>
        /// <param name="tilePosition">The X Y position of the tile on the map where the trigger originated from</param>
        /// <param name="triggers">The types of triggers to run</param>
        /// <returns>true if any action was triggered, false otherwise</returns>
        public bool TriggerActions(IEnumerable<Layer> layers, Farmer? who, GameLocation location, Point tilePosition, IEnumerable<string> triggers);

        /// <summary>
        /// Add a regular expression which should be interpreted as a global trigger
        /// </summary>
        /// <param name="regex">The regular expression to check any trigger against</param>
        /// <returns>true if the trigger was added, false if a similar regular expression already existed</returns>
        public bool AddGlobalTrigger(string regex);

        /// <summary>
        /// Add a custom action for DMT to run
        /// </summary>
        /// <param name="key">A unique key for content pack authors to use this action</param>
        /// <param name="handler">The function which should be run when this action is triggered</param>
        /// <returns>true if this action was added, false if a similar key already existed</returns>
        public bool RegisterAction(string key, Action<Farmer, string, Tile, Point> handler);
    }
}
