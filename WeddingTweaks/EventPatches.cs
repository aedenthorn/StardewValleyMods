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

        private static List<int[]> allWeddingPositions = new List<int[]>
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
                    if (!__instance.isWedding)
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
                        if(npcWeddingDict.TryGetValue(spouse, out WeddingData data) && data.witnesses.Count > 0)
                        {
                            List<string> list = new List<string>();
                            foreach (var name in data.witnesses)
                            {
                                if (name != spouse && name != witness)
                                    list.Add(name);
                            }
                            if (list.Count > 0)
                            {
                                npcWitness = list[Game1.random.Next(list.Count)];
                            }
                        }
                        if (npcWitness is null)
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
                                            if(relations[i] != spouse && relations[i] != witness)
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
                                if(npcWitness is null && Config.AllowRandomNPCWitnesses && Game1.player.friendshipData.Keys.Count() > 1)
                                {
                                    List<string> friends = new List<string>();
                                    foreach (var key in Game1.player.friendshipData.Keys)
                                    {
                                        try
                                        {
                                            if (key != spouse && key != witness)
                                                friends.Add(key);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    npcWitness = friends[Game1.random.Next(friends.Count)];
                                }
                                if(npcWitness != null)
                                {
                                    SMonitor.Log($"{spouse} chose {npcWitness} as witness");
                                }
                            }
                        }
                    }
                    else
                    {
                        Game1.player.modData.Remove(witnessKey);
                    }

                    List<string> spouses = Misc.GetSpouses(Game1.player, 0).Keys.ToList();
                    Misc.ShuffleList(ref spouses);
                    bool addSpouses = Config.AllSpousesJoinWeddings && spouses.Count > 0 && freeLoveAPI is not null;

                    if(witness is not null && !__instance.actors.Exists(n => n.Name == witness))
                    {
                        try
                        {
                            AccessTools.Method(typeof(Event), "addActor").Invoke(__instance, new object[] { witness, 26, 63, 1, location });
                        }
                        catch (Exception ex)
                        {
                            SMonitor.Log($"Error adding {witness} to wedding: \n\n{ex}");
                        }
                    }
                    if(npcWitness is not null && !__instance.actors.Exists(n => n.Name == npcWitness))
                    {
                        try
                        {
                            AccessTools.Method(typeof(Event), "addActor").Invoke(__instance, new object[] { npcWitness, 29, 63, 3, location });
                        }
                        catch (Exception ex)
                        {
                            SMonitor.Log($"Error adding {npcWitness} to wedding: \n\n{ex}");
                        }
                    }

                    var weddingPositions = new List<int[]>();
                    for(int i = 0; i < allWeddingPositions.Count; i++)
                    {
                        if (i == 0 && witness is not null)
                            continue;
                        if (i == 1 && npcWitness is not null)
                            continue;
                        weddingPositions.Add(allWeddingPositions[i]);
                    }

                    foreach (NPC actor in __instance.actors)
                    {
                        if (witness is not null && actor.Name == witness)
                        {
                            actor.position.Value = new Vector2(26 * Game1.tileSize, 63 * Game1.tileSize);

                            if (Config.AllSpousesWearMarriageClothesAtWeddings)
                            {
                                int frame = 37;
                                if (actor.Gender == 0)
                                {
                                    frame += 12;
                                }
                                if (npcWeddingDict.TryGetValue(actor.Name, out WeddingData data) && data.witnessFrame >= 0)
                                {
                                    frame = data.witnessFrame + 1;
                                }
                                else if (!actor.datable.Value)
                                {
                                    frame = 12;
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
                        if ((npcWitness is not null && actor.Name == npcWitness))
                        {
                            actor.position.Value = new Vector2(29 * Game1.tileSize, 63 * Game1.tileSize);

                            if (Config.AllSpousesWearMarriageClothesAtWeddings)
                            {
                                int frame = 37;
                                if (actor.Gender == 0)
                                {
                                    frame += 12;
                                }
                                if (npcWeddingDict.TryGetValue(actor.Name, out WeddingData data) && data.witnessFrame >= 0)
                                {
                                    frame = data.witnessFrame + 1;
                                }
                                else if (!actor.datable.Value)
                                {
                                    frame = 12;
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
                        if (addSpouses && spouses.Contains(actor.Name))
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
                            if (Config.AllSpousesWearMarriageClothesAtWeddings)
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
                                if (npcWeddingDict.TryGetValue(actor.Name, out WeddingData data) && data.witnessFrame >= 0)
                                {
                                    frame = data.witnessFrame + 1;
                                    if (pos.X < 30 * Game1.tileSize && pos.X > 25 * Game1.tileSize)
                                    {
                                        frame--;
                                    }
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
                    if (!Config.AllSpousesJoinWeddings || freeLoveAPI == null)
                        return;
                    if (split != null && split.Length > 1)
                    {
                        string text = split[1];
                        if (text == "wedding")
                        {
                            freeLoveAPI.PlaceSpousesInFarmhouse(Utility.getHomeOfFarmer(Game1.player));
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