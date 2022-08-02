using HarmonyLib;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PetBed
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Pet), nameof(Pet.warpToFarmHouse))]
        public static class Pet_warpToFarmHouse_Patch
        {
            public static bool Prefix(Pet __instance, Farmer who)
            {
                SMonitor.Log("Warping pet to farmhouse");
                return !Config.EnableMod || Game1.random.NextDouble() > Config.BedChance / 100f || !WarpPetToBed(__instance, Utility.getHomeOfFarmer(who), false);
            }
        }

        [HarmonyPatch(typeof(Pet), nameof(Pet.setAtFarmPosition))]
        public static class Pet_setAtFarmPosition_Patch
        {
            public static bool Prefix(Pet __instance)
            {
                SMonitor.Log("Setting pet to farm position");
                return !Config.EnableMod || Game1.random.NextDouble() > Config.BedChance / 100f || !Game1.IsMasterGame || Game1.isRaining || !WarpPetToBed(__instance, Game1.getFarm(), true);
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.dayUpdate))]
        public static class Pet_dayUpdate_Patch
        {
            public static void Prefix(Pet __instance, ref bool __state)
            {
                if (!Config.EnableMod)
                    return;
                if(__instance.currentLocation is Farm && !Game1.isRaining && Game1.random.NextDouble() < Config.BedChance / 100f && Game1.IsMasterGame)
                {
                    __state = true;
                }
                if(__instance.currentLocation is FarmHouse && Game1.isRaining && Game1.random.NextDouble() < Config.BedChance / 100f && Game1.IsMasterGame)
                {
                    __state = true;
                }
            }
            public static void Postfix(Pet __instance, bool __state)
            {
                if (__state)
                {
                    SMonitor.Log("Setting pet to bed position");
                    WarpPetToBed(__instance, __instance.currentLocation, true);
                }
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.HasSittingFarmers))]
        public static class Furniture_HasSittingFarmers_Patch
        {
            public static bool Prefix(Furniture __instance, ref bool __result)
            {
                if (Config.EnableMod && __instance is not BedFurniture && Furniture.isDrawingLocationFurniture)
                {
                    foreach(var c in Game1.currentLocation.characters)
                    {
                        if(c is Pet && __instance.boundingBox.Value.Intersects(c.GetBoundingBox()))
                        {
                            (c as Pet).isSleepingOnFarmerBed.Value = false;
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Microsoft.Xna.Framework.Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) })]
        public static class GameLocation_isCollidingPosition_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Furniture), nameof(Furniture.IntersectsForCollision)))
                    {
                        SMonitor.Log("adding check for pet on furniture");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckForPetOnBed))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_S, 6));
                        codes.Insert(i + 1, codes[i - 3].Clone());
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static bool CheckForPetOnBed(bool result, Furniture f, Character character)
        {

            if (!Config.EnableMod || character is not Pet || !f.boundingBox.Value.Intersects(character.GetBoundingBox()))
            {
                var y = f;
                return result;
            }
            return false;
        }
    }
}