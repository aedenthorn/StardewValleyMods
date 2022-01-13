
using StardewModdingAPI;

namespace NightEventChanceTweak
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool CumulativeChance { get; set; } = true;
        public float FairyChance { get; set; } = 1;
        public float WitchChance { get; set; } = 1;
        public float MeteorChance { get; set; } = 1;
        public float OwlChance { get; set; } = 0.5f;
        public float CapsuleChance { get; set; } = 0.8f;

    }
}
