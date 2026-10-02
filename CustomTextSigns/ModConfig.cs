
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomTextSigns
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool AllSignsTextSigns { get; set; } = true;
        public SButton UpKey { get; set; } = SButton.NumPad8;
        public SButton DownKey { get; set; } = SButton.NumPad2;
        public SButton IncreaseWidthKey { get; set; } = SButton.NumPad6;
        public SButton DecreaseWidthKey { get; set; } = SButton.NumPad4;
        public SButton SuppressKey { get; set; } = SButton.LeftShift;
        public int MaxLength { get; set; } = 256;
    }
}
