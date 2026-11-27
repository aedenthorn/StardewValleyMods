
using StardewModdingAPI;

namespace MineHelper
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string ChestType { get; set; } = "130";
        public SButton ModKey { get; set; } = SButton.LeftControl;
    }
}
