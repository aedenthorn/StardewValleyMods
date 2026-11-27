
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FarmerPortraits
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float Scale { get; set; } = 1f;
        public bool ShowWithQuestions { get; set; } = true;
        public bool ShowWithEvents { get; set; } = false;
        public bool ShowWithNPCPortrait { get; set; } = true;
        public bool ShowMisc { get; set; } = false;
        public bool FacingFront { get; set; } = false;
        public bool UseCustomPortrait { get; set; } = true;
        public bool UseCustomBackground { get; set; } = true;
        public KeybindList CustomButton { get; set; } = new KeybindList(new Keybind(SButton.MouseLeft, SButton.LeftShift));
    }
}
