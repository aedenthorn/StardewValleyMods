using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace FarmCaveDoors
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MineDoorX { get; set; } = 2;
        public int MineDoorY { get; set; } = 4;
        public int SkullDoorX { get; set; } = 10;
        public int SkullDoorY { get; set; } = 4;
    }
}
