
using StardewModdingAPI;

namespace WikiLinks
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool SendToBack { get; set; } = true;
        public SButton LinkModButton { get; set; } = SButton.RightShift;
    }
}
