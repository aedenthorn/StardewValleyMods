using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ImmersiveScarecrows
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool ShowRangeWhenPlacing { get; set; } = true;
        public float Scale { get; set; } = 4;
        public float Alpha { get; set; } = 1;
        public Color RangeTint { get; set; } = Color.White;
        public float RangeAlpha { get; set; } = 1;
        public int DrawOffsetX { get; set; } = 0;
        public int DrawOffsetY { get; set; } = 0;
        public int DrawOffsetZ { get; set; } = 0;
        public SButton PickupButton { get; set; } = SButton.E;
        public bool PickupNearby { get; set; } = false;
        public SButton ShowRangeButton { get; set; } = SButton.LeftAlt;
        public SButton ShowAllRangeButton { get; set; } = SButton.RightAlt;
    }
}
