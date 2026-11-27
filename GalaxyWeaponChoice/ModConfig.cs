using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace GalaxyWeaponChoice
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool MenuOnMailbox { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.None;
        public SButton MenuKey { get; set; } = SButton.F15;
        public string InboxText { get; set; } = "Mailbox";
        public string ArchiveText { get; set; } = "Old Mail";
        public int WindowWidth { get; set; } = 1600;
        public int WindowHeight { get; set; } = 1000;

        public int GridColumns { get; set; } = 4;
        public int EnvelopeWidth { get; set; } = 256;
        public int EnvelopeHeight { get; set; } = 192;
        public int SideWidth { get; set; } = 194;
        public int GridSpace { get; set; } = 64;

    }
}
