using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace FarmCaveFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static List<string> answerIds = new List<string>();
        private static bool Event_answerDialogue_Prefix(string questionKey, int answerChoice)
        {

            if (!Config.EnableMod || !Game1.IsMasterGame || questionKey != "cave")
                return true;
            caveChoice = SHelper.GameContent.Load<Dictionary<string,CaveChoice>>(frameworkPath)[answerIds[answerChoice]];
            SMonitor.Log($"Chose {caveChoice.choice}, objects {caveChoice.objects.Count}");
            SHelper.Data.WriteSaveData("farm-cave-framework-choice", caveChoice.id);
            if (caveChoice.objects.Count > 0)
            {
                FarmCave cave = Game1.getLocationFromName("FarmCave") as FarmCave;
                foreach (var o in caveChoice.objects)
                {
                    Object obj = GetObjectFromID(o.id, new Vector2(o.X, o.Y));
                    if(obj != null)
                    {
                        cave.setObject(new Vector2(o.X, o.Y), obj);
                    }
                }
            }
            return false;
        }

        private static Object GetObjectFromID(string id, Vector2 tile, int amount = 1, bool spawned = false)
        {
            SMonitor.Log($"Trying to get object {id}, DGA {apiDGA != null}, JA {apiJA != null}");

            Object obj = null;
            try
            {

                if (int.TryParse(id, out int index))
                {
                    SMonitor.Log($"Spawning object with index {id}");
                    return spawned ? new Object(index, amount, false, -1, 0)
                    {
                        IsSpawnedObject = true
                    } : new Object(tile, index, false);
                }
                if (apiDGA != null && id.Contains("/"))
                {
                    object o = apiDGA.SpawnDGAItem(id);
                    if (o is Object)
                    {
                        SMonitor.Log($"Spawning DGA object {id}");
                        obj = (Object)o;
                        if (spawned)
                        {
                            obj.IsSpawnedObject = true;
                            obj.Stack = amount;
                        }
                        else
                        {
                            obj.TileLocation = tile;
                        }
                        return obj;
                    }
                }
                if (apiJA != null)
                {
                    int idx = apiJA.GetObjectId(id);
                    if (idx != -1)
                    {
                        SMonitor.Log($"Spawning JA object {id}");
                        obj = spawned ? new Object(index, amount, false, -1, 0)
                        {
                            IsSpawnedObject = true
                        } : new Object(tile, idx, false);
                        return obj;
                    }
                }
            }
            catch
            {
            }
            SMonitor.Log($"Couldn't find item with id {id}");
            return obj;
        }

        private static bool Event_command_cave_Prefix()
        {
            if (!Config.EnableMod || !Game1.IsMasterGame || Game1.activeClickableMenu != null)
                return true;
            Dictionary<string, CaveChoice> choices = SHelper.GameContent.Load<Dictionary<string, CaveChoice>>(frameworkPath);
            SMonitor.Log($"showing {choices.Count} cave choices");
            List<Response> responses = new List<Response>();
            answerIds.Clear();
            foreach (var c in choices)
            {
                answerIds.Add(c.Key);
                responses.Add(new Response(c.Key, c.Value.choice));
            }
            Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1223"), responses.ToArray(), "cave");
            Game1.dialogueTyping = false;
            return false;
        }
        private static bool FarmCave_UpdateWhenCurrentLocation_Prefix(FarmCave __instance, GameTime time)
        {
            if (!Config.EnableMod || !Game1.IsMasterGame || caveChoice == null)
                return true;
            var ptr = typeof(GameLocation).GetMethod("UpdateWhenCurrentLocation", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            var baseMethod = (Func<GameTime, GameLocation>)Activator.CreateInstance(typeof(Func<GameTime, GameLocation>), __instance, ptr);
            baseMethod(time);

            if (caveChoice.periodics.Count > 0)
            {
                foreach(var p in caveChoice.periodics)
                {
                    if (Game1.random.NextDouble() < p.chance / 100f)
                    {
                        if (p.animations.Count > 0)
                        {
                            var a = p.animations[Game1.random.Next(p.animations.Count)];
                            __instance.TemporarySprites.Add(new TemporaryAnimatedSprite(a.sourceFile, new Rectangle(a.sourceX, a.sourceY, a.width, a.height), a.interval, a.length, a.loops, new Vector2(!a.randomX ? (a.right ? __instance.map.Layers[0].LayerWidth * 64 + a.X : a.X) : Game1.random.Next(__instance.map.Layers[0].LayerWidth) * 64f, !a.randomY ? (a.bottom ? __instance.map.Layers[0].LayerHeight * 64 + a.Y : a.Y) : Game1.random.Next(__instance.map.Layers[0].LayerHeight) * 64f), a.flicker, a.flipped, 1f, a.alphaFade, a.color, a.scale, a.scaleChange, a.rotation, a.rotationChange, false)
                            {
                                xPeriodic = true,
                                xPeriodicLoopTime = a.loopTIme,
                                xPeriodicRange = a.range,
                                motion = new Vector2(a.motionX, a.motionY)
                            });
                        }
                        if (p.randomSounds.Count > 0)
                        {
                            var s = p.randomSounds[Game1.random.Next(p.randomSounds.Count)];
                            if (Game1.random.NextDouble() < s.chance / 100f)
                            {
                                __instance.localSound(s.id);
                            }
                        }
                        if (p.repeatedSounds.Count > 0)
                        {
                            var s = p.repeatedSounds[Game1.random.Next(p.repeatedSounds.Count)];
                            if (Game1.random.NextDouble() < s.chance / 100f)
                            {
                                for (int i = 1; i < s.count; i++)
                                {
                                    DelayedAction.playSoundAfterDelay(s.id, s.delayMult * i + s.delayAdd, null, s.pitch);
                                }
                            }
                        }
                        if (p.specials.Count > 0)
                        {
                            var s = p.specials[Game1.random.Next(p.specials.Count)];
                            if (s == "BatTemporarySprite")
                            {
                                __instance.temporarySprites.Add(new BatTemporarySprite(new Vector2((float)((Game1.random.NextDouble() < 0.5) ? 0 : (__instance.map.DisplayWidth - 64)), (float)(__instance.map.DisplayHeight - 64))));
                            }
                        }
                    }
                }
            }

            return false;
        }
        private static bool FarmCave_resetLocalState_Prefix(FarmCave __instance)
        {
            if (!Config.EnableMod || !Game1.IsMasterGame)
                return true;
            LoadCaveChoice();
            if (caveChoice == null)
                return true;

            var ptr = typeof(GameLocation).GetMethod("resetLocalState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            var baseMethod = (Func<GameLocation>)Activator.CreateInstance(typeof(Func<GameLocation>), __instance, ptr);
            baseMethod();


            if (caveChoice.animations.Count > 0)
            {
                foreach (var a in caveChoice.animations)
                {
                    __instance.temporarySprites.Add(new TemporaryAnimatedSprite(a.sourceFile, new Rectangle(a.sourceX, a.sourceY, a.width, a.height), new Vector2(a.right ? __instance.map.Layers[0].LayerWidth * 64 + a.X : a.X, a.bottom ? __instance.map.Layers[0].LayerHeight * 64 + a.Y : a.Y), false, 0f, Color.White)
                    {
                        interval = a.interval,
                        animationLength = a.length,
                        totalNumberOfLoops = a.loops,
                        scale = a.scale,
                        delayBeforeAnimationStart = a.delay,
                        layerDepth = 1f,
                        light = a.light,
                        lightRadius = a.lightRadius
                    });
                }
            }
            Game1.ambientLight = caveChoice.ambientLight;
            return false;
        }
        private static bool FarmCave_DayUpdate_Prefix(FarmCave __instance, int dayOfMonth)
        {
            if (!Config.EnableMod || !Game1.IsMasterGame)
                return true;
            LoadCaveChoice();
            if (caveChoice == null)
                return true;

            var ptr = typeof(GameLocation).GetMethod("DayUpdate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            var baseMethod = (Func<GameLocation>)Activator.CreateInstance(typeof(Func<GameLocation>), __instance, ptr);
            baseMethod();
            if (caveChoice.resources.Count > 0)
            {
                SMonitor.Log($"Spawning resources");
                float totalWeight = 0;
                foreach (var r in caveChoice.resources)
                {
                    totalWeight += r.weight;
                }
                int spawned = 0;
                while (Game1.random.NextDouble() < Math.Min(0.99f, caveChoice.resourceChance / 100f))
                {
                    float currentWeight = 0;
                    double chance = Game1.random.NextDouble();
                    foreach (var r in caveChoice.resources)
                    {
                        currentWeight += r.weight;
                        if(chance < currentWeight / totalWeight)
                        {
                            Vector2 v = new Vector2(Game1.random.Next(1, __instance.map.Layers[0].LayerWidth - 1), Game1.random.Next(1, __instance.map.Layers[0].LayerHeight - 4));
                            if (__instance.isTileLocationTotallyClearAndPlaceable(v))
                            {
                                spawned++;
                                int amount = Game1.random.Next(r.min, r.max + 1);
                                Object obj = GetObjectFromID(r.id, Vector2.Zero, amount, true);
                                __instance.setObject(v, obj);
                            }
                            break;
                        }
                    }
                }
                SMonitor.Log($"Spawned {spawned} resources, total weight {totalWeight}");
            }
            __instance.UpdateReadyFlag();
            return false;
        }
        private static bool FarmCave_UpdateReadyFlag_Prefix(FarmCave __instance)
        {
            if (!Config.EnableMod || !Game1.IsMasterGame)
                return true;
            /*
            string choiceId = SHelper.Data.ReadSaveData<string>("farm-cave-framework-choice");
            if (choiceId == null)
                return true;
            Dictionary<string, CaveChoice> choices = SHelper.Content.Load<Dictionary<string, CaveChoice>>(frameworkPath, ContentSource.GameContent);
            if (!choices.ContainsKey(choiceId))
            {
                SMonitor.Log($"Choice key {choiceId} not found");
                return true;
            }
            CaveChoice choice = choices[choiceId];
            */
            bool flag_value = false;
            foreach (Object o in __instance.objects.Values)
            {
                if (o.IsSpawnedObject)
                {
                    flag_value = true;
                    break;
                }
                if (o.bigCraftable.Value && o.heldObject.Value != null && o.MinutesUntilReady <= 0)
                {
                    flag_value = true;
                    break;
                }
            }
            Game1.getFarm().farmCaveReady.Value = flag_value;
            return false;
        }
    }
}