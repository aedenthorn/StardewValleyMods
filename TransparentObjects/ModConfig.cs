using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace TransparentObjects
{
    public class ModConfig
    {
        public bool EnableMod{ get; set; }
        public float MinTransparency { get; set; }
        public int TransparencyMaxDistance { get; set; }

        public ModConfig()
        {
            MinTransparency = 0.1f;
            TransparencyMaxDistance = 192;
            EnableMod = true;
        }
    }
}