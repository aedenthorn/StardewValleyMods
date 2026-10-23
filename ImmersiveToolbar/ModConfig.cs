
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ImmersiveToolbar
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int ScrollSpeedPercent { get; set; } = 100;
        public float BackgroundOpacity { get; set; } = 1f;
        public float FadeOpacity { get; set; } = 0.66f;
        public int FadeDelay { get; set; } = 120;
        public float AdjacentOpacity { get; set; } = 0.5f;
        public bool AlwaysShowAdjacent { get; set; } = false;
        public bool ShowNumbers { get; set; } = true;
    }
}
