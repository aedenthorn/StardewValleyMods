using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using Object = StardewValley.Object;

namespace CloserCrops
{
    public partial class ModEntry
    {
        public static bool DefaultMiniCrop(Crop crop)
        {
            return crop != null && Config.CloserCrops.Contains(crop.netSeedIndex.Value) && (!crop.forageCrop.Value || crop.whichForageCrop.Value != "2");
        }
        public static bool TryGetMiniCropNumber(Crop crop, out int num)
        {
            num = 0;
            if (crop == null)
            {
                return false;
            }
            if (!crop.modData.TryGetValue(numberKey, out var str) || !int.TryParse(str, out num))
            {
                return false;
            }
            return true;
        }
        public static bool TryGetMiniCropWhich(Crop crop, out int num)
        {
            num = 0;
            if (crop == null)
            {
                return false;
            }
            if (!crop.modData.TryGetValue(whichKey, out var str) || !int.TryParse(str, out num))
            {
                return false;
            }
            return true;
        }
        public static bool IsCloserCropseed(Object obj)
        {
            return obj != null && obj.HasTypeObject() && obj.Category == Object.SeedsCategory && (Config.CloserCrops.Contains(obj.ItemId) || (Config.ModKey != SButton.None && SHelper.Input.IsDown(Config.ModKey) == Config.ModKeyForCloser));
        }
        public static float CheckScale(float value, Crop crop)
        {
            if(!Config.ModEnabled || !TryGetMiniCropNumber(crop, out _) || crop.currentPhase.Value == 0)
                return value;
            return value * Config.Scale;
        }
        public static double CheckRandom(double value, Crop crop)
        {
            if(!Config.ModEnabled || !TryGetMiniCropWhich(crop, out int add))
                return value;
            return value + add;
        }
    }
}