
using Microsoft.Xna.Framework;

namespace ToolUpgraders
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool AlwaysAllowPet { get; set; } = true;
        public int FrameMilliseconds { get; set; } = 300;
        public int MovementTicks { get; set; } = 15;
        public bool Debug { get; set; } = false;
    }
}
