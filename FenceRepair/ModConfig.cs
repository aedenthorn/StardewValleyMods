using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace FenceRepair
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; }
        public SButton ShowHealthKey { get; set; } = SButton.LeftControl;
        public bool ShowNumber { get; set; } = true;
        public bool ToggleShow { get; set; } = false;
        public bool RequireMats { get; set; } = true;
        public bool TakeMatsFromInventory { get; set; } = true;
        public bool TakeMatsFromChests { get; set; } = true;
        public Color ColorNumber { get; set; } = Color.White;
        public Color ColorHigh { get; set; } = Color.Green;
        public Color ColorMid { get; set; } = Color.Yellow;
        public Color ColorLow { get; set; } = Color.Red;
    }
}
