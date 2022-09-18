using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;
using xTile.Dimensions;

namespace CustomBackpack
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public Vector2 BackpackPosition { get; set; } = new Vector2(456f, 1088f);
        public bool ShowRowNumbers { get; set; } = true;
        public bool ShowArrows { get; set; } = true;
    }
}
