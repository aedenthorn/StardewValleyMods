
using StardewModdingAPI;

namespace CustomPictureFrames
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton StartFramingKey { get; set; } = SButton.F10;
        public SButton SwitchFrameKey { get; set; } = SButton.F11;
        public SButton TakePictureKey { get; set; } = SButton.F12;
        public SButton SwitchPictureKey { get; set; } = SButton.F11;
        public SButton DeletePictureKey { get; set; } = SButton.Delete;
        public string Message { get; set; } = "Saving framed picture for {0}";
    }
}
