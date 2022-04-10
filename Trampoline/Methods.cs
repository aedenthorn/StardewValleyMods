using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace Trampoline
{
    public partial class ModEntry
    {
        private static bool IsOnTrampoline(Farmer farmer = null)
        {
            if (farmer == null)
                farmer = Game1.player;
            return farmer.IsSitting() && farmer.sittingFurniture is Furniture && (farmer.sittingFurniture as Furniture).modData.ContainsKey(trampolineKey);
        }
    }
}