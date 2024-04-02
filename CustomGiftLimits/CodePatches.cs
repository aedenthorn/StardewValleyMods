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
                    /*
                     * 
                     * // if ((value != null && value.GiftsThisWeek < 2) || who.spouse == base.Name || this is Child || this.isBirthday() || who.ActiveObject.QualifiedItemId == "(O)StardropTea")
	                 *  IL_18d6: ldloc.1
	                 *  IL_18d7: brfalse.s IL_18e2
                     *
	                 *  IL_18d9: ldloc.1
	                 *  IL_18da: call instance int32 StardewValley.Friendship::get_GiftsThisWeek()
	                 *  IL_18df: ldc.i4.2
	                 *  IL_18e0: blt.s IL_1929
                     *
	                 *  // This is that "who.spouse == base.Name". We want to null out all of these codes
	                 *  IL_18e2: ldloc.0
	                 *  IL_18e3: ldfld class StardewValley.Farmer StardewValley.NPC/'<>c__DisplayClass234_0'::who
	                 *  IL_18e8: callvirt instance string StardewValley.Farmer::get_spouse()
	                 *  IL_18ed: ldarg.0
	                 *  IL_18ee: call instance string StardewValley.Character::get_Name()
	                 *  IL_18f3: call bool [System.Runtime]System.String::op_Equality(string, string)
	                 *  IL_18f8: brtrue.s IL_1929
                     */
                    // For this first if, we want codes[i] to be that ldloc.0
                    if (!found1 && i < codes.Count - 7 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 2].opcode == OpCodes.Callvirt && codes[i + 3].opcode == OpCodes.Ldarg_0 && codes[i + 4].opcode == OpCodes.Call && codes[i + 5].opcode == OpCodes.Call && codes[i + 6].opcode == OpCodes.Brtrue_S 
                        && (MethodInfo)codes[i+2].operand == AccessTools.Method(typeof(Farmer), "get_spouse") && (MethodInfo)codes[i + 4].operand == AccessTools.Method(typeof(Character), "get_Name"))
                    {
                        SMonitor.Log("Removing spouse infinite gifts per week");
                        for(int j = 0; j < 7; j++)
                        {
                            codes[i + j].opcode = OpCodes.Nop;
                            codes[i + j].operand = null;
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
                /*
                 * Before patch:
                 * 
                 * // if (!flag2 && !socialEntry.IsChild) <- the C# code we are replacing
	             *  IL_039d: ldloc.s 5
	             *  IL_039f: brtrue IL_063d
                 *
	             *  IL_03a4: ldloc.0
	             *  IL_03a5: ldfld bool StardewValley.Menus.SocialPage/SocialEntry::IsChild
	             *  IL_03aa: brtrue IL_063d
                 * 
                 * I want to replace the operations before the second brtrue with setting up and calling drawGiftAmounts (will take 4 operations)
                 * drawGiftAmounts will always return true, so the brtrue will be hit
                 * That if statement leads to the default game code for drawing the gifts, so if we hit the brtrue, it won't be run (good)
                 * 
                 * It should look as follows:
                 * Load SocialPage
                 * Load SpriteBatch
                 * Load int
                 * Call drawGiftAmounts
                 * brtrue
                 * 
                 * After patch:
                 * 
                 * Load arg 0 - Load the SocialPage onto the stack
                 * Load arg 1 - Load the SpriteBatch onto the stack
                 * Load arg 2 - Load the int onto the stack
                 * Call ModEntry.DrawGiftAmounts  (will consume the three things we just loaded)
                 * IL_039f: brtrue IL_063d
                 */
                SMonitor.Log($"Transpiling SocialPage.drawNPCSlot");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    // We want to make sure codes[i] is that ldloc.s
                    if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Brtrue && codes[i + 2].opcode == OpCodes.Ldloc_0  && codes[i + 3].opcode == OpCodes.Ldfld && codes[i + 4].opcode == OpCodes.Brtrue && 
                        ((FieldInfo)codes[i + 3].operand) == AccessTools.Field(typeof(SocialPage.SocialEntry), nameof(SocialPage.SocialEntry.IsChild))) 
                    {

                        SMonitor.Log("replacing gift boxes");
                        codes[i].opcode = OpCodes.Ldarg_0; // Replaces the ldloc.s
                        codes[i].operand = null;

                        codes[i+1].opcode = OpCodes.Ldarg_1; // Replaces the first brtrue
                        codes[i+1].operand = null;

                        codes[i+2].opcode = OpCodes.Ldarg_2; // Replaces the ldloc.0
                        codes[i+2].operand = null;

                        codes[i+3].opcode = OpCodes.Call; // Replaces the ldfld
                        codes[i+3].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawGiftAmounts));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static bool DrawGiftAmounts(SocialPage page, SpriteBatch b, int i)
        {
            Friendship f = page.GetSocialEntry(i).Friendship;

            if (!Config.ModEnabled) // If the mod is disabled or f is null (the npc is unmet), just run the default code
                return page.GetSocialEntry(i).IsMarriedToCurrentPlayer() || page.GetSocialEntry(i).IsChild;

            if (Game1.player.getChildren().Contains(page.SocialEntries[i].Character))
                return true;

            int perDay;
            int perWeek;

            if (f == null) // If they haven't been met yet.
            {
                perDay = Config.OrdinaryGiftsPerDay;
                perWeek = Config.OrdinaryGiftsPerWeek;
            }
            else if (f.IsMarried() || f.IsRoommate())
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

            string day = f == null ? "0" : f.GiftsToday+"";
            string week = f == null ? "0" : f.GiftsThisWeek+"";
            string perDayString = perDay + "";
            string perWeekString = perWeek + "";

            ClickableTextureComponent sprite = AccessTools.FieldRefAccess<SocialPage, List<ClickableTextureComponent>>(page, "sprites")[i];

            Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(page.xPositionOnScreen + 384 + 424, sprite.bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, 0, -1, 0.2f);
            b.Draw(Game1.mouseCursors, new Vector2(page.xPositionOnScreen + 384 + 432, sprite.bounds.Y + 32 + 20), new Rectangle?(new Rectangle(227 + (f !=null && f.TalkedToToday ? 9 : 0), 425, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

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