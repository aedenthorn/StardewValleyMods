
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Moolah
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string Separator { get; set; } = ",";
        public int SeparatorInterval { get; set; } = 3;
        public int SeparatorX { get; set; } = 12;
        public int SeparatorY { get; set; } = -4;
    }
}
