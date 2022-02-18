using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace CustomSpousePatioRedux
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int MaxSpousesPerPage { get; set; } = 6;
        public SButton PatioWizardKey { get; set; } = SButton.F8;
    }
}
