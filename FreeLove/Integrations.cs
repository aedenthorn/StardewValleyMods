using StardewModdingAPI;
using System;

namespace FreeLove
{
    public class Integrations
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;
        public static IKissingAPI kissingAPI;
        public static IBedTweaksAPI bedTweaksAPI;
        public static ICustomSpouseRoomsAPI customSpouseRoomsAPI;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }
        public static void LoadModApis()
        {
            try 
            { 
                kissingAPI = Helper.ModRegistry.GetApi<IKissingAPI>("aedenthorn.HugsAndKisses");
            }
            catch(Exception ex) { Monitor.Log($"kissapi: {ex}"); }
            try 
            { 
                bedTweaksAPI = Helper.ModRegistry.GetApi<IBedTweaksAPI>("aedenthorn.BedTweaks");
            }
            catch { }
            try 
            { 
                customSpouseRoomsAPI = Helper.ModRegistry.GetApi<ICustomSpouseRoomsAPI>("aedenthorn.CustomSpouseRooms");
            }
            catch { }

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
                Monitor.Log("customSpouseRooms API loaded");
            }
        }
    }
}