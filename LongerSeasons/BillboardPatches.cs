using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static void Billboard_Postfix(Billboard __instance, bool dailyQuest)
        {
            if (dailyQuest || Game1.dayOfMonth < 29)
                return;
            __instance.calendarDays = new List<ClickableTextureComponent>();
            Dictionary<int, List<NPC>> birthdays = __instance.GetBirthdays();

            int startDate = (Game1.dayOfMonth - 1) / 28 * 28 + 1;
            for (int day = startDate; day <= startDate + 27; day++)
            {
                int l = (day - 1) % 28 + 1;
                List<Billboard.BillboardEvent> curEvents = __instance.GetEventsForDay(day, birthdays);
                if (curEvents.Count > 0)
                {
                    __instance.calendarDayData[day] = new Billboard.BillboardDay(curEvents.ToArray());
                }
                int index = day - 1;
                __instance.calendarDays.Add(new ClickableTextureComponent(day.ToString(), new Rectangle(__instance.xPositionOnScreen + 152 + index % 7 * 32 * 4, __instance.yPositionOnScreen + 200 + index / 7 * 32 * 4, 124, 124), string.Empty, string.Empty, null, Rectangle.Empty, 1f, false)
                {
                    myID = day,
                    rightNeighborID = ((day % 7 != 0) ? (day + 1) : (-1)),
                    leftNeighborID = ((day % 7 != 1) ? (day - 1) : (-1)),
                    downNeighborID = day + 7,
                    upNeighborID = ((day > 7) ? (day - 7) : (-1))
                });
            }
        }

        private static void Billboard_draw_Postfix(Billboard __instance, Texture2D ___billboardTexture, bool ___dailyQuestBoard, SpriteBatch b)
        {
            if (___dailyQuestBoard)
                return;
            int add = Game1.dayOfMonth / 28 * 28;
            for (int i = 0; i < __instance.calendarDays.Count; i++)
            {
                if (Game1.dayOfMonth > add + i + 1)
                {
                    b.Draw(Game1.staminaRect, __instance.calendarDays[i].bounds, Color.Gray * 0.25f);
                }
                else if (Game1.dayOfMonth == add + i + 1)
                {
                    int offset = (int)(4f * Game1.dialogueButtonScale / 8f);
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), __instance.calendarDays[i].bounds.X - offset, __instance.calendarDays[i].bounds.Y - offset, __instance.calendarDays[i].bounds.Width + offset * 2, __instance.calendarDays[i].bounds.Height + offset * 2, Color.Blue, 4f, false, -1f);
                }
                else if (i + add >= Config.DaysPerMonth)
                {
                    b.Draw(Game1.staminaRect, __instance.calendarDays[i].bounds, Color.White);
                }
            }
        }


        public static IEnumerable<CodeInstruction> Billboard_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Billboard.draw");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == typeof(Game1).GetField(nameof(Game1.dayOfMonth), BindingFlags.Public | BindingFlags.Static) && codes[i + 1].opcode == OpCodes.Ldloc_2 && codes[i + 2].opcode == OpCodes.Ldc_I4_1 && codes[i + 3].opcode == OpCodes.Add && codes[i + 4].opcode == OpCodes.Ble_S)
                {
                    SMonitor.Log("Removing greyed out date covering");
                    codes[i + 2] = new CodeInstruction(OpCodes.Ldc_I4, 29);
                }
            }

            return codes.AsEnumerable();
        }
    }
}