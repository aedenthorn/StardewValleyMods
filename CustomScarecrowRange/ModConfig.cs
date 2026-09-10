using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace ImmersiveSprinklersAndScarecrows
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int ScarecrowRadius { get; set; } = 9;
        public int DeluxeRadius { get; set; } = 17;
        public Color ScarecrowRangeTint { get; set; } = Color.Green;
        public float RangeAlpha { get; set; } = 1;
        public bool SquareScarecrowRange { get; set; } = false;
        public SButton ShowScarecrowRangeButton { get; set; } = SButton.RightControl;
    }
}
