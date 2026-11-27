using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace PetHats
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public Color TweakedColor { get; set; } = Color.White;
        public int Opacity { get; set; } = 0;

    }
}
