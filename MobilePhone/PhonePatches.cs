using Harmony;
using StardewModdingAPI;
using StardewValley;
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
        internal static bool Farmer_changeFriendship_prefix(int amount, NPC n)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not change friendship with {n.name} by {amount}");
                return false;
            }
            return true;
        }        
        internal static bool Event_command_prefix(Event __instance, string[] split)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not execute event command {string.Join(" ",split)}");
                __instance.CurrentCommand++;
                return false;
            }
            return true;
        }
        internal static bool Event_endBehaviors_prefix(Event __instance, string[] split)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not execute end behaviors {string.Join(" ", split)}");
                __instance.exitEvent();
                return false;
            }
            return true;
        }
    }
}