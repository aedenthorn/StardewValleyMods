using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace Swim
{
    public class ModConfig
    {
        public bool EnableMod{ get; set; }
        public bool SwimByDefault { get; set; }
        public int JumpTimeInMilliseconds { get; set; }
        public SButton SwimKey{ get; set; }
        public SButton SwimSuitKey { get; set; }
        public SButton DiveKey { get; set; }
        public float ChanceTreasure { get; set; }

        public ModConfig()
        {
            SwimKey = SButton.J;
            SwimSuitKey = SButton.K;
            DiveKey = SButton.H;
            EnableMod = true;
            SwimByDefault = false;
            JumpTimeInMilliseconds = 500;
            ChanceTreasure = 0.9f;
        }
    }
}