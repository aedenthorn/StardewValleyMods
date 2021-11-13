using StardewModdingAPI;
using StardewValley;
using System;

namespace WeddingTweaks
{
    public static class NPCPatches
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

        public static void NPC_engagementResponse_Postfix(NPC __instance, Farmer who, bool asRoommate = false)
        {
            Monitor.Log($"engagement response for {__instance.Name}");
            if (asRoommate)
            {
                Monitor.Log($"{__instance.Name} is roomate");
                return;
            }
            if (!who.friendshipData.ContainsKey(__instance.Name))
            {
                Monitor.Log($"{who.Name} has no friendship data for {__instance.Name}", LogLevel.Error);
                return;
            }
            Friendship friendship = who.friendshipData[__instance.Name];
            WorldDate weddingDate = new WorldDate(Game1.Date);
            weddingDate.TotalDays += Math.Max(1,Config.DaysUntilMarriage);
            while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
            {
                weddingDate.TotalDays++;
            }
            friendship.WeddingDate = weddingDate;
        }
    }
}
