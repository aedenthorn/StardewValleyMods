
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace SubmergedCrabPots
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool SubmergeHarvestable { get; set; } = true;
        public bool ShowRipples { get; set; } = true;
        public Color BobberTint { get; set; } = Color.White;
        public float BobberScale { get; set; } = 4f;
        public int BobberOpacity { get; set; } = 100;
    }
}
