using StardewModdingAPI;
using StardewValley;
using System;

namespace HugsAndKisses
{
    public static class NPCPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }

        public static bool NPC_checkAction_Prefix(ref NPC __instance, ref Farmer who, GameLocation l, ref bool __result)
        {

            try
            {
                if (!Config.EnableMod || __instance.IsInvisible || __instance.isSleeping.Value || !who.canMove || who.checkForQuestComplete(__instance, -1, -1, who.ActiveObject, null, -1, 5) || (who.pantsItem.Value?.ParentSheetIndex == 15 && (__instance.Name.Equals("Lewis") || __instance.Name.Equals("Marnie"))) || (__instance.Name.Equals("Krobus") && who.hasQuest(28)) || !who.IsLocalPlayer)
                    return true;

                if (
                    (who.friendshipData.ContainsKey(__instance.Name) && (who.friendshipData[__instance.Name].IsMarried() || who.friendshipData[__instance.Name].IsEngaged())) ||
                    ((__instance.datable.Value || Config.AllowNonDateableNPCsToHugAndKiss) && who.friendshipData.ContainsKey(__instance.Name) && !who.friendshipData[__instance.Name].IsMarried() && !who.friendshipData[__instance.Name].IsEngaged() && ((who.friendshipData[__instance.Name].IsDating() && Config.DatingKisses) || (who.getFriendshipHeartLevelForNPC(__instance.Name) >= Config.HeartsForFriendship && Config.FriendHugs)))
                    )
                {
                    __instance.faceDirection(-3);

                    if (__instance.Sprite.CurrentAnimation == null && !__instance.hasTemporaryMessageAvailable() && __instance.currentMarriageDialogue.Count == 0 && __instance.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !__instance.isMoving() && who.ActiveObject == null)
                    {
                        bool kissing = who.friendshipData[__instance.Name].IsDating() || who.friendshipData[__instance.Name].IsMarried() || who.friendshipData[__instance.Name].IsEngaged();
                        Monitor.Log($"{who.Name} {(kissing ? "kissing" : "hugging")} {__instance.Name}");

                        if (kissing && __instance.hasBeenKissedToday.Value && !Config.UnlimitedDailyKisses)
                        {
                            Monitor.Log($"already kissed {__instance.Name}");
                            return false;
                        }

                        __instance.faceGeneralDirection(who.getStandingPosition(), 0, false);
                        who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                        if (__instance.FacingDirection == 3 || __instance.FacingDirection == 1)
                        {
                            if (kissing)
                            {
                                Kissing.PlayerNPCKiss(who, __instance);
                            }
                            else
                            {
                                Kissing.PlayerNPCHug(who, __instance);
                            }
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
    }
}
