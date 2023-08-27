using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;

namespace QuestTimeLimits
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.SetDuration))]
        public class SpecialOrder_SetDuration_Patch
        {
            public static void Postfix(SpecialOrder __instance)
            {
                if (!Config.ModEnabled || Config.SpecialOrderMult <= 0)
                    return;

                __instance.dueDate.Value = Game1.Date.TotalDays + (int)Math.Round((__instance.dueDate.Value - Game1.Date.TotalDays) * Config.SpecialOrderMult);
                SMonitor.Log($"Set special order quest days left to {__instance.dueDate.Value - Game1.Date.TotalDays}");
            }
        }
        [HarmonyPatch(typeof(Game1), "_newDayAfterFade")]
        public class Game1__newDayAfterFade_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled || Config.SpecialOrderMult > 0)
                    return;
                if (Game1.IsMasterGame)
                {
                    Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
                    var orders = new List<SpecialOrder>(Game1.player.team.specialOrders.ToArray());
                    orders.AddRange(Game1.player.team.availableSpecialOrders);
                    for (int m = 0; m < orders.Count; m++)
                    {
                        SpecialOrder order = orders[m];
                        if (order_data.TryGetValue(order.questKey.Value, out var data))
                        {
                            int days = 7;
                            switch (data.Duration)
                            {
                                case "TwoWeeks":
                                    days = 14;
                                    break;
                                case "Month":
                                    days = WorldDate.DaysPerMonth;
                                    break;
                                case "TwoDays":
                                    days = 2;
                                    break;
                                case "ThreeDays":
                                    days = 3;
                                    break;
                            }
                            order.dueDate.Value = Game1.Date.TotalDays + days;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.dayupdate))]
        public class Farmer_dayupdate_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || Config.DailyQuestMult > 0)
                    return;
                for (int i = __instance.questLog.Count - 1; i >= 0; i--)
                {
                    if (__instance.questLog[i].dailyQuest.Value)
                    {
                        __instance.questLog[i].daysLeft.Value += 1;
                    }
                }
            }
        }     
         
        [HarmonyPatch(typeof(Billboard), nameof(Billboard.receiveLeftClick))]
        public class Billboard_receiveLeftClick_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Billboard.receiveLeftClick");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_2)
                    {
                        SMonitor.Log($"adding method for days multiplier");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(MultiplyQuestDays))));
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}