using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Crops;
using System;

namespace CropQuality
{
    public partial class ModEntry
    {
        public static int GetQuality(Crop crop, int add = 0)
        {
            uint days;
            if (Config.ConstantQuality || Config.RandomQuality)
            {
                if(!crop.modData.TryGetValue(plantedKey, out var str) || !uint.TryParse(str, out days))
                {
                    days = Config.RandomQuality ? (uint)Game1.random.Next(int.MaxValue) : Game1.stats.DaysPlayed;
                    crop.modData[plantedKey] = days.ToString();
                }
            }
            else
            {
                days = Game1.stats.DaysPlayed;
            }
            if (crop.forageCrop.Value)
            {
                if (Game1.player.professions.Contains(16))
                    return 4;
                Random r = Utility.CreateRandom(days, Game1.uniqueIDForThisGame / 2UL, (double)(crop.tilePosition.X * 1000), (double)(crop.tilePosition.Y * 2000), add);
                if (r.NextDouble() < Game1.player.ForagingLevel / 30f * Config.QualityModifier * Config.IridiumChance)
                {
                    return 4;
                }
                else if (r.NextDouble() < Game1.player.ForagingLevel / 30f * Config.QualityModifier)
                {
                    return 2;
                }
                else if (r.NextDouble() < Game1.player.ForagingLevel / 15f * Config.QualityModifier)
                {
                    return 1;
                }
                return 0;
            }
            else
            {
                CropData data = crop.GetData();

                Random r2 = Utility.CreateRandom(crop.tilePosition.X * 7.0, crop.tilePosition.Y * 11.0, days, Game1.uniqueIDForThisGame, add);

                int fertilizerQualityLevel = crop.Dirt.GetFertilizerQualityBoostLevel();
                
                double chanceForGoldQuality = (0.2 * (Game1.player.FarmingLevel / 10.0) + 0.2 * fertilizerQualityLevel * ((Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01) * Config.QualityModifier;
                chanceForGoldQuality = Math.Min(Config.GoldMaxChance, chanceForGoldQuality);
                
                double chanceForSilverQuality = Math.Min(Config.SilverMaxChance, chanceForGoldQuality * 2.0);

                int cropQuality = 0;
                if (r2.NextDouble() < chanceForGoldQuality * (fertilizerQualityLevel >= 3 ? Config.IridiumChanceFertilized : Config.IridiumChance))
                {
                    cropQuality = 4;
                }
                else if (r2.NextDouble() < chanceForGoldQuality)
                {
                    cropQuality = 2;
                }
                else if (r2.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
                {
                    cropQuality = 1;
                }
                cropQuality = MathHelper.Clamp(cropQuality, (data != null) ? data.HarvestMinQuality : 0, ((data != null) ? data.HarvestMaxQuality : null).GetValueOrDefault(cropQuality));
                return cropQuality;
            }
        }
        public static int SetQuality(int quality, Crop crop)
        {
            if (!Config.ModEnabled || crop.Dirt == null)
                return quality;
            int add = crop.modData.TryGetValue(whichKey, out var str) && int.TryParse(str, out int which) ? which : 0;
            return GetQuality(crop, add);
        }
    }
}