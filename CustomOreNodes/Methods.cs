using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomOreNodes
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {


        private void ReloadOreData(bool first = false)
        {

            customOreNodesList.Clear();
            Helper.GameContent.InvalidateCache(dictPath);
            CustomOreData data = new();
            Dictionary<int, int> existingPSIs = new Dictionary<int, int>();

            var dict = Helper.GameContent.Load<Dictionary<string, CustomOreNode>>(dictPath);
            foreach (var kvp in dict)
            {
                customOreNodesList.Add(kvp.Value);

            }
            if (first)
                Monitor.Log($"Got {customOreNodesList.Count} ores total", LogLevel.Debug);
        }


        private static bool IsInRange(OreLevelRange range, GameLocation location, bool mineOnly)
        {

            int difficulty = (location is MineShaft) ? ((location as MineShaft).mineLevel > 120 ? Game1.netWorldState.Value.SkullCavesDifficulty : Game1.netWorldState.Value.MinesDifficulty) : 0;

            return (range.minLevel < 1 && !(location is MineShaft) && !mineOnly) || (location is MineShaft && (range.minLevel <= (location as MineShaft).mineLevel && (range.maxLevel < 0 || (location as MineShaft).mineLevel <= range.maxLevel))) && (range.minDifficulty <= difficulty) && (range.maxDifficulty < 0 || range.maxDifficulty >= difficulty);
        }
    }
}
 