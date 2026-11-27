using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PrismaticFire
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float PrismaticSpeed { get; set; } = 3;
        public string TriggerSound{ get; set; } = "fireball";
        public Color AmethystColor { get; set; } = Color.Purple;
        public Color AquamarineColor { get; set; } = Color.LightBlue;
        public Color EmeraldColor { get; set; } = Color.Green;
        public Color RubyColor { get; set; } = Color.Red;
        public Color TopazColor { get; set; } = Color.Yellow;
        public Color DiamondColor { get; set; } = Color.PaleTurquoise;
    }
}
