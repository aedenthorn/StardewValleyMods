
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace CropGrowthInformation
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool RequireToggle { get; set; } = true;
        public float TextScale { get; set; } = 1f;
        public float CropTransparency { get; set; } = 0.5f;
        public SButton ToggleButton { get; set; } = SButton.LeftAlt;
        public bool ShowCropName { get; set; } = true;
        public bool ShowReadyText { get; set; } = false;
        public bool ShowCurrentPhase { get; set; } = true;
        public bool ShowDaysInCurrentPhase { get; set; } = true;
        public bool ShowTotalGrowth { get; set; } = true;
        public string ReadyText { get; set; } = "Ready!";
        public string PhaseText { get; set; } = "Phase {0}/{1}";
        public string CurrentText { get; set; } = "Day {0}/{1}";
        public string TotalText { get; set; } = "Growth {0}/{1}";
        public Color CurrentPhaseColor { get; set; } = new Color(0, 0, 1f);
        public Color CurrentGrowthColor { get; set; } = new Color(1f, 1f, 0);
        public Color TotalGrowthColor { get; set; } = new Color(0, 1f, 0);
        public Color ReadyColor { get; set; } = new Color(0, 1f, 0);
        public Color NameColor { get; set; } = new Color(0, 1f, 0);
    }
}
