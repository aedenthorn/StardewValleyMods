using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System;
using StardewValley.Quests;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework;

namespace HelpWanted
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Billboard), nameof(Billboard.draw))]
        public class Billboard_Patch
        {
            public static bool Prefix(bool ___dailyQuestBoard)
            {
                if (!Config.ModEnabled || !___dailyQuestBoard || Game1.activeClickableMenu.GetType() != typeof(Billboard))
                    return true;
                Game1.activeClickableMenu = new OrdersBillboard();
                return false;
            }
        }
        [HarmonyPatch(typeof(Billboard), nameof(Billboard.receiveLeftClick))]
        public class Billboard_receiveLeftClick_Patch
        {
            public static void Postfix(Billboard __instance, bool ___dailyQuestBoard, int x, int y)
            {
                if (!Config.ModEnabled || !___dailyQuestBoard || Game1.activeClickableMenu is not OrdersBillboard)
                    return;
                __instance.acceptQuestButton.visible = true;
                if (__instance.acceptQuestButton.containsPoint(x, y))
                {
                    Game1.player.acceptedDailyQuest.Set(false);
                    Game1.questOfTheDay = null;
                    OrdersBillboard.questDict.Remove(OrdersBillboard.showingQuest);
                    OrdersBillboard.ccList.RemoveAll(c => c.myID == OrdersBillboard.showingQuest);
                    OrdersBillboard.questBillboard = null;
                }
                else if (__instance.upperRightCloseButton.containsPoint(x, y))
                {
                    OrdersBillboard.questBillboard = null;
                }
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Billboard.receiveLeftClick");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldfld && codes[i + 1].opcode == OpCodes.Ldc_I4_2 && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Quest), nameof(Quest.daysLeft)))
                    {
                        SMonitor.Log($"replacing days left with method");
                        codes[i + 1].opcode = OpCodes.Call;
                        codes[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetQuestDays));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        public static int GetQuestDays()
        {
            return !Config.ModEnabled ? 2 : Config.QuestDays;
        }

        [HarmonyPatch(typeof(Utility), nameof(Utility.getRandomItemFromSeason))]
        public class Utility_getRandomItemFromSeason_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Utility.getRandomItemFromSeason");

                var codes = new List<CodeInstruction>(instructions);
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_1));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetRandomItem))));

                return codes.AsEnumerable();
            }
            public static void Prefix(ref int randomSeedAddition)
            {
                if (!Config.ModEnabled || !gettingQuestDetails)
                    return;
                randomSeedAddition += random.Next();

            }
        }
        [HarmonyPatch(typeof(ItemDeliveryQuest), nameof(ItemDeliveryQuest.loadQuestInfo))]
        public class ItemDeliveryQuest_loadQuestInfo_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ItemDeliveryQuest.loadQuestInfo");

                var codes = new List<CodeInstruction>(instructions);

                bool start = false;
                bool found1 = false;
                bool found2 = false;
                for(int i = 0; i < codes.Count; i++)
                {
                    if (start && !found1 && codes[i].opcode == OpCodes.Ldc_R8)
                    {
                        codes[i].operand = -0.1;
                        found1 = true;
                    }
                    else if (!start && codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Cooking")
                    {
                        start = true;
                    }
                    else if (!found2 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Utility), nameof(Utility.possibleCropsAtThisTime)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetPossibleCrops))));
                        i++;
                        found2 = true;
                    }
                    if (found1 && found2)
                        break;
                }

                return codes.AsEnumerable();
            }
        }

    }
}