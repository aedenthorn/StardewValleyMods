
using Microsoft.Xna.Framework;

namespace Clocks
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool FixMapClocks { get; set; } = true;
        public bool SmoothMovement { get; set; } = true;
        public float Alpha { get; set; } = 0.75f;
        public Color HourHandColor { get; set; } = new Color(142, 61, 33);
        public Color MinuteHandColor { get; set; } = new Color(142, 61, 33);
        public Color NubColor { get; set; } = new Color(142, 61, 33);
        public bool Debug { get; set; } = false;
    }
}
