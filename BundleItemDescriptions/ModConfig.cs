
using Microsoft.Xna.Framework;

namespace BundleItemDescriptions
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowMissingItemIcons { get; set; } = true;
        public bool ShowMissingItemNames { get; set; } = true;
        public bool ShowMissingItemDetails { get; set; } = false;
        public bool ShowMissingAchievementNames { get; set; } = true;
        public bool ShowMissingAchievementDetails { get; set; } = false;
        public bool ShowMissingPowerIcons { get; set; } = true;
        public bool ShowMissingPowerNames { get; set; } = true;
        public bool ShowMissingPowerDetails { get; set; } = false;
        public Color MissingRecipeTint { get; set; } = new Color(100, 50, 50, 255);
        public bool Debug { get; set; } = false;
    }
}
