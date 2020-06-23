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
                if (__instance.exitLocation.Location.waterTiles[(int)(Game1.player.positionBeforeEvent.X),(int)(Game1.player.positionBeforeEvent.Y)])
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
                if(__instance.swimming && ModEntry.changeLocations.ContainsKey(Game1.currentLocation.Name))
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
        public static bool Toolbar_draw_Prefix(Farmer __instance)
        {
            try
            {
                if (Game1.currentLocation.Name == "AbigailCave")
                    return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_changeIntoSwimsuit_Postfix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }


    }
}