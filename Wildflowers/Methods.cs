using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wildflowers
{
    public partial class ModEntry
    {
        private static int GetRandomFlowerSeed(int[] flowers)
        {
            var crops = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            var idxs = new List<int>();
            foreach(var kvp in crops)
            {
                var split = kvp.Value.Split('/');
                if (!flowers.Contains(int.Parse(split[3])))
                    continue;
                if (!split[1].Split(' ').Contains(Game1.currentSeason))
                    continue;
                idxs.Add(kvp.Key);
            }
            if(idxs.Count > 0)
                return idxs[Game1.random.Next(idxs.Count)];
            return -1;
        }
        private static bool IsCropDataInvalid(Crop crop, CropData cropData)
        {
            return (!string.IsNullOrEmpty(cropData.harvestName) && Game1.objectInformation.TryGetValue(crop.indexOfHarvest.Value, out string harvest) && !harvest.StartsWith(cropData.harvestName + "/")) || (!string.IsNullOrEmpty(cropData.cropName) && Game1.objectInformation.TryGetValue(crop.netSeedIndex.Value, out string cropName) && cropName.Split('/', StringSplitOptions.None)[0] != cropData.cropName);
        }
        private static int SwitchExpType(int type, Crop crop)
        {
            if (!Config.ModEnabled || crop.whichForageCrop.Value != -424242)
                return type;
            return Farmer.foragingSkill;
        }
    }
}