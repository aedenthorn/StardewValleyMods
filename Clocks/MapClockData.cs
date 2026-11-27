using Microsoft.Xna.Framework;

namespace Clocks
{
    public class MapClockData
    {
        public string GameLocation { get; set; }
        public Vector2 Tile { get; set; }
        public Vector2 DrawOffset { get; set; }
        public string Background { get; set; }
        public Point BackgroundOffset { get; set; } = Point.Zero;
        public string HourHand { get; set; }
        public string MinuteHand { get; set; }
        public string Nub { get; set; }
        public Rectangle? BackgroundSourceRect { get; set; }
        public Rectangle? HourHandSourceRect { get; set; }
        public Rectangle? MinuteHandSourceRect { get; set; }
        public Rectangle? NubSourceRect { get; set; }
        public Point? HourHandSize { get; set; }
        public Point? MinuteHandSize { get; set; }
        public Point? NubSize { get; set; }
        public Color? HourHandColor { get; set; }
        public Color? MinuteHandColor { get; set; }
        public Color? NubColor { get; set; }
        public float? Alpha { get; set; }
        public float Scale { get; set; } = 1f;
    }
}