
using StardewModdingAPI;

namespace Fetch
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int GrabDistance { get; set; } = 64;
        public int MaxSteps { get; set; } = 20;
    }
}
