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
        public bool ShowRangeWhenPlacing { get; set; } = true;
        public bool RequireHoeDirt { get; set; } = true;
        public float Scale { get; set; } = 4;
        public float Alpha { get; set; } = 1;
        public Color BothRangeTint { get; set; } = Color.DarkGreen;
        public Color ScarecrowRangeTint { get; set; } = Color.Green;
        public Color SprinklerRangeTint { get; set; } = Color.Green;
        public float RangeAlpha { get; set; } = 1;
        public int DrawOffsetX { get; set; } = 0;
        public int DrawOffsetY { get; set; } = 0;
        public int DrawOffsetZ { get; set; } = 0;
        public SButton PickupButton { get; set; } = SButton.E;
        public bool PickupNearby { get; set; } = false;
        public SButton ActivateButton { get; set; } = SButton.Enter;
        public bool ActivateNearby { get; set; } = false;
        public int ActivateNearbyRange { get; set; } = 2;
        public int PickupNearbyRange { get; set; } = 2;
        public SButton ShowScarecrowRangeButton { get; set; } = SButton.RightControl;
        public SButton ShowSprinklerRangeButton { get; set; } = SButton.LeftControl;
        public Dictionary<string, int> SprinklerRadii { get; set; } = new()
        {
            { "Sprinkler", 0 },
            { "Quality Sprinkler", 1 },
            { "Iridium Sprinkler", 2 },
            { "PrismaticSprinkler", 3 }
        };
    }
}
