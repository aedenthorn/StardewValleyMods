using StardewModdingAPI;

namespace ModThis
{
    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public SButton WizardKey { get; set; } = SButton.Pause;
    }
}
