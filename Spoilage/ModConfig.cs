using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace Spoilage
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool QualityReduction { get; set; } = true;
        public bool Spoiling { get; set; } = true;
        public bool RemoveSpoiled { get; set; } = false;
        public bool DisplayDays { get; set; } = true;
        public bool DisplayDaysLeft { get; set; } = true;
        public int FruitsDays { get; set; } = 5;
        public int VegetablesDays { get; set; } = 5;
        public int GreensDays { get; set; } = 4;
        public int FlowersDays { get; set; } = 3;
        public int EggDays { get; set; } = 3;
        public int MilkDays { get; set; } = 2;
        public int CookingDays { get; set; } = 2;
        public int MeatDays { get; set; } = 1;
        public int FishDays { get; set; } = 1;
        public float FridgeMult { get; set; } = 0.1f;
        public float PlayerMult { get; set; } = 1.5f;
        public int SpoiledIndex { get; set; } = 168;
        public Color CustomSpoiledColor { get; set; } = Color.Lime;
        public Dictionary<string, SpoilData> CustomSpoilage { get; set; } = new Dictionary<string, SpoilData>
        {
            { "Sap", new SpoilData() { age = 0 } }
        };
    }
}
