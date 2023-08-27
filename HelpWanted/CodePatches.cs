using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace HelpWanted
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.CanAcceptDailyQuest))]
        public class Game1_CanAcceptDailyQuest_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                try
                {
                    __result = Game1.questOfTheDay != null && !Game1.player.acceptedDailyQuest.Value && Game1.questOfTheDay.questDescription != null && Game1.questOfTheDay.questDescription.Length != 0;
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }
        
        [HarmonyPatch(typeof(DescriptionElement), nameof(DescriptionElement.loadDescriptionElement))]
        public class DescriptionElement_loadDescriptionElement_Patch
        {
            public static bool Prefix(DescriptionElement __instance, ref string __result)
            {
                if (!Config.ModEnabled)
                    return true;
                try
                {
                    DescriptionElement temp = new DescriptionElement(__instance.xmlKey, __instance.param);
                    for (int i = 0; i < temp.param.Count; i++)
                    {
                        if (temp.param[i] is DescriptionElement)
                        {
                            DescriptionElement d = temp.param[i] as DescriptionElement;
                            temp.param[i] = d.loadDescriptionElement();
                        }
                        if (temp.param[i] is Object)
                        {
                            string objectInformation;
                            Game1.objectInformation.TryGetValue((temp.param[i] as Object).ParentSheetIndex, out objectInformation);
                            temp.param[i] = objectInformation.Split('/', StringSplitOptions.None)[4];
                        }
                        if (temp.param[i] is Monster)
                        {
                            DescriptionElement d2;
                            if ((temp.param[i] as Monster).Name.Equals("Frost Jelly"))
                            {
                                d2 = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13772");
                                temp.param[i] = d2.loadDescriptionElement();
                            }
                            else
                            {
                                d2 = new DescriptionElement("Data\\Monsters:" + (temp.param[i] as Monster).Name);
                                temp.param[i] = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? (d2.loadDescriptionElement().Split('/', StringSplitOptions.None).Last<string>() + "s") : d2.loadDescriptionElement().Split('/', StringSplitOptions.None).Last<string>());
                            }
                            temp.param[i] = d2.loadDescriptionElement().Split('/', StringSplitOptions.None).Last<string>();
                        }
                        if (temp.param[i] is NPC)
                        {
                            DescriptionElement d3 = new DescriptionElement("Data\\NPCDispositions:" + (temp.param[i] as NPC).Name);
                            temp.param[i] = d3.loadDescriptionElement().Split('/', StringSplitOptions.None).Last<string>();
                        }
                    }
                    return true;
                }
                catch
                {
                    __result = string.Empty;
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(Billboard), nameof(Billboard.draw))]
        public class Billboard_draw_Patch
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
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetQuestDays))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        public static int GetQuestDays(int days)
        {
            return !Config.ModEnabled ? days: Config.QuestDays;
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