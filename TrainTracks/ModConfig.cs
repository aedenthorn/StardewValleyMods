
using StardewModdingAPI;

namespace TrainTracks
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int DefaultSpeed { get; set; } = 3;
        public int MaxSpeed { get; set; } = 20;
        public SButton PlaceTrackKey { get; set; } = SButton.MouseLeft;
        public SButton RemoveTrackKey { get; set; } = SButton.MouseRight;
        public SButton PrevTrackKey { get; set; } = SButton.PageUp;
        public SButton NextTrackKey { get; set; } = SButton.PageDown;
        public SButton SpeedUpKey { get; set; } = SButton.Up;
        public SButton SlowDownKey { get; set; } = SButton.Down;
        public SButton TurnLeftKey { get; set; } = SButton.Left;
        public SButton TurnRightKey { get; set; } = SButton.Right;
        public SButton ReverseKey { get; set; } = SButton.R;
        public SButton TogglePlacingKey { get; set; } = SButton.Insert;
        public SButton PlaceTrainKey { get; set; } = SButton.Enter;
        public string PlaceTrackSound { get; set; } = "axe";
        public string PlaceTrainSound { get; set; } = "thudStep";
        public string RemoveSound { get; set; } = "axchop";
        public string SwitchSound { get; set; } = "shwip";
        public string SpeedSound { get; set; } = "Cowboy_gunshot";
        public string ReverseSound { get; set; } = "dwop";
    }
}
