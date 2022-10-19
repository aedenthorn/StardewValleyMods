using HarmonyLib;
using StardewValley;
using StardewValley.Objects;

namespace CustomStarterPackage
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Chest), nameof(Chest.dumpContents))]
        public class Chest_dumpContents_Patch
        {
            public static void Postfix(Chest __instance, GameLocation location)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(chestKey))
                    return;
                Game1.drawObjectDialogue(SHelper.Translation.Get("open-message"));
            }
        }
    }
}