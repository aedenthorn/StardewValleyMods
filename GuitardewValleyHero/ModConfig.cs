
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace GuitardewValleyHero
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public KeybindList ResetKeys { get; set; } = KeybindList.Parse("LeftShift + F6");
        public SButton StringOneKey { get; set; } = SButton.J;
        public SButton StringTwoKey { get; set; } = SButton.K;
        public SButton StringThreeKey { get; set; } = SButton.L;
        public SButton StringFourKey { get; set; } = SButton.OemSemicolon;
    }
}
