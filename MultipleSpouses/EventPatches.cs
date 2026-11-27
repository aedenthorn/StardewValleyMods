using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleSpouses
{
    public static class EventPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static List<int[]> weddingPositions = new List<int[]>
        {
            new int[]{26,63,1},
            new int[]{29,63,3},
            new int[]{25,63,1},
            new int[]{30,63,3}
        };
        public static bool startingLoadActors = false;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static bool Event_answerDialogueQuestion_Prefix(Event __instance, NPC who, string answerKey)
        {
            try
            {

                if (answerKey == "danceAsk" && !who.HasPartnerForDance && Game1.player.friendshipData[who.Name].IsMarried())
                {
                    string accept = "";
                    int gender = who.Gender;
                    if (gender != 0)
                    {
                        if (gender == 1)
                        {
                            accept = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1634");
                        }
                    }
                    else
                    {
                        accept = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1633");
                    }
                    try
                    {
                        Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.Name, true));
                    }
                    catch
                    {
                    }
                    Game1.player.dancePartner.Value = who;
                    who.setNewDialogue(accept, false, false);
                    using (List<NPC>.Enumerator enumerator = __instance.actors.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            NPC j = enumerator.Current;
                            if (j.CurrentDialogue != null && j.CurrentDialogue.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
                            {
                                j.CurrentDialogue.Clear();
                            }
                        }
                    }
                    Game1.drawDialogue(who);
                    who.immediateSpeak = true;
                    who.facePlayer(Game1.player);
                    who.Halt();
                    return false;
                }
            }

            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_answerDialogueQuestion_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static void Event_setUpCharacters_Postfix(Event __instance, GameLocation location)
        {
            try
            {
                if (!__instance.isWedding || !ModEntry.config.AllSpousesJoinWeddings)
                    return;

                List<string> spouses = Misc.GetSpouses(Game1.player, 0).Keys.ToList();
                Misc.ShuffleList(ref spouses);
                foreach (NPC actor in __instance.actors)
                {
                    if (spouses.Contains(actor.Name))
                    {
                        int idx = spouses.IndexOf(actor.Name);
                        Vector2 pos;
                        if (idx < weddingPositions.Count)
                        {
                            pos = new Vector2(weddingPositions[idx][0] * Game1.tileSize, weddingPositions[idx][1] * Game1.tileSize);
                        }
                        else
                        {
                            int x = 25 + ((idx - 4) % 6);
                            int y = 62 - ((idx - 4) / 6);
                            pos = new Vector2(x * Game1.tileSize, y * Game1.tileSize);
                        }
                        actor.position.Value = pos;
                        if (ModEntry.config.AllSpousesWearMarriageClothesAtWeddings)
                        {
                            bool flipped = false;
                            int frame = 37;
                            if(pos.Y < 62 * Game1.tileSize)
                            {
                                if (pos.X == 25 * Game1.tileSize)
                                {
                                    flipped = true;
                                }
                                else if (pos.X < 30 * Game1.tileSize)
                                {
                                    frame = 36;
                                }

                            }
                            else if(pos.X < 28 * Game1.tileSize)
                            {
                                flipped = true;
                            }

                            actor.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                            {
                                new FarmerSprite.AnimationFrame(frame, 0, false, flipped, null, true)
                            });
                        }
                        else 
                            Utility.facePlayerEndBehavior(actor, location);
                    }
                }
            }

            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_answerDialogueQuestion_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool Event_command_playSound_Prefix(Event __instance, string[] split)
        {
            try
            {
                if (split[1] == "dwop" && __instance.isWedding && ModEntry.config.RealKissSound && Kissing.kissEffect != null)
                {
                    Kissing.kissEffect.Play();
                    int num = __instance.CurrentCommand;
                    __instance.CurrentCommand = num + 1;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_command_playSound_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static void Event_endBehaviors_Postfix(string[] split)
        {
            try
            {
                if (split != null && split.Length > 1)
                {
                    string text = split[1];
                    if (text == "wedding")
                    {
                        Misc.PlaceSpousesInFarmhouse(Utility.getHomeOfFarmer(Game1.player));
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_endBehaviors_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Event_command_loadActors_Prefix()
        {
            try
            {
                startingLoadActors = true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_command_loadActors_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Event_command_loadActors_Postfix()
        {
            try
            {
                startingLoadActors = false;
                Game1Patches.lastGotCharacter = null;

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_command_loadActors_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}