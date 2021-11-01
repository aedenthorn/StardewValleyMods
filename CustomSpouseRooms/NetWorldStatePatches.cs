using StardewModdingAPI;
using StardewValley;

namespace CustomSpouseRooms
{
    public static class NetWorldStatePatches
    {
        private static IMonitor Monitor;

        public static ModConfig Config;
        private static IModHelper Helper;
        public static string lastGotCharacter = null;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }

        public static bool hasWorldStateID_Prefix(ref string id, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;
            if(id == "sebastianFrogReal")
            {
                Monitor.Log($"Allowing frogs");
                id = "sebastianFrog";
                return true;
            }
            if(id == "sebastianFrog")
            {
                Monitor.Log($"Preventing frogs");
                __result = false;
                return false;
            }
            return true;
        }
    }
}
