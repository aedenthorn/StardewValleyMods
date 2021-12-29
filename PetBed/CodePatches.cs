using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace PetBed
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static bool Pet_warpToFarmHouse_Prefix(Pet __instance, Farmer who, ref int ____currentBehavior)
        {
            SMonitor.Log("Warping pet to farmhouse");
            return !Config.EnableMod || Game1.random.NextDouble() > Config.BedChance / 100f || !WarpPetToBed(__instance, Utility.getHomeOfFarmer(who), ref ____currentBehavior, false);
        }
        
        private static bool Pet_setAtFarmPosition_Prefix(Pet __instance, ref int ____currentBehavior)
        {
            SMonitor.Log("Setting pet to farm position");
            return !Config.EnableMod || Game1.random.NextDouble() > Config.BedChance / 100f || !Game1.IsMasterGame || Game1.isRaining || !WarpPetToBed(__instance, Game1.getFarm(), ref ____currentBehavior, true);
        }
    }
}