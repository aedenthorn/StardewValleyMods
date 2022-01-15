using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using Object = StardewValley.Object;

namespace TappablePalmTrees
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static bool Tree_UpdateTapperProduct_Prefix(Tree __instance, Object tapper_instance)
        {
            if (!Config.EnableMod || tapper_instance == null || (__instance.treeType.Value != 6 && __instance.treeType.Value != 9))
                return true;
            SMonitor.Log("Updating palm tree tapper");
            float time_multiplier = tapper_instance != null && tapper_instance.ParentSheetIndex == 264 ? 0.5f : 1;
            tapper_instance.heldObject.Value = GetObjectFromID(Config.Product);
            tapper_instance.MinutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor((double)(Config.DaysToFill * time_multiplier))));
            return false;
        }
    }
}