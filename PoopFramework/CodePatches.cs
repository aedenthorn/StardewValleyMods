using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoopFramework
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(NPC), nameof(NPC.performTenMinuteUpdate))]
        public class NPC_performTenMinuteUpdate_Patch
        {
            public static void Postfix(NPC __instance, int timeOfDay, GameLocation l)
            {
                if (!Config.ModEnabled)
                    return;
                TryPoop(__instance, timeOfDay, l);
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.performTenMinuteUpdate))]
        public class Farmer_performTenMinuteUpdate_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                if (!Config.ModEnabled || !__instance.IsLocalPlayer)
                    return;
                TryPoop(__instance, Game1.timeOfDay, __instance.currentLocation);
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.updatePerTenMinutes))]
        public class FarmAnimal_updatePerTenMinutes_Patch
        {
            public static void Postfix(FarmAnimal __instance, int timeOfDay, GameLocation environment)
            {
                if (!Config.ModEnabled)
                    return;
                TryPoop(__instance, timeOfDay, environment);
            }
        }

    }
}