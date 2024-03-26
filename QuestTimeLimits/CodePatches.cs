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
using StardewValley.SpecialOrders;
using StardewValley.GameData.SpecialOrders;
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