using StardewModdingAPI;
using System.Collections.Generic;

namespace CustomHay
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float OrdinaryHayChance { get; set; } = 0.5f;
        public float GoldHayChance { get; set; } = 0.75f;
    }
}
