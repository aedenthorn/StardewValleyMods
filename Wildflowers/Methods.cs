using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wildflowers
{
    public partial class ModEntry
    {
        private static bool IsCropDataInvalid(Crop crop, CropData cropData)
        {
            return (!string.IsNullOrEmpty(cropData.harvestName) && Game1.objectData.TryGetValue(crop.indexOfHarvest.Value, out var harvest) && harvest.Name != cropData.harvestName || (!string.IsNullOrEmpty(cropData.cropName) && Game1.objectData.TryGetValue(crop.netSeedIndex.Value, out var objData) && objData.Name != cropData.cropName));
        }
        private static int SwitchExpType(int type, Crop crop, HoeDirt dirt)
        {
            if (!Config.ModEnabled || dirt?.modData.ContainsKey(wildKey) != true)
                return type;
            return Farmer.foragingSkill;
        }
    }
}