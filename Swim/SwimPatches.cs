using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
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
		internal static void FarmerRenderer_draw_Prefix(Farmer who, ref bool __state)
        {
            try
            {
                if(who.swimming && Game1.currentLocation.Name.StartsWith("Underwater"))
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
                if (Game1.player.swimming)
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
                if (__instance.exitLocation != null && __instance.exitLocation.Location.waterTiles != null && Game1.player.positionBeforeEvent != null && __instance.exitLocation.Location.waterTiles[(int)(Game1.player.positionBeforeEvent.X),(int)(Game1.player.positionBeforeEvent.Y)])
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


        public static void Farmer_updateCommon_Prefix(Farmer __instance, ref float[] __state)
        {
            try
            {
                __state = new float[0];
                if(__instance.swimming && ModEntry.changeLocations.ContainsKey(Game1.currentLocation.Name) && Config.ReadyToSwim)
                {
                    __state = new float[]{
                        __instance.stamina,
                        __instance.health
                    };
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_updateCommon_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Farmer_updateCommon_Postfix(Farmer __instance, float[] __state)
        {
            try
            {
                if(__state.Length == 2)
                {
                    if (__instance.stamina == __state[0] + 1)
                        __instance.stamina = __state[0];
                    if (__instance.health == __state[1] + 1)
                        __instance.health = (int)__state[1];
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_updateCommon_Postfix)}:\n{ex}", LogLevel.Error);
            }
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
        public static bool Toolbar_draw_Prefix()
        {
            try
            {
                if (Game1.currentLocation != null && Game1.currentLocation.Name == "AbigailCave")
                    return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Toolbar_draw_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
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
                if(__instance.Name == "ScubaCrystalCave")
                {
                    if (Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        __instance.mapPath.Value = "Maps\\CrystalCaveDark";
                    }
                    else
                    {
                        __instance.mapPath.Value = "Maps\\CrystalCave";
                        ModEntry.oldMariner = new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(10f, 7f) * 64f, 2, "Old Mariner", null);

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
                if(__instance.Name == "ScubaCrystalCave")
                {
                    if (!Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        ModEntry.oldMariner.draw(b);
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
                if(__instance.Name == "ScubaCrystalCave")
                {
                    if (!Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed") && ModEntry.oldMariner != null && position.Intersects(ModEntry.oldMariner.GetBoundingBox()))
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
                if (__instance.Name == "ScubaCrystalCave")
                {
                    if (!Game1.player.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        if (ModEntry.oldMariner != null)
                        {
                            ModEntry.oldMariner.update(time, __instance);
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
                    if (!Game1.player.swimming)
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
                    if (Game1.player.swimming)
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
                if (__instance.Name == "ScubaCrystalCave")
                {
                    if (!who.mailReceived.Contains("SwimMod_Mariner_Completed"))
                    {
                        if (ModEntry.oldMariner != null && ModEntry.oldMariner.getTileX() == tileLocation.X && ModEntry.oldMariner.getTileY() == tileLocation.Y)
                        {
                            string playerTerm = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));

                            if (ModEntry.marinerQuestionsWrongToday)
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
                if (__result == false || !isFarmer || !character.Equals(Game1.player) || !Game1.player.swimming || ModEntry.isUnderwater)
                    return;
                Vector2 next = SwimUtils.GetNextTile();
                if (__instance.Map.Layers[0].LayerWidth <= (int)next.X || __instance.Map.Layers[0].LayerHeight <= (int)next.Y || SwimUtils.doesTileHaveProperty(__instance.map, (int)next.X, (int)next.Y, "Water", "Back") != null)
                {
                    __result = false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_isCollidingPosition_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}