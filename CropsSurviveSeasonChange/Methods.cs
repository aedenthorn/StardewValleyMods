using StardewValley;

namespace CropsSurviveSeasonChange
{
    public partial class ModEntry
    {
        private static void CheckKill(Crop crop, GameLocation environment)
        {
            if (!Config.ModEnabled || crop.dead.Value || (!Config.IncludeRegrowables && crop.regrowAfterHarvest.Value != -1) || (environment.GetSeasonForLocation() == "winter" && !Config.IncludeWinter))
            {
                crop.Kill();
            }
        }
    }
}