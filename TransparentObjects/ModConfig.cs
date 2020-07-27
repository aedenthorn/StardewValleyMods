using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace TransparentObjects
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float MinTransparency { get; set; } = 0.1f;
        public int TransparencyMaxDistance { get; set; } = 192;

    }
}