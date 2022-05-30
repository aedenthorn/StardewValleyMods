
using StardewModdingAPI;

namespace FoodOnTheTable
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int MinutesToHungry { get; set; } = 300;
        public float MoveToFoodChance { get; set; } = 0.1f;
        public float MaxDistanceToEat { get; set; } = 3f;
        public float PointsMult { get; set; } = 0.5f;
        public bool CountAsFedSpouse { get; set; } = true;
    }
}
