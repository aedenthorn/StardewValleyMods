using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace BeePaths
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowWhenRaining { get; set; } = true;
        public bool FixFlowerFind { get; set; } = true;
        public int NumberBees { get; set; } = 5;
        public int BeeRange { get; set; } = 5;
        public Color BeeColor { get; set; } = new Color(155, 85, 0, 255);
        public float BeeScale { get; set; } = 4;
        public float BeeSpeed { get; set; } = 4;
        public int BeeDamage { get; set; } = 4;
        public int BeeStingChance { get; set; } = 0;
        public string BeeSound { get; set; } = "flybuzzing";
        public float MaxSoundDistance { get; set; } = 0;

    }
}
