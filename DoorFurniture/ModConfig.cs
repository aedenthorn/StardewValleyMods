using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DoorFurniture
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool AutoOpen { get; set; } = false;
        public int NPCCloseDelay { get; set; } = 10;
        public int AutoCloseDelay { get; set; } = -1;
        public int PreventAutoCloseBuffer { get; set; } = 32;
        public int WoodenDoorPrice { get; set; } = 2000;
        public int MetalDoorPrice { get; set; } = 5000;
        public string WoodenDoorIngredients { get; set; } = "388 10 709 5";
        public string MetalDoorIngredients { get; set; } = "335 10 338 5";
        public KeybindList FlipButton { get; set; } = new KeybindList(SButton.F);
        public KeybindList LockButton { get; set; } = new KeybindList(SButton.L);
        public KeybindList CombineButton { get; set; } = new KeybindList(SButton.MouseMiddle);
        public KeybindList CopyButton { get; set; } = new KeybindList(SButton.C);
        public KeybindList ColorButton { get; set; } = new KeybindList(SButton.C);
        public KeybindList RenameButton { get; set; } = new KeybindList(SButton.R);
        public Color KeyCategoryColor { get; set; } = Color.DarkGray;
        public Color DoorCategoryColor { get; set; } = Color.SaddleBrown;
        public Color KeyRingCategoryColor { get; set; } = Color.Brown;
        public bool FixSliderBar { get; set; } = true;
    }
}
