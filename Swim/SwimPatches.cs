using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Tiles;

namespace Swim
{
    internal class SwimPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }
        public static void FarmerRenderer_draw_Prefix(Farmer who, ref bool __state)
        {
            try
            {
                if(who.swimming.Value && Game1.player.currentLocation.Name.StartsWith("Custom_Underwater"))
                {
                    who.swimming.Value = false;
                    __state = true;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmerRenderer_draw_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static bool FarmerSprite_checkForFootstep_Prefix()
        {
            try
            {
                if(Game1.player.swimming.Value)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmerSprite_checkForFootstep_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        internal static void FarmerRenderer_draw_Postfix(Farmer who, bool __state)
        {
            try
            {
                if (__state)
                {
                    who.swimming.Value = true;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmerRenderer_draw_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        internal static void GameLocation_StartEvent_Postfix()
        {
            try
            {
                if (Game1.player.swimming.Value)
                {
                    Game1.player.swimming.Value = false;
                    if (!Config.SwimSuitAlways)
                        Game1.player.changeOutOfSwimSuit();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_StartEvent_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        internal static void Event_exitEvent_Postfix(Event __instance)
        {
            try
            {
                Monitor.Log($"exiting event");
                if (__instance.exitLocation != null && __instance.exitLocation != null && __instance.exitLocation.Location.waterTiles != null && __instance.exitLocation.Location.waterTiles[(int)(Game1.player.positionBeforeEvent.X),(int)(Game1.player.positionBeforeEvent.Y)])
                {
                    Monitor.Log($"swimming again");
                    ChangeAfterEvent();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_exitEvent_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static async Task ChangeAfterEvent()
        {
            await Task.Delay(1500);
            Game1.player.changeIntoSwimsuit();
            Game1.player.swimming.Value = true;
        }


        public static void Farmer_updateCommon_Prefix(ref Farmer __instance)
        {
            try
            {
                if (__instance.swimming.Value && (!Config.ReadyToSwim || Config.SwimRestoresVitals) && __instance.timerSinceLastMovement > 0 && !Game1.eventUp && (Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
                {
                    if (__instance.timerSinceLastMovement > 800)
                    {
                        __instance.currentEyes = 1;
                    }
                    else if (__instance.timerSinceLastMovement > 700)
                    {
                        __instance.currentEyes = 4;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_updateCommon_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void Farmer_updateCommon_Postfix(ref Farmer __instance)
        {
            try
            {
                if (__instance.swimming.Value && (!Config.ReadyToSwim || Config.SwimRestoresVitals) && __instance.timerSinceLastMovement > 0 && !Game1.eventUp && (Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
                {
                    if (__instance.swimTimer < 0)
                    {
                        __instance.swimTimer = 100;
                        if (__instance.stamina < (float)__instance.maxStamina.Value)
                        {
                            float stamina = __instance.stamina;
                            __instance.stamina = stamina + 1f;
                        }
                        if (__instance.health < __instance.maxHealth)
                        {
                            __instance.health++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_updateCommon_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static IEnumerable<CodeInstruction> Farmer_updateCommon_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            try
            {
                bool startLooking = false;
                int start = -1;
                int end = -1;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (startLooking)
                    {
                        if(start == -1 && codes[i].opcode == OpCodes.Ldfld && codes[i].operand as FieldInfo == typeof(Farmer).GetField("timerSinceLastMovement"))
                        {
                            start = i - 1;
                            Monitor.Log($"start at {start}");
                        }
                        if (codes[i].opcode == OpCodes.Stfld && codes[i].operand as FieldInfo == typeof(Farmer).GetField("health"))
                        {
                            end = i + 1;
                            Monitor.Log($"end at {end}");
                        }
                    }
                    else if (codes[i].operand as string == "slosh")
                    {
                        startLooking = true;
                    }
                }
                if(start > -1 && end > start)
                {
                    codes.RemoveRange(start, end - start);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_updateCommon_Transpiler)}:\n{ex}", LogLevel.Error);
            }

            return codes.AsEnumerable();
        }

        public static void Farmer_changeIntoSwimsuit_Postfix(Farmer __instance)
        {
            try
            {
                if(Config.AllowActionsWhileInSwimsuit)
                    __instance.canOnlyWalk = false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_changeIntoSwimsuit_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        public static void Farmer_setRunning_Prefix(Farmer __instance, ref bool __state)
        {
            try
            {
                if (__instance.bathingClothes.Value && Config.AllowRunningWhileInSwimsuit)
                {
                    __instance.bathingClothes.Value = false;
                    __state = true;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_changeIntoSwimsuit_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void Farmer_setRunning_Postfix(Farmer __instance, bool __state)
        {
            try
            {
                if(!__instance.bathingClothes.Value && Config.AllowRunningWhileInSwimsuit && __state == true)
                    __instance.bathingClothes.Value = true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_setRunning_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool Toolbar_draw_Prefix()
        {
            try
            {
                if (Game1.player.currentLocation != null && Game1.player.currentLocation.Name == "AbigailCave")
                    return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Toolbar_draw_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }


        public static void Wand_DoFunction_Prefix(ref Farmer who, ref bool __state)
        {
            if (who.bathingClothes.Value)
            {
                who.bathingClothes.Value = false;
                __state = true;
            }
        }
        public static void Wand_DoFunction_Postfix(ref Farmer who, bool __state)
        {
            if(__state)
            {
                who.bathingClothes.Value = true;
            }
        }
        public static IEnumerable<CodeInstruction> Wand_DoFunction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            try
            {
                int start = 0;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ret)
                    {
                        start = i + 1;
                        return codes.Skip(start).AsEnumerable();
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Wand_DoFunction_Transpiler)}:\n{ex}", LogLevel.Error);
            }

            return codes.AsEnumerable();
        }
        public static void GameLocation_resetForPlayerEntry_Prefix(GameLocation __instance)
        {
            try
            {
                if(__instance.Name == "Custom_ScubaCrystalCave")
                {
                    if (Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        __instance.mapPath.Value = "Maps\\CrystalCaveDark";
                    }
                    else
                    {
                        __instance.mapPath.Value = "Maps\\CrystalCave";
                        ModEntry.oldMariner.Value = new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(10f, 7f) * 64f, 2, "Old Mariner", null);

                    }
                    //__instance.updateMap();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_resetForPlayerEntry_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void GameLocation_draw_Prefix(GameLocation __instance, SpriteBatch b)
        {
            try
            {
                if(__instance.Name == "Custom_ScubaCrystalCave")
                {
                    if (!Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        ModEntry.oldMariner.Value.draw(b);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_draw_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static bool GameLocation_isCollidingPosition_Prefix(GameLocation __instance, Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, ref bool __result)
        {
            try
            {
                if(__instance.Name == "Custom_ScubaCrystalCave")
                {
                    if (!Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed") && ModEntry.oldMariner != null && position.Intersects(ModEntry.oldMariner.Value.GetBoundingBox()))
                    {
                        __result = true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_isCollidingPosition_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        public static void GameLocation_UpdateWhenCurrentLocation_Postfix(GameLocation __instance, GameTime time)
        {
            try
            {
                if (__instance.Name == "Custom_ScubaCrystalCave")
                {
                    if (!Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        if (ModEntry.oldMariner != null)
                        {
                            ModEntry.oldMariner.Value.update(time, __instance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_UpdateWhenCurrentLocation_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void GameLocation_performTouchAction_Prefix(string fullActionString)
        {
            try
            {
                string text = fullActionString.Split(new char[]
                {
                    ' '
                })[0];
                if (text == "PoolEntrance")
                {
                    if (!Game1.player.swimming.Value)
                    {
                        Config.ReadyToSwim = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_performTouchAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void GameLocation_performTouchAction_Postfix(string fullActionString)
        {
            try
            {
                string text = fullActionString.Split(new char[]
                {
                    ' '
                })[0];
                if (text == "PoolEntrance")
                {
                    if (Game1.player.swimming.Value)
                    {
                        Config = Helper.ReadConfig<ModConfig>();
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_performTouchAction_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void GameLocation_checkAction_Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            try
            {
                if (__instance.Name == "Custom_ScubaCrystalCave")
                {
                    if (!who.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        if (ModEntry.oldMariner != null && ModEntry.oldMariner.Value.Tile.X == tileLocation.X && ModEntry.oldMariner.Value.Tile.Y == tileLocation.Y)
                        {
                            string playerTerm = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));

                            if (ModEntry.marinerQuestionsWrongToday.Value)
                            {
                                string preface = Helper.Translation.Get("SwimMod_Mariner_Wrong_Today");
                                Game1.drawObjectDialogue(string.Format(preface, playerTerm));
                            }
                            else
                            {
                                Response[] answers = new Response[]
                                {
                                new Response("SwimMod_Mariner_Questions_Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")),
                                new Response("SwimMod_Mariner_Questions_No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No"))
                                };
                                __instance.createQuestionDialogue(Game1.parseText(String.Format(Helper.Translation.Get(Game1.player.mailReceived.Contains("SwimMod_Mariner_Already") ? "SwimMod_Mariner_Questions_Old" : "SwimMod_Mariner_Questions").ToString(), playerTerm)), answers, "SwimMod_Mariner_Questions");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_UpdateWhenCurrentLocation_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void GameLocation_isCollidingPosition_Postfix(GameLocation __instance, ref bool __result, Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
        {
            try
            {
                if (__result == false || !isFarmer || character?.Equals(Game1.player) != true || !Game1.player.swimming.Value || ModEntry.isUnderwater.Value)
                    return;

                Vector2 next = SwimUtils.GetNextTile();
                //Monitor.Log($"Checking collide {SwimUtils.doesTileHaveProperty(__instance.map, (int)next.X, (int)next.Y, "Water", "Back") != null}");
                if ((int)next.X <= 0 || (int)next.Y <= 0 || __instance.Map.Layers[0].LayerWidth <= (int)next.X || __instance.Map.Layers[0].LayerHeight <= (int)next.Y || SwimUtils.doesTileHaveProperty(__instance.map, (int)next.X, (int)next.Y, "Water", "Back") != null)
                {
                    __result = false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_isCollidingPosition_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void GameLocation_sinkDebris_Postfix(GameLocation __instance, bool __result, Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
        {
            try
            {
                if (__result == false || !Game1.IsMasterGame || !SwimUtils.DebrisIsAnItem(debris))
                    return;

                if (ModEntry.diveMaps.ContainsKey(__instance.Name) && ModEntry.diveMaps[__instance.Name].DiveLocations.Count > 0)
                {
                    Point pos = new Point((int)chunkTile.X, (int)chunkTile.Y);
                    Location loc = new Location(pos.X, pos.Y);

                    DiveMap dm = ModEntry.diveMaps[__instance.Name];
                    DiveLocation diveLocation = null;
                    foreach (DiveLocation dl in dm.DiveLocations)
                    {
                        if (dl.GetRectangle().X == -1 || dl.GetRectangle().Contains(loc))
                        {
                            diveLocation = dl;
                            break;
                        }
                    }

                    if (diveLocation == null)
                    {
                        Monitor.Log($"sink debris: No dive destination for this point on this map");
                        return;
                    }

                    if (Game1.getLocationFromName(diveLocation.OtherMapName) == null)
                    {
                        Monitor.Log($"sink debris: Can't find destination map named {diveLocation.OtherMapName}", LogLevel.Warn);
                        return;
                    }

                    
                    foreach(Chunk chunk in debris.Chunks)
                    {

                        if(chunk.position.Value == chunkPosition)
                        {
                            Monitor.Log($"sink debris: creating copy of debris {debris.debrisType} item {debris.item != null} on {diveLocation.OtherMapName}");

                            if (debris.debrisType.Value != Debris.DebrisType.ARCHAEOLOGY && debris.debrisType.Value != Debris.DebrisType.OBJECT && chunk.randomOffset % 2 != 0)
                            {
                                Monitor.Log($"sink debris: non-item debris");
                                break;
                            }

                            Debris newDebris;
                            Vector2 newTile = diveLocation.OtherMapPos == null ? chunkTile : new Vector2(diveLocation.OtherMapPos.X, diveLocation.OtherMapPos.Y);
                            Vector2 newPos = new Vector2(newTile.X * Game1.tileSize, newTile.Y * Game1.tileSize);
                            if (debris.item != null)
                            {
                                newDebris = Game1.createItemDebris(debris.item, newPos, Game1.random.Next(4));
                                Game1.getLocationFromName(diveLocation.OtherMapName).debris.Add(newDebris);
                            }
                            else
                            {
                                Game1.createItemDebris(ItemRegistry.Create(debris.itemId.Value, 1, debris.itemQuality, false), newPos, Game1.random.Next(4), Game1.getLocationFromName(diveLocation.OtherMapName));
                            }
                            break; 
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_sinkDebris_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}