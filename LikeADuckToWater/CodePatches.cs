using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LikeADuckToWater
{
    public partial class ModEntry
    {
        public class FarmAnimal_updatePerTenMinutes_Patch
        {
            public static void Postfix(FarmAnimal __instance)
            {
                if (NotReadyToSwim(__instance))
                    return;
                TryAddToQueue(__instance, __instance.currentLocation);
            }
        }
        
        public class FarmAnimal_MovePosition_Patch
        {
            public static void Prefix(FarmAnimal __instance, ref Vector2 __state)
            {
                if (!Config.ModEnabled || !__instance.CanSwim())
                    return;
                __state = __instance.Position;
            }
            public static void Postfix(FarmAnimal __instance, GameLocation currentLocation, Vector2 __state)
            {
                if (!Config.ModEnabled || !__instance.CanSwim())
                    return;
                if(__instance.hopOffset == Vector2.Zero && __state != __instance.Position)
                {
                    __instance.isSwimming.Value = currentLocation.isWaterTile(__instance.TilePoint.X, __instance
                    .TilePoint.Y);
                }
            }
        }

        public class FarmAnimal_Eat_Patch
        {
            public static void Postfix(FarmAnimal __instance)
            {
                if (NotReadyToSwim(__instance))
                    return;
                TryAddToQueue(__instance, __instance.currentLocation);
            }
        }
        
        public class FarmAnimal_pet_Patch
        {
            public static void Postfix(FarmAnimal __instance, bool is_auto_pet)
            {
                if (is_auto_pet || NotReadyToSwim(__instance))
                    return;
                TryAddToQueue(__instance, __instance.currentLocation);
            }
        }
              
        public class GameLocation_isCollidingPosition_Patch
        {
            public static bool Prefix(GameLocation __instance, Rectangle position, Character character, ref bool __result)
            {
                if (!isCollidingWater(__instance, character, position.X / 64, position.Y / 64))
                    return false;
                return true;
            }
        }
        public class FarmAnimal_dayUpdate_Patch
        {
            public static void Postfix(FarmAnimal __instance)
            {
                __instance.modData.Remove(swamTodayKey);
            }
        }
        public class FarmAnimal_HandleHop_Patch
        {
            public static void Postfix(FarmAnimal __instance)
            {
                if (__instance.hopOffset == Vector2.Zero)
                {
                    Point p = __instance.TilePoint;
                    __instance.isSwimming.Value = __instance.currentLocation.doesTileHaveProperty(p.X, p.Y, "Water", "Back") != null;
                }
                if (!__instance.modData.ContainsKey(swamTodayKey)) 
                {
                    SwamToday(__instance);
                }
            }
        }

        public class GameLocation_isOpenWater_Patch
        {
            public static bool Prefix(GameLocation __instance, int xTile, int yTile, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (!__instance.isWaterTile(xTile, yTile))
                {
                    __result = false;
                    return false;
                }
                int tile_index = __instance.getTileIndexAt(xTile, yTile, "Buildings");
                if (tile_index != -1)
                {
                    bool tile_blocked = !(__instance.getTileSheetIDAt(xTile, yTile, "Buildings") == "outdoors" && waterBuildingTiles.Contains(tile_index));
                    if (tile_blocked)
                    {
                        __result = false;
                        return false;
                    }
                }
                __result = !__instance.objects.ContainsKey(new Vector2((float)xTile, (float)yTile));
                return false;
            }
        }
    }
}