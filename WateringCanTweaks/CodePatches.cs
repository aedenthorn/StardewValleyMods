using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace WateringCanTweaks
{
    public partial class ModEntry
    {
        public static int tileCount = 0;
        public static int wateredTiles = 0;
        public static bool Tool_draw_Prefix(Tool __instance)
        {
            return (!Config.EnableMod || !Config.FillAdjacent || __instance is not WateringCan || __instance.UpgradeLevel == 0);
        }
        public static bool WateringCan_DoFunction_Prefix(WateringCan __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            if (!Config.EnableMod || !Config.FillAdjacent || Game1.currentLocation.CanRefillWateringCanOnTile(x / 64, y / 64) || who.toolPower == 0)
                return true;
            var ptr = AccessTools.Method(typeof(Tool), nameof(Tool.DoFunction)).MethodHandle.GetFunctionPointer();
            var baseMethod = (Action<GameLocation, int, int, int, Farmer>)Activator.CreateInstance(typeof(Action<GameLocation, int, int, int, Farmer>), __instance, ptr);
            baseMethod(location, x, y, power, who);
            who.stopJittering();

            return false;
        }
        public static void WateringCan_DoFunction_Postfix(WateringCan __instance, int x, int y, Farmer who)
        {
            if (!Config.EnableMod || !Game1.currentLocation.CanRefillWateringCanOnTile(x / 64, y / 64))
                return;
            __instance.waterCanMax = (int)(__instance.waterCanMax * Config.VolumeMult);
            AccessTools.FieldRefAccess<WateringCan, int>(__instance, "waterLeft") = __instance.waterCanMax;
            SMonitor.Log($"Filled watering can to {__instance.waterCanMax}");
        }
        public static bool Farmer_toolPowerIncrease_Prefix(Farmer __instance, ref int ___toolPitchAccumulator)
        {
            if (!Config.EnableMod || !Config.FillAdjacent || __instance.CurrentTool is not WateringCan || __instance.CurrentTool.UpgradeLevel == 0)
                return true;

            if (AccessTools.FieldRefAccess<WateringCan, int>(__instance.CurrentTool as WateringCan, "waterLeft") <= 0)
                return false;

            SMonitor.Log($"Tool power {__instance.toolPower}");

            int level = __instance.CurrentTool.UpgradeLevel;

            switch (__instance.toolPower)
            {
                case 0:
                    ___toolPitchAccumulator = 0;
                    tileCount = (int)(3 * Config.WaterMult);
                    break;
                case 1:
                    tileCount = (int)(5 * Config.WaterMult);
                    break;
                case 2:
                    tileCount = (int)(9 * Config.WaterMult);
                    break;
                case 3:
                    tileCount = (int)(18 * Config.WaterMult);
                    break;
            }


            Vector2 startTile = new Vector2((int)__instance.GetToolLocation().X  / 64, (int)__instance.GetToolLocation().Y  / 64);

            SMonitor.Log($"Trying to water tiles starting at {startTile}");

            if (__instance.CurrentTool.UpgradeLevel <= __instance.toolPower || (__instance.CurrentTool as WateringCan).WaterLeft <= 0 || !__instance.currentLocation.terrainFeatures.TryGetValue(startTile, out TerrainFeature feature) || feature is not HoeDirt)
                return false;

            var wateredTiles = 0;
            var tilesToWater = tileCount;

            SMonitor.Log($"Trying to water {tilesToWater} tiles");

            List<Vector2> tiles = new();
            if((feature as HoeDirt).state.Value == 0)
            {
                (__instance.currentLocation.terrainFeatures[startTile] as HoeDirt).state.Value = 1;
                wateredTiles++;
                SMonitor.Log($"watered tile {startTile}");
                if (!(__instance.CurrentTool as WateringCan).IsBottomless && !__instance.hasWateringCanEnchantment)
                {
                    AccessTools.FieldRefAccess<WateringCan, int>(__instance.CurrentTool as WateringCan, "waterLeft")--;
                    if (AccessTools.FieldRefAccess<WateringCan, int>(__instance.CurrentTool as WateringCan, "waterLeft") <= 0)
                    {
                        SMonitor.Log("Watering can empty");
                        return false;
                    }
                }
            }
            tileCount--;
            if (tileCount > 0)
                WaterTiles(__instance, tiles, startTile);
            if(wateredTiles > 0)
            {
                if (__instance.ShouldHandleAnimationSound())
                {
                    __instance.currentLocation.localSound("wateringCan");
                }
                SMonitor.Log($"watered {wateredTiles} tiles");

            }
            if (!__instance.CurrentTool.IsEfficient)
            {
                var staminaUsed = ((2 * (__instance.toolPower + 1)) - __instance.FarmingLevel * 0.1f) * Config.ChargedStaminaMult;
                __instance.Stamina -= staminaUsed;
                SMonitor.Log($"Used {staminaUsed} stamina");
            }
            SMonitor.Log($"Increasing tool power to {__instance.toolPower + 1}");
            __instance.toolPower++;

            return false;
        }

        private static void WaterTiles(Farmer instance, List<Vector2> tiles, Vector2 startTile)
        {
            var adjacents = Utility.getAdjacentTileLocations(startTile);
            for(int i = adjacents.Count - 1; i >= 0; i--)
            {
                if (tiles.Contains(adjacents[i]) || !instance.currentLocation.terrainFeatures.TryGetValue(adjacents[i], out TerrainFeature feature) || feature is not HoeDirt)
                {
                    adjacents.RemoveAt(i);
                    continue;
                }
                if ((feature as HoeDirt).state.Value == 0)
                {
                    (instance.currentLocation.terrainFeatures[adjacents[i]] as HoeDirt).state.Value = 1;
                    wateredTiles++;
                    SMonitor.Log($"watered tile {adjacents[i]}");
                    if(!(instance.CurrentTool as WateringCan).IsBottomless)
                    {
                        AccessTools.FieldRefAccess<WateringCan, int>(instance.CurrentTool as WateringCan, "waterLeft")--;
                        if (AccessTools.FieldRefAccess<WateringCan, int>(instance.CurrentTool as WateringCan, "waterLeft") <= 0)
                        {
                            SMonitor.Log("Watering can empty");
                            return;
                        }
                    }
                }
                tileCount--;
                if (tileCount <= 0)
                    return;
                tiles.Add(adjacents[i]);
            }
            foreach(var tile in adjacents)
            {
                WaterTiles(instance, tiles, tile);
            }
        }
    }
}