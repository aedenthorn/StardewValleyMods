
using StardewModdingAPI;

namespace Chess
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool FreeMode { get; set; } = false;
        public float HeldPieceOpacity { get; set; } = 0.75f;
        public string PickupSound { get; set; } = "bigSelect";
        public string PlaceSound { get; set; } = "bigDeSelect";
        public string CancelSound { get; set; } = "leafrustle";
        public string FlipSound { get; set; } = "dwoop";
        public string SetupSound { get; set; } = "yoba";
        public string ClearSound { get; set; } = "leafrustle";
        public SButton ModeKey { get; set; } = SButton.H;
        public SButton SetupKey { get; set; } = SButton.J;
        public SButton FlipKey { get; set; } = SButton.K;
        public SButton ClearKey { get; set; } = SButton.L;
        public SButton ChangeKey { get; set; } = SButton.Tab;
        public SButton ChangeModKey { get; set; } = SButton.LeftShift;
        public SButton RemoveKey { get; set; } = SButton.Delete;
    }
}
