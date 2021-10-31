using StardewModdingAPI;

namespace FreeLove
{
    public class Integrations
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;
        public static IKissingAPI kissingAPI;
        internal static ICustomSpouseRoomsAPI customSpouseRoomsAPI;
        internal static IBedTweaksAPI bedTweaksAPI;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }
        public static void LoadModApis()
        {
            kissingAPI = Helper.ModRegistry.GetApi<IKissingAPI>("aedenthorn.FreeKisses");
            bedTweaksAPI = Helper.ModRegistry.GetApi<IBedTweaksAPI>("aedenthorn.BedTweaks");
            customSpouseRoomsAPI = Helper.ModRegistry.GetApi<ICustomSpouseRoomsAPI>("aedenthorn.CustomSpouseRooms");

            if (kissingAPI != null)
            {
                Monitor.Log("Kissing API loaded");
            }
            if (bedTweaksAPI != null)
            {
                Monitor.Log("BedTweaks API loaded");
            }
            if (customSpouseRoomsAPI != null)
            {
                Monitor.Log("Custom Spouse Room API loaded");
            }

        }
    }
}