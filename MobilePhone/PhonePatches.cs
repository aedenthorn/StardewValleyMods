using StardewModdingAPI;
using System;

namespace MobilePhone
{
    internal class PhonePatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        internal static bool Reminiscing_Override_prefix()
        {
            if (ModEntry.isReminiscing)
                return false;
            return true;
        }
        internal static bool Event_endBehaviors_prefix(StardewValley.Event __instance)
        {
            if (ModEntry.isReminiscing)
            {
                __instance.exitEvent();
                return false;
            }
            return true;
        }
    }
}