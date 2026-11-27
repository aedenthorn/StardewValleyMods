using Microsoft.Xna.Framework;

namespace FuzzyTime
{
    public class ClockData
    {
        public Vector2 DrawOffset { get; set; }
        public string HourHand { get; set; }
        public string MinuteHand { get; set; }
        public string Nub { get; set; }
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