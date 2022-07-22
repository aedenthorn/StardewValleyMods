using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WeddingTweaks
{
    public partial class ModEntry
    {

        private static List<int[]> weddingPositions = new List<int[]>
        {
            new int[]{26,63,1},
            new int[]{29,63,3},
            new int[]{25,63,1},
            new int[]{30,63,3}
        };
        public static bool startingLoadActors = false;

        [HarmonyPatch(typeof(Event), "setUpCharacters")]
        public static class Event_setUpCharacters_Patch
        {
            public static void Postfix(Event __instance, GameLocation location)
            {
                try
                {
                    if (!__instance.isWedding || !Config.AllSpousesJoinWeddings || Misc.GetSpouses(Game1.player, 0).Count == 0 || ModEntry.freeLoveAPI == null)
                        return;
                    string witness = null;
                    string npcWitness = null;
                    if (Config.AllowWitnesses)
                    {
                        if (Game1.player.modData.TryGetValue(witnessKey, out witness))
                        {
                            SMonitor.Log($"Player has {witness} as witness");
                            Game1.player.modData.Remove(witnessKey);
                        }
                        string spouse = Game1.player.getSpouse().Name;
                        if (!npcWitnessDict.TryGetValue(spouse, out npcWitness))
                        {
                            Dictionary<string, string> dispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
                            if(dispositions.TryGetValue(spouse, out string dis))
                            {
                                string[] relations = dis.Split('/')[9].Split(' ');
                                if (relations.Length > 0)
                                {
                                    List<string> family = new List<string>();
                                    for (int i = 0; i < relations.Length; i += 2)
                                    {
                                        try
                                        {
                                            family.Add(relations[i]);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    if(family.Count > 0)
                                    {
                                        npcWitness = family[Game1.random.Next(family.Count)];
                                    }
                                }
                                if(npcWitness != null)
                                {
                                    SMonitor.Log($"{spouse} chose {npcWitness} as witness");
                                }
                            }
                        }
                    }

                    List<string> spouses = Misc.GetSpouses(Game1.player, 0).Keys.ToList();
                    Misc.ShuffleList(ref spouses);
                    foreach (NPC actor in __instance.actors)
                    {
                        if (witness is not null && actor.Name == witness)
                        {
                            actor.position.Value = new Vector2(26 * Game1.tileSize, 64 * Game1.tileSize);

                            if (ModEntry.Config.AllSpousesWearMarriageClothesAtWeddings)
                            {
                                int frame = 37;
                                if (actor.Gender == 0)
                                {
                                    frame += 12;
                                }
                                actor.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new FarmerSprite.AnimationFrame(frame, 0, false, false, null, true)
                                });
                            }
                            else
                                Utility.facePlayerEndBehavior(actor, location);
                            continue;
                        }
                        if ((npcWitness is not null && actor.Name == npcWitness) || (Config.AllowWitnesses && Config.AllowRandomNPCWitnesses))
                        {
                            if(npcWitness is null && Game1.player.friendshipData.Keys. Count() > 0)
                            {
                                npcWitness = Game1.player.friendshipData.Keys.ToArray()[Game1.random.Next(Game1.player.friendshipData.Keys.Count())];
                            }
                            if(npcWitness is not null)
                            {
                                actor.position.Value = new Vector2(29 * Game1.tileSize, 64 * Game1.tileSize);

                                if (Config.AllSpousesWearMarriageClothesAtWeddings)
                                {
                                    int frame = 37;
                                    if (actor.Gender == 0)
                                    {
                                        frame += 12;
                                    }
                                    actor.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new FarmerSprite.AnimationFrame(frame, 0, false, true, null, true)
                                });
                                }
                                else
                                    Utility.facePlayerEndBehavior(actor, location);
                                continue;
                            }
                        }
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
                            if (ModEntry.Config.AllSpousesWearMarriageClothesAtWeddings)
                            {
                                bool flipped = false;
                                int frame = 37;
                                if (pos.Y < 62 * Game1.tileSize)
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
                                else if (pos.X < 28 * Game1.tileSize)
                                {
                                    flipped = true;
                                }
                                if (actor.Gender == 0)
                                {
                                    frame += 12;
                                }
                                actor.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                            {
                                new FarmerSprite.AnimationFrame(frame, 0, false, flipped, null, true)
                            });
                            }
                            else
                                Utility.facePlayerEndBehavior(actor, location);
                            continue;
                        }
                    }
                }

                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in {nameof(Event_setUpCharacters_Patch)}:\n{ex}", LogLevel.Error);
                }
            }
        }

        [HarmonyPatch(typeof(Event), "endBehaviors")]
        public static class Event_endBehaviors_Patch
        {
            public static void Postfix(string[] split)
            {
                try
                {
                    if (!Config.AllSpousesJoinWeddings || ModEntry.freeLoveAPI == null)
                        return;
                    if (split != null && split.Length > 1)
                    {
                        string text = split[1];
                        if (text == "wedding")
                        {
                            ModEntry.freeLoveAPI.PlaceSpousesInFarmhouse(Utility.getHomeOfFarmer(Game1.player));
                        }
                    }
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in {nameof(Event_endBehaviors_Patch)}:\n{ex}", LogLevel.Error);
                }
            }
        }

        [HarmonyPatch(typeof(Event), "command_loadActors")]
        public static class Event_command_loadActors_Patch
        {
            public static void Prefix()
            {
                try
                {
                    startingLoadActors = true;
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in {nameof(Event_command_loadActors_Patch)}:\n{ex}", LogLevel.Error);
                }
            }

            public static void Postfix()
            {
                try
                {
                    startingLoadActors = false;
                    Game1Patches.lastGotCharacter = null;

                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in {nameof(Event_command_loadActors_Patch)}:\n{ex}", LogLevel.Error);
                }
            }
        }
        

    }
}