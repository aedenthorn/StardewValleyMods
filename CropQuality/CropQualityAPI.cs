using StardewValley;

namespace CropQuality
{
    public interface ICropQualityAPI
    {
        public int GetCropQuality(Crop crop, int add = 0);
    }
    public class CropQualityAPI : ICropQualityAPI
    {
        public int GetCropQuality(Crop crop, int add = 0)
        {
            return ModEntry.GetQuality(crop, add);
        }
    }
}