using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace NPCStatusIcons
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool RequireModKey { get; set; } = true;
        public bool ShowGiftable { get; set; } = true;
        public bool ShowTalkable { get; set; } = true;
        public bool ShowBirthday { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftAlt;
    }
}
