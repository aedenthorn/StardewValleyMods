using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace Swim
{
    public class ModConfig
    {
        public bool EnableMod{ get; set; }
        public bool ReadyToSwim { get; set; }
        public bool SwimSuitAlways { get; set; }
        public int JumpTimeInMilliseconds { get; set; }
        public SButton SwimKey{ get; set; }
        public SButton SwimSuitKey { get; set; }
        public SButton DiveKey { get; set; }
        public int OxygenMult { get; set; }
        public int BubbleMult { get; set; }
        public bool AllowActionsWhileInSwimsuit { get; set; }
        public bool AddFishies { get; set; }

        public ModConfig()
        {
            SwimKey = SButton.J;
            SwimSuitKey = SButton.K;
            DiveKey = SButton.H;
            EnableMod = true;
            ReadyToSwim = true;
            SwimSuitAlways = false;
            JumpTimeInMilliseconds = 500;
            OxygenMult = 2;
            BubbleMult = 1;
            AllowActionsWhileInSwimsuit = true;
            AddFishies = true;
        }
    }
}