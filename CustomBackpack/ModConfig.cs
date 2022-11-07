using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace CustomBackpack
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public Vector2 BackpackPosition { get; set; } = new Vector2(456f, 1088f);
        public bool ShowRowNumbers { get; set; } = true;
        public bool ShowArrows { get; set; } = true;
        public int MinHandleHeight { get; set; } = 16;
        public int ShiftRows { get; set; } = 3;
        public Color HandleColor { get; set; } = new Color(233, 84, 32);
        public Color BackgroundColor { get; set; } = new Color(174, 167, 159);
        public SButton ShowExpandedButton { get; set; } = SButton.RightShoulder;
    }
}
