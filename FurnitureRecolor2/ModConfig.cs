using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FurnitureRecolor
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public KeybindList ColorButton { get; set; } = new KeybindList(SButton.C);
        public bool FixSliderBar { get; set; } = true;
        public float MaxDiff { get; set; } = 0.25f;
        public int MaxGroups { get; set; } = 5;
    }
}
