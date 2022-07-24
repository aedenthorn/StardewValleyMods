using StardewModdingAPI;
using System.Collections.Generic;

namespace Skateboard
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public SButton RideButton { get; set; } = SButton.MouseRight;
        public float MaxSpeed { get; set; } = 10;
        public float Acceleration { get; set; } = 1;
        public float Deceleration { get; set; } = 0.5f;
    }
}
