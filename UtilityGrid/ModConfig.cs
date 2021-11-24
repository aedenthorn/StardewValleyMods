
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace UtilityGrid
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton ToggleGrid { get; set; } = SButton.Home;
        public SButton SwitchGrid { get; set; } = SButton.Delete;
        public SButton SwitchTile { get; set; } = SButton.PageUp;
        public SButton RotateTile { get; set; } = SButton.PageDown;
        public SButton PlaceTile { get; set; } = SButton.MouseLeft;
        public Color WaterColor { get; set; } = Color.Aqua;
        public Color ElectricityColor { get; set; } = Color.Yellow;
    }
}
