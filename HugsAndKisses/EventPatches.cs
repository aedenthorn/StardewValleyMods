using StardewModdingAPI;
using StardewValley;
using System;

namespace HugsAndKisses
{
    public static class EventPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static bool Event_playSound_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (args[1] == "dwop" && @event.isWedding && ModEntry.Config.CustomKissSound.Length > 0 && Kissing.kissEffect != null)
                {
                    Kissing.kissEffect.Play();
                    int num = @event.CurrentCommand;
                    @event.CurrentCommand = num + 1;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_playSound_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
    }
}