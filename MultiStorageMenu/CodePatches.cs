using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Linq;

namespace MultiStorageMenu
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Chest), nameof(Chest.clearNulls))]
        public class Chest_clearNulls_Patch
        {
            public static bool Prefix(Chest __instance)
            {
                return true;

            }
        }
        [HarmonyPatch(typeof(Chest), nameof(Chest.GetActualCapacity))]
        public class Chest_GetActualCapacity_Patch
        {
            public static void Postfix(Chest __instance, ref int __result)
            {
                __result = 12 * Config.ChestRows;
            }
        }
    }
}