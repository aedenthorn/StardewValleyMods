using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Object = StardewValley.Object;

namespace CloserCrops
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public static class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || !IsCloserCropseed(__instance))
                    return true;
                Vector2 placementTile = new(x / 64, y / 64);

                if (!location.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt dirt || dirt.crop?.netSeedIndex.Value != __instance.ItemId || dirt.crop.currentPhase.Value > 0)
                    return true;
                if(dirt.crop.modData.TryGetValue(numberKey, out var str) && int.TryParse(str, out var num))
                {
                    if (num == 4)
                        return true;
                }
                else
                {
                    return true;
                }
                var add = Math.Min(__instance.Stack, 4 - num);
                __instance.Stack -= add;
                location.playSound("dirtyHit", placementTile);
                dirt.crop.modData[numberKey] = (num + add).ToString();
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public static class Object_canBePlacedHere_Patch
        {
            public static bool Prefix(Object __instance, GameLocation l, Vector2 tile, CollisionMask collisionMask, bool showError, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || !IsCloserCropseed(__instance) || !l.terrainFeatures.TryGetValue(tile, out var tf) || tf is not HoeDirt dirt || dirt.crop?.netSeedIndex.Value != __instance.ItemId || dirt.crop.currentPhase.Value > 0)
                    return true;
                int num = 1;
                if(dirt.crop.modData.TryGetValue(numberKey, out var str) && int.TryParse(str, out num))
                {
                    if (num == 4)
                        return true;
                }
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.performUseAction))]
        public static class HoeDirt_performUseAction_Patch
        {
            public static bool Prefix(HoeDirt __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || !TryGetMiniCropNumber(__instance.crop, out var num) || num == 4 || __instance.crop.netSeedIndex.Value != Game1.player?.ActiveObject?.ItemId || __instance.crop.currentPhase.Value > 0)
                    return true;
                var add = Config.AutoPlantMax ? Math.Min(Game1.player.ActiveObject.Stack, 4 - num) : 1;
                Game1.player.ActiveObject.Stack -= add - 1;
                Game1.player.reduceActiveItemByOne();
                __instance.Location.playSound("dirtyHit", __instance.Tile);
                __instance.crop.modData[numberKey] = (num + add).ToString();
                __result = true; 
                return false;
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.plant))]
        public static class HoeDirt_plant_Patch
        {
            public static void Postfix(HoeDirt __instance, string itemId, Farmer who, bool isFertilizer, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || ((Config.ModKey != SButton.None && SHelper.Input.IsDown(Config.ModKey) != Config.ModKeyForCloser && !DefaultMiniCrop(__instance.crop)) || (Config.ModKey == SButton.None && !DefaultMiniCrop(__instance.crop))) || !__result || who is null || isFertilizer || who.ActiveObject?.ItemId != __instance.crop.netSeedIndex.Value || __instance.crop.modData.ContainsKey(numberKey))
                    return;

                var add = Config.AutoPlantMax ? Math.Min(Game1.player.ActiveObject.Stack, 4) : 1;
                who.ActiveObject.Stack -= add - 1;
                __instance.crop.modData[numberKey] = add.ToString();
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.addItemToInventoryBool))]
        public static class Farmer_addItemToInventoryBool_Patch
        {
            public static void Postfix(Farmer __instance, Item item, ref bool __result)
            {
                if (!Config.ModEnabled || !skipCheckHarvest || __result)
                    return;
                Game1.createItemDebris(item, new Vector2((float)(__instance.Tile.X * 64 + 32), (float)(__instance.Tile.Y * 64 + 32)), -1, null, -1, false);
                __result = true;
            }
        }
        private static bool skipCheckHarvest;

        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public static class Crop_harvest_Patch
        {
            public static bool Prefix(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester, bool isForcedScytheHarvest, ref bool __result)
            {
                if (!Config.ModEnabled || skipCheckHarvest || !Config.MultiplyPlantAndHarvest || !TryGetMiniCropNumber(__instance, out var num))
                    return true;
                skipCheckHarvest = true;
                var days = __instance.dayOfCurrentPhase.Value;
                for (int i = 0; i < num; i++)
                {
                    __instance.modData[whichKey] = (i + 1).ToString();
                    __result = __instance.harvest(xTile, yTile, soil, junimoHarvester, true);
                    __instance.modData.Remove(whichKey);
                    if (__instance.RegrowsAfterHarvest() && i < num - 1 && __instance.dayOfCurrentPhase.Value != days)
                    {
                        __instance.dayOfCurrentPhase.Value = 0;
                    }
                    else if (!__result)
                    {
                        skipCheckHarvest = false;
                        return false;
                    }
                }
                skipCheckHarvest = false;
                return false;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.harvest");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Utility), nameof(Utility.CreateRandom)))
                    {
                        SMonitor.Log("Adding quality randomizer");
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckRandom))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.draw))]
        public static class Crop_draw_Patch
        {
            private static bool skip = false;
            public static bool Prefix(Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
            {
                if (__instance.drawPosition.X < 0)
                    __instance.drawPosition *= -1; // allow drawing of this crop
                if (!Config.ModEnabled || skip)
                    return true;

                int num;
                if(Config.MultiplyPlantAndHarvest)
                {
                    if(!TryGetMiniCropNumber(__instance, out num))
                        return true;
                }
                else
                {
                    if(!DefaultMiniCrop(__instance))
                            return true;
                    num = 4;
                }

                var layerDepth = __instance.layerDepth;
                var coloredLayerDepth = __instance.coloredLayerDepth;
                var drawPosition = __instance.drawPosition;
                skip = true;
                var tempDrawPosition = drawPosition - new Vector2(16, 24);
                __instance.drawPosition = tempDrawPosition;
                __instance.draw(b, tileLocation, toTint, rotation);
                if(num > 1)
                {
                    __instance.layerDepth = layerDepth + 0.5f / 10000f;
                    __instance.coloredLayerDepth = coloredLayerDepth + 1 / 10000f;
                    __instance.drawPosition = tempDrawPosition + new Vector2(32, 0);
                    __instance.draw(b, tileLocation, toTint, rotation);
                }
                if (num > 2)
                {
                    __instance.layerDepth = layerDepth + 32f /10000f;
                    __instance.coloredLayerDepth = coloredLayerDepth + 32 / 10000f;
                    __instance.drawPosition = tempDrawPosition + new Vector2(0, 32);
                    __instance.draw(b, tileLocation, toTint, rotation);
                }
                if (num > 3)
                {
                    __instance.layerDepth = layerDepth + 32.5f / 10000f;
                    __instance.coloredLayerDepth = coloredLayerDepth + 33 / 10000f;
                    __instance.drawPosition = tempDrawPosition + new Vector2(32, 32);
                    __instance.draw(b, tileLocation, toTint, rotation);
                }
                __instance.layerDepth = layerDepth;
                __instance.coloredLayerDepth = coloredLayerDepth;
                __instance.drawPosition = drawPosition;
                skip = false;
                __instance.drawPosition *= -1; // prevent further drawing of this crop
                return false;

            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float f && f == 4)
                    {
                        SMonitor.Log("Adding scale");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckScale))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}