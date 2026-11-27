using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ResourceStorage
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool AutoUse { get; set; } = true;
        public bool ShowMessage { get; set; } = false;
        public string AutoStore { get; set; } = "Sap,Wood,Hardwood,Stone,Coal,Fiber,Clay";
        public SButton ModKey1 { get; set; } = SButton.LeftShift;
        public SButton ModKey2 { get; set; } = SButton.LeftControl;
        public SButton ModKey3 { get; set; } = SButton.LeftAlt;
        public int ModKey1Amount { get; set; } = 999;
        public int ModKey2Amount { get; set; } = 100;
        public int ModKey3Amount { get; set; } = 10;
        public SButton ResourcesKey { get; set; } = SButton.R;
        public int IconOffsetX { get; set; } = 0;
        public int IconOffsetY { get; set; } = 0;
    }
}
