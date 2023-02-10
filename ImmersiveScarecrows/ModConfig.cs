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
        public int DrawOffsetX { get; set; } = 0;
        public int DrawOffsetY { get; set; } = 0;
        public int DrawOffsetZ { get; set; } = 0;
        public SButton PickupButton { get; set; } = SButton.E;
        public SButton ShowRangeButton { get; set; } = SButton.LeftAlt;

    }
}
