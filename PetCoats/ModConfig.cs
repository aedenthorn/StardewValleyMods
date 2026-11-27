using StardewModdingAPI;

namespace PetCoats
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton CoatKey { get; set; } = SButton.LeftControl;
        public SButton BreedKey { get; set; } = SButton.LeftAlt;
        public SButton TypeKey { get; set; } = SButton.RightAlt;
    }
}
