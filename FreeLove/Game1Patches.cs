using StardewModdingAPI;
using StardewValley;

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
    }
}
