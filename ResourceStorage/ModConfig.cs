using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ResourceStorage
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool AutoUse { get; set; } = true;
        public string AutoStore { get; set; } = "Sap,Wood,Hardwood,Stone,Coal,Fiber,Clay";
        public SButton ModKeyMax = SButton.LeftShift;
    }
}
