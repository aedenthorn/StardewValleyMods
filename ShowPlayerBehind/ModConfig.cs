using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace ShowPlayerBehind
{
    public class ModConfig
    {
        public bool EnableMod{ get; set; }
        public float InnerTransparency { get; set; } = 0.5f;
        public float OuterTransparency { get; set; } = 0.7f;

    }
}