using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomGiftLimits
{
    public partial class ModEntry
    {
        public enum FriendshipLevel
        {
            stranger,
            friend,
            dating,
            spouse
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.tryToReceiveActiveObject))]
        public static class NPC_tryToReceiveActiveObject_Patch
        {
            public static void Prefix(NPC __instance, ref Farmer who, Dictionary<string, string> ___dialogue, ref List<int> __state)
            {
                if (!Config.ModEnabled || !Game1.NPCGiftTastes.ContainsKey(__instance.Name))
                    return;

                FriendshipLevel level = FriendshipLevel.stranger;
                int perDay = Config.OrdinaryGiftsPerDay;
                int perWeek = Config.OrdinaryGiftsPerWeek;
                if (who.friendshipData.TryGetValue(__instance.Name, out Friendship f))
                {
                    if (f.IsMarried() || f.IsRoommate())
                    {
                        level = FriendshipLevel.spouse;
                        perDay = Config.SpouseGiftsPerDay;
                        perWeek = Config.SpouseGiftsPerWeek;
                    }
                    else if (f.IsDating())
                    {
                        level = FriendshipLevel.dating;
                        perDay = Config.DatingGiftsPerDay;
                        perWeek = Config.DatingGiftsPerWeek;
                    }
                    else if (f.Points >= 1500)
                    {
                        level = FriendshipLevel.friend;
                        perDay = Config.FriendGiftsPerDay;
                        perWeek = Config.FriendGiftsPerWeek;
                    }
                }

                SMonitor.Log($"Gift to {level} {__instance.Name}");
                __state = new List<int> {
                    who.friendshipData[__instance.Name].GiftsToday,
                    who.friendshipData[__instance.Name].GiftsThisWeek,
                    0,
                    0
                };
                if (perDay < 0 || who.friendshipData[__instance.Name].GiftsToday < perDay)
                {
                    who.friendshipData[__instance.Name].GiftsToday = 0;
                }
                else
                {
                    who.friendshipData[__instance.Name].GiftsToday = 1;
                    __state[2] = 1; // flag to say we set it to 1
                }
                if (perWeek < 0 || who.friendshipData[__instance.Name].GiftsThisWeek < perWeek)
                {
                    who.friendshipData[__instance.Name].GiftsThisWeek = 0;
                }
                else
                {
                    who.friendshipData[__instance.Name].GiftsThisWeek = 2;
                    __state[3] = 1; // flag to say we set it to 2
                }
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling NPC.tryToReceiveActiveObject");

                bool found1 = false;
                bool found2 = false;

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 13 && codes[i].opcode == OpCodes.Callvirt && codes[i + 1].opcode == OpCodes.Ldc_I4_2 && codes[i + 2].opcode == OpCodes.Blt_S  && codes[i + 5].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Friendship), nameof(Friendship.GiftsThisWeek)) && (MethodInfo)codes[i + 5].operand == AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.spouse)))
                    {
                        SMonitor.Log("Removing spouse infinite gifts per week");
                        for(int j = 0; j < 11; j++)
                        {
                            codes[i + 3 + j].opcode = OpCodes.Nop;
                            codes[i + 3 + j].operand = null;
                        }
                        found1 = true;
                    }
                    else if (!found2 && codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Strings\\StringsFromCSFiles:NPC.cs.3987")
                    {
                        SMonitor.Log("Changing max per week message");
                            codes[i + 3].opcode = OpCodes.Call;
                            codes[i + 3].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxGiftsPerWeek));
                            codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_1));
                            codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(NPC __instance, ref Farmer who, List<int> __state)
            {
                if (__state != null && __state.Count > 0)
                {
                    if (who.friendshipData[__instance.Name].GiftsToday == 1 && __state[2] == 0) // set to 0, giftstoday was increased
                    {
                        who.friendshipData[__instance.Name].GiftsToday = __state[0] + 1;
                    }
                    else
                    {
                        who.friendshipData[__instance.Name].GiftsToday = __state[0];
                    }
                    if (who.friendshipData[__instance.Name].GiftsThisWeek == 1 && __state[3] == 0) // set to 0, gifts this week was increased
                    {
                        who.friendshipData[__instance.Name].GiftsThisWeek = __state[1] + 1;
                    }
                    else
                    {
                        who.friendshipData[__instance.Name].GiftsThisWeek = __state[1];
                    }
                    SMonitor.Log($"gifts today {who.friendshipData[__instance.Name].GiftsToday}");
                    SMonitor.Log($"gifts this week {who.friendshipData[__instance.Name].GiftsThisWeek}");
                }
            }
        }

        private static int GetMaxGiftsPerWeek(NPC npc, Farmer who)
        {
            if (!Config.ModEnabled)
                return 2;
            int perWeek = Config.OrdinaryGiftsPerWeek;
            if (who.friendshipData.TryGetValue(npc.Name, out Friendship f))
            {
                if (f.IsMarried() || f.IsRoommate())
                {
                    perWeek = Config.SpouseGiftsPerWeek;
                }
                else if (f.IsDating())
                {
                    perWeek = Config.DatingGiftsPerWeek;
                }
                else if (f.Points >= 1500)
                {
                    perWeek = Config.FriendGiftsPerWeek;
                }
            }
            return perWeek;
        }

        [HarmonyPatch(typeof(SocialPage), "drawNPCSlot")]
        public static class SocialPage_drawNPCSlot_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling SocialPage.drawNPCSlot");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Callvirt  && codes[i + 5].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 5].operand == AccessTools.Field(typeof(SocialPage), "kidsNames") && (MethodInfo)codes[i + 2].operand == AccessTools.Method(typeof(Friendship), nameof(Friendship.IsMarried)) ) 
                    {
                        SMonitor.Log("replacing gift boxes");
                        codes[i + 2].opcode = OpCodes.Call;
                        codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawGiftAmounts));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_1));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static bool DrawGiftAmounts(Friendship f, SocialPage page, SpriteBatch b, int i)
        {
            if (!Config.ModEnabled)
                return f.IsMarried();

            if (AccessTools.FieldRefAccess<SocialPage, List<string>>(page, "kidsNames").Contains(page.names[i] as string))
                return true;

            int perDay;
            int perWeek;
            if (f.IsMarried() || f.IsRoommate())
            {
                perDay = Config.SpouseGiftsPerDay;
                perWeek = Config.SpouseGiftsPerWeek;
            }
            else if (f.IsDating())
            {
                perDay = Config.DatingGiftsPerDay;
                perWeek = Config.DatingGiftsPerWeek;
            }
            else if (f.Points >= 1500)
            {
                perDay = Config.FriendGiftsPerDay;
                perWeek = Config.FriendGiftsPerWeek;
            }
            else
            {
                perDay = Config.OrdinaryGiftsPerDay;
                perWeek = Config.OrdinaryGiftsPerWeek;
            }

            string day = f.GiftsToday+"";
            string week = f.GiftsThisWeek+"";
            string perDayString = perDay + "";
            string perWeekString = perWeek + "";

            ClickableTextureComponent sprite = AccessTools.FieldRefAccess<SocialPage, List<ClickableTextureComponent>>(page, "sprites")[i];

            Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(page.xPositionOnScreen + 384 + 424, sprite.bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, 0, -1, 0.2f);
            b.Draw(Game1.mouseCursors, new Vector2(page.xPositionOnScreen + 384 + 432, sprite.bounds.Y + 32 + 20), new Rectangle?(new Rectangle(227 + (f.TalkedToToday ? 9 : 0), 425, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

            if (perDay == 0 || perWeek == 0)
                return true;

            Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(page.xPositionOnScreen + 384 + 304, sprite.bounds.Y - 4), new Rectangle(166, 174, 14, 12), Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, 0, -1, 0.2f);

            if (perDay == -1)
            {
                b.DrawString(Game1.smallFont, day, new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 62 - Game1.smallFont.MeasureString(day).X / 2), sprite.bounds.Y + 44), Config.DayColor);
            }
            else {
                b.DrawString(Game1.smallFont, day, new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 31 - Game1.smallFont.MeasureString(day).X / 2), sprite.bounds.Y + 44), Config.DayColor);
                b.DrawString(Game1.smallFont, "/", new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 62 - Game1.smallFont.MeasureString("/").X / 2), sprite.bounds.Y + 45), Config.DayColor);
                b.DrawString(Game1.smallFont, perDayString, new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 93 - Game1.smallFont.MeasureString(perDayString).X / 2), sprite.bounds.Y + 44), Config.DayColor);
            }

            if (perWeek == -1)
            {
                b.DrawString(Game1.smallFont, week, new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 62 - Game1.smallFont.MeasureString(week).X / 2), sprite.bounds.Y + 68), Config.WeekColor);
            }
            else
            {
                b.DrawString(Game1.smallFont, week, new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 31 - Game1.smallFont.MeasureString(week).X / 2), sprite.bounds.Y + 68), Config.WeekColor);
                b.DrawString(Game1.smallFont, "/", new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 62 - Game1.smallFont.MeasureString("/").X / 2), sprite.bounds.Y + 69), Config.WeekColor);
                b.DrawString(Game1.smallFont, perWeekString, new Vector2((float)(page.xPositionOnScreen + 384 + 274 + 93 - Game1.smallFont.MeasureString(perWeekString).X / 2), sprite.bounds.Y + 68), Config.WeekColor);
            }

            return true;
        }
    }
}