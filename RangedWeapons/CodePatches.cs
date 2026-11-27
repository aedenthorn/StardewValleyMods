using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace RangedWeapons
{
    public partial class ModEntry
    {
        public static bool drawingWallPot;
        public static int drawingWallPotOffset;
        public static int drawingWallPotInnerOffset;

        [HarmonyPatch(typeof(Utility), nameof(Utility.playerCanPlaceItemHere))]
        public class playerCanPlaceItemHere_Patch
        {
            public static bool Prefix(GameLocation location, Item item, int x, int y, Farmer f, ref bool __result)
            {
                if (!Config.EnableMod || item is not Object || !(item as Object).bigCraftable.Value || item.ParentSheetIndex != 62 || !typeof(DecoratableLocation).IsAssignableFrom(location.GetType()) || !(location as DecoratableLocation).isTileOnWall(x / 64, y / 64) || !Utility.isWithinTileWithLeeway(x, y, item, f))
                    return true;
                SMonitor.Log($"Placing planter on wall");
                __result = true;
                return false;
            }
        }
    }
}