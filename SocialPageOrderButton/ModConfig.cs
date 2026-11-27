using StardewModdingAPI;

namespace SocialPageOrderMenu
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int CurrentSort { get; set; } = 0;
        public SButton prevButton { get; set; } = SButton.Up;
        public SButton nextButton { get; set; } = SButton.Down;
    }
}
