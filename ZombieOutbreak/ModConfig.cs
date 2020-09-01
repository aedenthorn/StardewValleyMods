using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace ZombieOutbreak
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float GreenAmount { get; set; } = 0.8f;
        public float DailyZombificationChance { get; set; } = 0.1f;
        public int InfectionDistance { get; set; } = 128;
        public float InfectionChancePerSecond { get; set; } = 0.05f;
    }
}