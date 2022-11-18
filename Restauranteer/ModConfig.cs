using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace Restauranteer
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float OrderChance { get; set; } = 0.1f;
        public float LovedDishChance { get; set; } = 0.7f;

    }
}
