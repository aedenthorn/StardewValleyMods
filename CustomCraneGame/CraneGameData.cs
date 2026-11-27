using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomCraneGame
{
    public class CraneGameData
    {
        public string Name { get; set; }
        public Dictionary<string, List<PrizeData>> NormalPrizes { get; set; } = new();
        public Dictionary<string, SpecialPrizeData> SpecialPrizes { get; set; } = new();
        public int[] PrizeMap { get; set; } = new int[]
{
                0, 0, 0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                0, 1, 0, 2, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 1, 0, 0, 1, 0,
                0, 0, 0, 1, 0, 2, 0, 3
        };
        public int Price { get; set; } = 500;
    }

    public class SpecialPrizeData
    {
        public List<PrizeData> Prizes { get; set; }
        public float Chance { get; set; }
    }

    public class PrizeData
    {
        public string ItemId { get; set; }
        public int Amount { get; set; } = 1;
        public int Quality { get; set; }
        public int Weight { get; set; } = 1;
    }
}