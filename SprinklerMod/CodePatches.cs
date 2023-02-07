using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Object = StardewValley.Object;

namespace SprinklerMod
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.performRemoveAction))]
        public class Object_performRemoveAction_Patch
        {
            public static void Postfix(Object __instance, Vector2 tileLocation, GameLocation environment)
            {
                if (!Config.EnableMod || !__instance.IsSprinkler())
                    return;
                DeactiveateSprinkler(tileLocation, environment);
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.GetBaseRadiusForSprinkler))]
        public class Object_GetBaseRadiusForSprinkler_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if (!Config.EnableMod)
                    return;
                if ((__instance.bigCraftable.Value && sprinklerDict.TryGetValue(__instance.ParentSheetIndex + "", out var radius)) || sprinklerDict.TryGetValue(__instance.Name, out radius))
                {
                    int.TryParse(radius, out __result);
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public class Object_canBePlacedHere_Patch
        {
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.IsSprinkler())
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.isPlaceable))]
        public class Object_isPlaceable_Patch
        {
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.IsSprinkler())
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.performToolAction))]
        public class Object_performToolAction_Patch
        {
            public static void Prefix(Object __instance, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.IsSprinkler())
                    return;
                //__instance.Fragility = 0;
                __instance.Type = "Crafting";
                var y = __instance.bigCraftable.Value;
                var z = 1;
            }
        }
    }
}
