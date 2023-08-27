using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CropsSurviveSeasonChange
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool IncludeRegrowables { get; set; } = false;
        public bool IncludeWinter { get; set; } = false;
    }
}
