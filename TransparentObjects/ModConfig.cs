using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace TransparentObjects
{
    public class ModConfig
    {
        public bool EnableMod{ get; set; }
        public float ObjectAlpha { get; set; }
        public int TransparencyDiameter { get; set; }

        public ModConfig()
        {
            ObjectAlpha = 0.4f;
            TransparencyDiameter = 128;
            EnableMod = true;
        }
    }
}