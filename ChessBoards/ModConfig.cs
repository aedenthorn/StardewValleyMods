
using StardewModdingAPI;

namespace ChessBoards
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool FreeMode { get; set; } = false;
        public bool WalkThrough { get; set; } = false;
        public float HeldPieceOpacity { get; set; } = 1f;
        public string PickupSound { get; set; } = "Cowboy_gunshot";
        public string PlaceSound { get; set; } = "Cowboy_gunshot";
        public string CancelSound { get; set; } = "shiny4";
        public string FlipSound { get; set; } = "bigSelect";
        public string UnflipSound { get; set; } = "bigDeSelect";
        public string SetupSound { get; set; } = "leafrustle";
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
