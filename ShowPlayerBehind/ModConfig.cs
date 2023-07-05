using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace ShowPlayerBehind
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float InnerTransparency { get; set; } = 0.6f;
        public float OuterTransparency { get; set; } = 0.7f;
        public float CornerTransparency { get; set; } = 0.8f;
}
}