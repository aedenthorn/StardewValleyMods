using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

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
                    tileCount = (int)(2 * Config.WaterMult);
                    break;
                case 2:
                    tileCount = (int)(4 * Config.WaterMult);
                    break;
                case 3:
                    tileCount = (int)(9 * Config.WaterMult);
                    break;
            }


            Vector2 startTile = new Vector2((int)__instance.GetToolLocation().X  / 64, (int)__instance.GetToolLocation().Y  / 64);

            SMonitor.Log($"Trying to water tiles starting at {startTile}");

            HoeDirt dirt = GetHoeDirt(__instance.currentLocation, startTile);
            if (__instance.CurrentTool.UpgradeLevel <= __instance.toolPower || (__instance.CurrentTool as WateringCan).WaterLeft <= 0 || dirt == null)
                return false;

            var wateredTiles = 0;

            SMonitor.Log($"Trying to water {tileCount} tiles");

            List<Vector2> tiles = new();
            bool empty = false;
            if(dirt.state.Value == 0)
            {
                empty = !WaterHoeDirt(__instance, startTile);
                wateredTiles++;

            }
            WaterTiles(__instance, tiles, startTile);

            tiles.Sort(delegate(Vector2 a, Vector2 b)
            {
                return Vector2.Distance(startTile, a).CompareTo(Vector2.Distance(startTile, b));
            });
            foreach (var tile in tiles)
            {
                if (empty || wateredTiles >= tileCount)
                    break;
                dirt = GetHoeDirt(__instance.currentLocation, tile);

                if (dirt.state.Value == 0)
                {
                    empty = !WaterHoeDirt(__instance, tile);
                    wateredTiles++;
                }
            }

            if (wateredTiles > 0)
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

        private static void WaterTiles(Farmer farmer, List<Vector2> tiles, Vector2 startTile)
        {
            var adjacents = Utility.getAdjacentTileLocations(startTile);
            for(int i = adjacents.Count - 1; i >= 0; i--)
            {
                HoeDirt dirt = GetHoeDirt(farmer.currentLocation, adjacents[i]);
                if (tiles.Contains(adjacents[i]) || dirt is null)
                {
                    adjacents.RemoveAt(i);
                    continue;
                }
                tiles.Add(adjacents[i]);
            }
            foreach(var tile in adjacents)
            {
                WaterTiles(farmer, tiles, tile);
            }
        }

        private static HoeDirt GetHoeDirt(GameLocation location, Vector2 tile)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is HoeDirt)
                return feature as HoeDirt;
            else if (SHelper.ModRegistry.IsLoaded("aedenthorn.ConnectedGardenPots") && location.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot)
                return (obj as IndoorPot).hoeDirt.Value;

            return null;
        }
        private static bool WaterHoeDirt(Farmer f, Vector2 tile)
        {
            if (f.currentLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is HoeDirt)
            {
                (f.currentLocation.terrainFeatures[tile] as HoeDirt).state.Value = 1;
            }
            else if (SHelper.ModRegistry.IsLoaded("aedenthorn.ConnectedGardenPots") && f.currentLocation.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot)
            {
                (obj as IndoorPot).hoeDirt.Value.state.Value = 1;
                (obj as IndoorPot).showNextIndex.Value = true;
            }
            SMonitor.Log($"watered tile {tile}");
            if (!(f.CurrentTool as WateringCan).IsBottomless && !f.hasWateringCanEnchantment)
            {
                AccessTools.FieldRefAccess<WateringCan, int>((f.CurrentTool as WateringCan), "waterLeft")--;
                if (AccessTools.FieldRefAccess<WateringCan, int>((f.CurrentTool as WateringCan), "waterLeft") <= 0)
                {
                    SMonitor.Log("Watering can empty");
                    return false;
                }
            }
            return true;
        }
    }
}