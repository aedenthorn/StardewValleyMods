using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swim
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(MetalHead), nameof(MetalHead.shedChunks))]
        public static class MetalHead_shedChunks_Patch
        {
            public static bool Prefix(MetalHead __instance)
            {
                if (!IsMonster(__instance))
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(MetalHead), nameof(MetalHead.takeDamage))]
        public static class MetalHead_takeDamage_Patch
        {
            public static bool Prefix(MetalHead __instance, ref int __result)
            {
                if (!IsMonster(__instance))
                    return true;
                __instance.objectsToDrop.Clear();
                if (__instance.currentLocation.characters.Contains(__instance))
                    __instance.currentLocation.characters.Remove(__instance);
                __result = 1000;
                return false;
            }
        }
        [HarmonyPatch(typeof(MetalHead), nameof(MetalHead.getExtraDropItems))]
        public static class MetalHead_getExtraDropItems_Patch
        {
            public static bool Prefix(MetalHead __instance, ref List<Item> __result)
            {
                if (!IsMonster(__instance))
                    return true;
                __result = new();
                return false;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.Removed))]
        public static class NPC_Removed_Patch
        {
            public static bool Prefix(MetalHead __instance)
            {
                if (!IsMonster(__instance))
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Monster), nameof(Monster.onDealContactDamage))]
        public static class Monster_onDealContactDamage_Patch
        {
            public static bool Prefix(MetalHead __instance)
            {
                if (!IsMonster(__instance, "AbigailMetalHead"))
                    return true;
                Game1.playSound("cowboy_dead");
                ModEntry.abigailTicks.Value = -1;
                return false;
            }
        }
    }
}
