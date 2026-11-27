using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace FuzzyTime
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw))]
        public class DayTimeMoneyBox_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Building.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi.Name.EndsWith("drawTextWithShadow"))
                    {
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawTextWithShadow));
                        codes.Insert(i, new(OpCodes.Ldarg_0));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.isWithinBounds))]
        public static class DayTimeMoneyBox_isWithinBounds_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y, ref bool __result)
            {
                if (!Config.ModEnabled || !GetBounds(__instance).Contains(x, y))
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.performHoverAction))]
        public static class DayTimeMoneyBox_performHoverAction_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y, ref StringBuilder ____hoverText, StringBuilder ____timeText)
            {
                if (!Config.ModEnabled || !GetBounds(__instance).Contains(x, y))
                    return true;
                ____hoverText.Clear();
                ____hoverText.Append(____timeText.ToString());
                return false;
            }
        }
        public static Rectangle GetBounds(DayTimeMoneyBox box)
        {
            return new Rectangle(box.xPositionOnScreen + 104, box.yPositionOnScreen + 112, 164, 36);
        }
    }
}