using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SewerSlimes
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public Dictionary<string, int> SlimeWeights { get; set; } = new()
        {
            { "Green Slime", 1000 },
            { "Frost Jelly", 800 },
            { "Red Sludge", 600 },
            { "Purple Sludge", 400 },
            { "Yellow Slime", 100 },
            { "Black Slime", 50 },
            { "Copper Slime", 80 },
            { "Iron Slime", 50 },
            { "Tiger Slime", 20 },
            { "Aqua Slime", 10 },
            { "Prismatic Slime", 1 }
        };
        public Dictionary<string, int> BigSlimeWeights { get; set; } = new()
        {
            { "Green", 200 },
            { "Blue", 150 },
            { "Red", 100 },
            { "Purple", 50 }
        };
        public int BigChancePercent { get; set; } = 20;
        public int SpecialChancePercent { get; set; } = 10;
        public int MinSlimesPerDay { get; set; } = 1;
        public int MaxSlimesPerDay { get; set; } = 3;
        public int MaxTotalSlimes { get; set; } = 10;
        public List<Rectangle> SpawnAreas { get; set; } = new List<Rectangle>()
        {
            new Rectangle(12, 11, 9, 12)
        };
        public List<Rectangle> ForbidAreas { get; set; } = new List<Rectangle>()
        {
            new Rectangle(28, 15, 7, 8)
        };
    }
}
