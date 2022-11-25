using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Linq;

namespace AllChestsMenu
{
    public partial class ModEntry
    {

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