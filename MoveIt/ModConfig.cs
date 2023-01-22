using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace MoveIt
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ProtectOverwrite { get; set; } = true;
        public string Sound { get; set; } = "shwip";
        public SButton ModKey { get; set; } = SButton.LeftAlt;
        public SButton MoveKey { get; set; } = SButton.MouseLeft;
        public SButton CancelKey { get; set; } = SButton.Escape;
    }
}
