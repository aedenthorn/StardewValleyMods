using System.Collections.Generic;

namespace Skateboard
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float MaxSpeed { get; set; } = 10;
        public float Accelleration { get; set; } = 1;
        public float Deccelleration { get; set; } = 0.5f;
    }
}
