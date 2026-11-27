using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace PlantAll
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Utility), nameof(Utility.tryToPlaceItem))]
        private static class UtilitytryToPlaceItem_Patch
        {
            private static void Postfix(GameLocation location, Item item, int x, int y, bool __result)
            {
                if (!Config.EnableMod || !__result || (!SHelper.Input.IsDown(Config.ModButton) && !SHelper.Input.IsDown(Config.StraightModButton) && !SHelper.Input.IsDown(Config.SprinklerModButton)) || !IsValidItem(item))
                    return;
                PlantAll(item, location, x / 64, y / 64);
            }
        }

        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.performObjectDropInAction))]
        private static class IndoorPot_performObjectDropInAction_Patch
        {
            private static void Postfix(Object __instance, Item dropInItem, bool probe, Farmer who, bool __result)
            {
                if (!Config.EnableMod || probe || !__result || (!SHelper.Input.IsDown(Config.ModButton) && !SHelper.Input.IsDown(Config.StraightModButton) && !SHelper.Input.IsDown(Config.SprinklerModButton)) || !IsValidItem(dropInItem))
                    return;
                PlantAll(dropInItem, who.currentLocation, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y);
            }
        }

    }
}