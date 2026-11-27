
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace FashionSenseAppearanceMenu
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public SButton MenuKey { get; set; } = SButton.M;
    }
}
