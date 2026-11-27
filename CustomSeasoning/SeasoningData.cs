using Microsoft.Xna.Framework;
using StardewValley.GameData.Buffs;

namespace CustomSeasoning
{
    public class SeasoningData
    {
        public BuffAttributesData BuffData { get; set; }
        public string ChangedName { get; set; }
        public float PriceMultiplier { get; set; } = 1;
        public float DurationMultiplier { get; set; } = 1;
        public int BaseDuration { get; set; } = 600;
        public int Quality { get; set; }
        public Color Tint { get; set; } = Color.Transparent;
        public float TintAlpha { get; set; } = 0.1f;
        public string TriggerAction { get; set; }
        public int Emote { get; set; } = -1;
    }
}