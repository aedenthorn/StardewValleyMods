using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Globalization;

namespace CropVariation
{
    public partial class ModEntry
    {
        private static float ChangeScale(Vector2 tileLocation)
        {
            if (!Config.EnableMod || Config.SizeVariationPercent == 0 || Game1.currentLocation?.terrainFeatures?.TryGetValue(tileLocation, out TerrainFeature f) != true || f is not HoeDirt || (f as HoeDirt).crop is null || (f as HoeDirt).crop.currentPhase.Value == 0 || (!Config.EnableTrellisResize && (f as HoeDirt).crop.raisedSeeds.Value))
                return 4;
            if (!f.modData.TryGetValue(sizeVarKey, out string varString) || !float.TryParse(varString, NumberStyles.Float, CultureInfo.InvariantCulture, out float sizeVarFloat))
                sizeVarFloat = GetRandomSizeVar(f as HoeDirt);
            int sizeVar = (int)Math.Round(sizeVarFloat * Config.SizeVariationPercent);
            return 4 * (1 + sizeVar / 100f);
        }


        private static float GetRandomSizeVar(HoeDirt hoeDirt)
        {
            if (!Config.EnableMod || hoeDirt is null)
                return 0;
            double sv = Game1.random.NextDouble() * 2 - 1;
            hoeDirt.modData[sizeVarKey] = sv + "";
            return (float)sv;
        }

        private static void GetRandomColorVars(HoeDirt hoeDirt)
        {
            if (!Config.EnableMod || hoeDirt is null)
                return;
            hoeDirt.modData[redVarKey] = (Game1.random.NextDouble() * 2 - 1) + "";
            hoeDirt.modData[greenVarKey] = (Game1.random.NextDouble() * 2 - 1) + "";
            hoeDirt.modData[blueVarKey] = (Game1.random.NextDouble() * 2 - 1) + "";
        }
        private static int ChangeQuality(Crop crop, HoeDirt hoeDirt, int quality)
        {
            if (!Config.EnableMod || crop.indexOfHarvest.Value == "771" || crop.indexOfHarvest.Value == "889")
                return quality;
            float factor = 0;
            if(Config.ColorVariationQualityFactor > 0)
            {
                if(hoeDirt.modData.TryGetValue(redVarKey, out string redVarString)
                    && float.TryParse(redVarString, NumberStyles.Float, CultureInfo.InvariantCulture, out float redVarFloat)
                    && hoeDirt.modData.TryGetValue(redVarKey, out string greenVarString)
                    && float.TryParse(greenVarString, NumberStyles.Float, CultureInfo.InvariantCulture, out float greenVarFloat)
                    && hoeDirt.modData.TryGetValue(redVarKey, out string blueVarString)
                    && float.TryParse(blueVarString, NumberStyles.Float, CultureInfo.InvariantCulture, out float blueVarFloat)
                )
                {
                    factor += (redVarFloat + greenVarFloat + blueVarFloat) / 3 * Config.ColorVariationQualityFactor / 100f;
                }
            }
            if(Config.SizeVariationQualityFactor > 0 && hoeDirt.modData.TryGetValue(sizeVarKey, out string sizeVarString) && float.TryParse(sizeVarString, NumberStyles.Float, CultureInfo.InvariantCulture, out float sizeVarFloat))
            {
                factor += sizeVarFloat * Config.SizeVariationQualityFactor / 100f;
            }
            var newQuality = Math.Clamp((int)Math.Round(quality + factor), 0, 4);
            if (newQuality == 3)
                newQuality = 2;
            SMonitor.Log($"Changed quality from {quality} to {newQuality}; var factor {factor}");
            return newQuality;
        }
    }
}