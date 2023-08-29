using StardewValley;

namespace CropsSurviveSeasonChange
{
    public partial class ModEntry
    {
        private static bool CheckKill(bool outdoors, Crop crop, GameLocation environment)
        {
            if (!Config.ModEnabled || crop.forageCrop.Value || crop.dead.Value || (!Config.IncludeRegrowables && crop.regrowAfterHarvest.Value != -1) || (environment.GetSeasonForLocation() == "winter" && !Config.IncludeWinter))
            {
                return outdoors;
            }
            return false;
        }
    }
}