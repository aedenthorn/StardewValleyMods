
using StardewModdingAPI;

namespace FarmerPortraits
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool ShowWithQuestions { get; set; } = true;
        public bool ShowWithEvents { get; set; } = false;
        public bool ShowWithNPCPortrait { get; set; } = true;
        public bool ShowMisc { get; set; } = false;
        public bool FacingFront { get; set; } = false;
        public bool UseCustomPortrait { get; set; } = false;
        public bool UseCustomBackground { get; set; } = true;
    }
}
