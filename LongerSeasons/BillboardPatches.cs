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

        private static void Billboard_Postfix(Billboard __instance, bool dailyQuest, Dictionary<ClickableTextureComponent, List<string>> ____upcomingWeddings)
        {
            if (dailyQuest || Game1.dayOfMonth < 29)
                return;
            __instance.calendarDays = new List<ClickableTextureComponent>();
            Dictionary<int, NPC> birthdays = new Dictionary<int, NPC>();
            foreach (NPC i in Utility.getAllCharacters())
            {
                if (i.isVillager() && i.Birthday_Season != null && i.Birthday_Season.Equals(Game1.currentSeason) && !birthdays.ContainsKey(i.Birthday_Day) && (Game1.player.friendshipData.ContainsKey(i.Name) || (!i.Name.Equals("Dwarf") && !i.Name.Equals("Sandy") && !i.Name.Equals("Krobus"))))
                {
                    birthdays.Add(i.Birthday_Day, i);
                }
            }
            int startDate = (Game1.dayOfMonth - 1) / 28 * 28 + 1;
            for (int j = startDate; j <= startDate + 27; j++)
            {
                int l = (j - 1) % 28 + 1;
                string festival = "";
                string birthday = "";
                NPC npc = birthdays.ContainsKey(j) ? birthdays[j] : null;
                if (Utility.isFestivalDay(j, Game1.currentSeason))
                {
                    festival = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + j.ToString())["name"];
                }
                else if (npc != null)
                {
                    if (npc.displayName.Last<char>() == 's' || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de && (npc.displayName.Last<char>() == 'x' || npc.displayName.Last<char>() == 'ß' || npc.displayName.Last<char>() == 'z')))
                    {
                        birthday = Game1.content.LoadString("Strings\\UI:Billboard_SBirthday", npc.displayName);
                    }
                    else
                    {
                        birthday = Game1.content.LoadString("Strings\\UI:Billboard_Birthday", npc.displayName);
                    }
                }
                Texture2D character_texture = null;
                if (npc != null)
                {
                    try
                    {
                        character_texture = Game1.content.Load<Texture2D>("Characters\\" + npc.getTextureName());
                    }
                    catch (Exception)
                    {
                        character_texture = npc.Sprite.Texture;
                    }
                }
                ClickableTextureComponent calendar_day = new ClickableTextureComponent(festival, new Rectangle(__instance.xPositionOnScreen + 152 + (l - 1) % 7 * 32 * 4, __instance.yPositionOnScreen + 200 + (l - 1) / 7 * 32 * 4, 124, 124), festival, birthday, character_texture, (npc != null) ? new Rectangle(0, 0, 16, 24) : Rectangle.Empty, 1f, false)
                {
                    myID = l,
                    rightNeighborID = ((l % 7 != 0) ? (l + 1) : -1),
                    leftNeighborID = ((l % 7 != 1) ? (l - 1) : -1),
                    downNeighborID = l + 7,
                    upNeighborID = ((l > 7) ? (l - 7) : -1)
                };
                HashSet<Farmer> traversed_farmers = new HashSet<Farmer>();
                foreach (Farmer farmer in Game1.getOnlineFarmers())
                {
                    if (!traversed_farmers.Contains(farmer) && farmer.isEngaged() && !farmer.hasCurrentOrPendingRoommate())
                    {
                        string spouse_name = null;
                        WorldDate wedding_date = null;
                        if (Game1.getCharacterFromName(farmer.spouse, true, false) != null)
                        {
                            wedding_date = farmer.friendshipData[farmer.spouse].WeddingDate;
                            spouse_name = Game1.getCharacterFromName(farmer.spouse, true, false).displayName;
                        }
                        else
                        {
                            long? spouse = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
                            if (spouse != null)
                            {
                                Farmer spouse_farmer = Game1.getFarmerMaybeOffline(spouse.Value);
                                if (spouse_farmer != null && Game1.getOnlineFarmers().Contains(spouse_farmer))
                                {
                                    wedding_date = farmer.team.GetFriendship(farmer.UniqueMultiplayerID, spouse.Value).WeddingDate;
                                    traversed_farmers.Add(spouse_farmer);
                                    spouse_name = spouse_farmer.Name;
                                }
                            }
                        }
                        if (!(wedding_date == null))
                        {
                            if (wedding_date.TotalDays < Game1.Date.TotalDays)
                            {
                                wedding_date = new WorldDate(Game1.Date);
                                wedding_date.TotalDays++;
                            }
                            if (wedding_date != null && wedding_date.TotalDays >= Game1.Date.TotalDays && Utility.getSeasonNumber(Game1.currentSeason) == wedding_date.SeasonIndex && j == wedding_date.DayOfMonth)
                            {
                                if (!____upcomingWeddings.ContainsKey(calendar_day))
                                {
                                    ____upcomingWeddings[calendar_day] = new List<string>();
                                }
                                traversed_farmers.Add(farmer);
                                ____upcomingWeddings[calendar_day].Add(farmer.Name);
                                ____upcomingWeddings[calendar_day].Add(spouse_name);
                            }
                        }
                    }
                }
                __instance.calendarDays.Add(calendar_day);
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