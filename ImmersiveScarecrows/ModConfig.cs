using StardewModdingAPI;

namespace ImmersiveScarecrows
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float Scale { get; set; } = 4;
        public float Alpha { get; set; } = 1;
        public SButton PickupButton { get; set; } = SButton.E;
        public SButton ActivateButton { get; set; } = SButton.Enter;

    }
}
