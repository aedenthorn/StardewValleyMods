
using StardewModdingAPI;

namespace Screenshot
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton ScreenshotKey { get; set; } = SButton.F9;
        public string ScreenshotFolder { get; set; } = "Screenshots";
        public string Message { get; set; } = "Screenshot saved to {0}";
    }
}
