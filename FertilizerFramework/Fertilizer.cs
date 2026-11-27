using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FertilizerFramework
{
    public class Fertilizer
    {
        public string ItemId { get; set; }
        public BoostValue QualityBoost { get; set; }
        public BoostValue YieldBoost { get; set; }
        public BoostValue SpeedBoost { get; set; }
        public ChanceValue RetentionChance { get; set; }
        public ChanceValue BigCropChance { get; set; }
        public bool SurviveSeasons { get; set; }
        public bool SurviveWinter { get; set; }
        public ChanceValue RepelCrows { get; set; }
    }

    public class BoostValue
    {
        public int Value { get; set; } = -1;
        public int Min { get; set; } = -1;
        public int Max { get; set; } = -1;
    }

    public class ChanceValue
    {
        public float Value { get; set; } = -1;
        public float Min { get; set; } = -1;
        public float Max { get; set; } = -1;
    }
}