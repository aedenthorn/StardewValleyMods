using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CropStacking
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool CombineColored { get; set; } = true;
        public bool CombinePreserves { get; set; } = true;
        public bool CombineQualities { get; set; } = true;
        public KeybindList CombineKey { get; set; } = new KeybindList(SButton.MouseMiddle);
    }
}
