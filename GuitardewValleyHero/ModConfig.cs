
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace GuitardewValleyHero
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int PerfectScoreLeeway { get; set; } = 8;
        public int ButtonSwell { get; set; } = 50;
        public int NoteSwell { get; set; } = 100;
        public Color BarColor { get; set; } = Color.White;
        public KeybindList ResetKeys { get; set; } = KeybindList.Parse("LeftShift + F5");
        public KeybindList FretKey1 { get; set; } = KeybindList.Parse("H");
        public KeybindList FretKey2 { get; set; } = KeybindList.Parse("J");
        public KeybindList FretKey3 { get; set; } = KeybindList.Parse("K");
        public KeybindList FretKey4 { get; set; } = KeybindList.Parse("L");
    }
}
