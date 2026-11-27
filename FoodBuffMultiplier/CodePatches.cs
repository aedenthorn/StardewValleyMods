using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using System;
using Object = StardewValley.Object;

namespace FoodBuffMultiplier
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.healthRecoveredOnConsumption))]
        public static class Object_healthRecoveredOnConsumption_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if(!Config.ModEnabled || __instance.QualifiedItemId != "(O)773" || Config.ExcludedFoods.Contains(__instance.ItemId))
                    return;
                __result = (int)Math.Round(__result * Config.HealthMult);
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.staminaRecoveredOnConsumption))]
        public static class Object_staminaRecoveredOnConsumption_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if(!Config.ModEnabled || !__instance.HasTypeObject() || Config.ExcludedFoods.Contains(__instance.ItemId))
                    return;
                __result = (int)Math.Round(__result * Config.EnergyMult);
            }
        }
    }
}