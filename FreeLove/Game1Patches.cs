using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace FreeLove
{
    public static class Game1Patches
    {
        private static IMonitor Monitor;
        public static string lastGotCharacter = null;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static void getCharacterFromName_Prefix(string name)
        {
            if (EventPatches.startingLoadActors)
                lastGotCharacter = name;
        }

        public static void warpCharacter_Prefix(NPC character, GameLocation targetLocation, ref Vector2 position)
        {
            if(ModEntry.Config.EnableMod && targetLocation is FarmHouse)
            {
                foreach(var n in targetLocation.characters)
                {
                    if(Vector2.Distance(n.Tile, position) < 1)
                    {
                        position = Utility.recursiveFindOpenTileForCharacter(character, targetLocation, position, 100, false);
                        break;
                    }
                }
            }
        }
    }
}
