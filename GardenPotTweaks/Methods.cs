using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace GardenPotTweaks
{
    public partial class ModEntry
    {
        private static bool IsPotModified(IndoorPot indoorPot)
        {
            return indoorPot.hoeDirt.Value?.crop is not null || indoorPot.bush?.Value is not null || indoorPot.hoeDirt?.Value?.fertilizer?.Value != 0;

        }
        private static int GetBushEffectiveSize(Bush bush)
        {
            if (bush.size.Value == 3)
            {
                return 0;
            }
            if (bush.size.Value == 4)
            {
                return 1;
            }
            return bush.size.Value;
        }
    }
}