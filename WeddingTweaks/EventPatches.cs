using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WeddingTweaks
{
    public static class EventPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        private static List<int[]> weddingPositions = new List<int[]>
        {
            new int[]{26,63,1},
            new int[]{29,63,3},
            new int[]{25,63,1},
            new int[]{30,63,3}
        };
        public static bool startingLoadActors = false;


        public static void Event_setUpCharacters_Postfix(Event __instance, GameLocation location)
        {
            try
            {
                if (!__instance.isWedding || !Config.AllSpousesJoinWeddings || Misc.GetSpouses(Game1.player, 0).Count == 0 || ModEntry.freeLoveAPI == null)
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
                    }
                }
            }

            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_setUpCharacters_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Event_endBehaviors_Postfix(string[] split)
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