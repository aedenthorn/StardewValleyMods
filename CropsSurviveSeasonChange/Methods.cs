using StardewValley;

namespace CropsSurviveSeasonChange
{
    public partial class ModEntry
    {
        private static bool CheckKill(bool outdoors, Crop crop, GameLocation environment)
        {
            if (!Config.ModEnabled || crop.forageCrop.Value || crop.dead.Value || (!Config.IncludeRegrowables && crop.GetData().RegrowDays != -1) || (environment.GetSeason() == Season.Winter && !Config.IncludeWinter))
            {
                return outdoors;
            }
            return false;
        }
    }
}