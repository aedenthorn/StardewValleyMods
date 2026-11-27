using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace CustomToolTile
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool ShowToolIcon { get; set; } = true;
        public bool Prismatic { get; set; } = false;
        public int PrismaticSpeed = 5;
        public Color Color { get; set; } = Color.Green;
        public Color Color2 { get; set; } = Color.Transparent;
        public bool Shadow = true;
        public int ShadowOffset = 4;

    }
}
