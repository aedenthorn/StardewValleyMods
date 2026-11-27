using Microsoft.Xna.Framework;
using StardewValley;

namespace DMT.Data
{
    public record SecondUpdateData
    {
        public long LastTick { get; set; } = context.UpdateTicks.Value - 60;
        public int Loops { get; set; } = 0;

        public Vector2 Tile { get; set; }

        public GameLocation Location { get; set; }

        public float Value { get; set; } = 0f;

        public Farmer Who { get; set; }

        public enum SecondUpdateType
        {
            Health,
            Stamina
        }
        public SecondUpdateType type { get; set; } = SecondUpdateType.Health;
    }
}
