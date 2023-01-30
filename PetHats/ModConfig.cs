using StardewModdingAPI;

namespace PetHats
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public SButton RetrieveButton { get; set; } = SButton.None;

    }
}
