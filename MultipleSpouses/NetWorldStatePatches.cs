using StardewModdingAPI;
using StardewValley;

namespace MultipleSpouses
{
    public static class NetWorldStatePatches
    {
        private static IMonitor Monitor;

        public static ModConfig Config;

        public static string lastGotCharacter = null;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
        }

        public static bool hasWorldStateID_Prefix(ref string id, ref bool __result)
        {
            if(Config.EnableMod && id == "sebastianFrogReal")
            {
                Monitor.Log($"Allowing frogs");
                id = "sebastianFrog";
                return true;
            }
            if(Config.EnableMod && Config.BuildAllSpousesRooms && id == "sebastianFrog")
            {
                Monitor.Log($"Preventing frogs");
                __result = false;
                return false;
            }
            return true;
        }
        public static void getCharacterFromName_Prefix(string name)
        {
            if (EventPatches.startingLoadActors)
                lastGotCharacter = name;
        }
    }
}
