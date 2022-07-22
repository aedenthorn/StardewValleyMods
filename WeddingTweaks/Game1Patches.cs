using StardewModdingAPI;
using StardewValley;

namespace WeddingTweaks
{
    public static class Game1Patches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static string lastGotCharacter = null;

        public static void prepareSpouseForWedding_Prefix(Farmer farmer)
        {
            if(farmer.spouse == null)
            {
                Monitor.Log($"Spouse for {farmer.Name} is null");
                foreach (string name in farmer.friendshipData.Keys)
                {
                    if (farmer.friendshipData[name].IsEngaged())
                    {
                        Monitor.Log($"Setting spouse for {farmer.Name} to fianc(e)e {name}");
                        farmer.spouse = name;
                        break;
                    }
                }
            }
        }
        public static void getCharacterFromName_Prefix(string name)
        {
            if (ModEntry.startingLoadActors)
                lastGotCharacter = name;
        }
    }
}
