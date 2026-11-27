using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace TransparentObjects
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool MakeTransparent { get; set; } = true;
        public bool RequireButtonDown { get; set; } = false;
        public SButton ToggleButton { get; set; } = SButton.NumPad0;
        public float MinTransparency { get; set; } = 0.1f;
        public int TransparencyMaxDistance { get; set; } = 192;
        public string[] Exceptions { get; set; } = {
            "Ornamental Hay Bale",
            "Log Section",
            "Campfire",
        };
        public string[] Allowed { get; set; } = new string[0];
    }
}