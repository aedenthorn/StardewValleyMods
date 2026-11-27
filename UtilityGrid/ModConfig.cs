
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace UtilityGrid
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton ToggleGrid { get; set; } = SButton.Home;
        public SButton ToggleEdit { get; set; } = SButton.End;
        public SButton SwitchGrid { get; set; } = SButton.Delete;
        public SButton SwitchTile { get; set; } = SButton.PageUp;
        public SButton RotateTile { get; set; } = SButton.PageDown;
        public SButton PlaceTile { get; set; } = SButton.MouseLeft;
        public SButton DestroyTile { get; set; } = SButton.MouseRight;
        public Color WaterColor { get; set; } = Color.Aqua;
        public Color UnpoweredGridColor { get; set; } = Color.White;
        public Color ElectricityColor { get; set; } = Color.Yellow;
        public Color InsufficientColor { get; set; } = Color.Red;
        public Color IdleColor { get; set; } = Color.LightGray;
        public Color ShadowColor { get; set; } = Color.Black;
        public int PipeCostGold { get; set; } = 100;
        public int PipeDestroyGold { get; set; } = 50;
        public string PipeCostItems { get; set; } = "378:2";
        public string PipeDestroyItems { get; set; } = "378:1";
        public string PipeSound { get; set; } = "dirtyHit";
        public string DestroySound { get; set; } = "axe";
    }
}
