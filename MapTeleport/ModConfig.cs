using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace MapTeleport
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton TeleportKey { get; set; } = SButton.MouseLeft;
    }
}
