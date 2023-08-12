using StardewModdingAPI;

namespace Swim
{
    public class ModConfig
    {
        public bool EnableMod { get; set; }
        public bool ReadyToSwim { get; set; }
        public bool SwimIndoors { get; set; }
        public bool SwimSuitAlways { get; set; }
        public bool NoAutoSwimSuit { get; set; }
        public bool ShowOxygenBar { get; set; }
        public int JumpTimeInMilliseconds { get; set; }
        public SButton SwimKey { get; set; }
        public SButton SwimSuitKey { get; set; }
        public SButton DiveKey { get; set; }
        public int OxygenMult { get; set; }
        public int BubbleMult { get; set; }
        public bool AllowActionsWhileInSwimsuit { get; set; }
        public bool AllowRunningWhileInSwimsuit { get; set; }
        public bool AddFishies { get; set; }
        public bool AddCrabs { get; set; }
        public bool BreatheSound { get; set; }
        public bool EnableClickToSwim { get; set; }
        public int MineralPerThousandMin { get; set; }
        public int MineralPerThousandMax { get; set; }
        public int CrabsPerThousandMin { get; set; }
        public int CrabsPerThousandMax { get; set; }
        public int PercentChanceCrabIsMimic { get; set; }
        public int MinSmolFishies { get; set; }
        public int MaxSmolFishies { get; set; }
        public int BigFishiesPerThousandMin { get; set; }
        public int BigFishiesPerThousandMax { get; set; }
        public int OceanForagePerThousandMin { get; set; }
        public int OceanForagePerThousandMax { get; set; }
        public int MinOceanChests { get; set; }
        public int MaxOceanChests { get; set; }
        public bool SwimRestoresVitals { get; set; }
        public SButton ManualJumpButton { get; set; }
        public float TriggerDistanceMult { get; set; }
        public int TriggerDistanceUp { get; set; }
        public int TriggerDistanceDown { get; set; }
        public int TriggerDistanceLeft { get; set; }
        public int TriggerDistanceRight { get; set; }

        public ModConfig()
        {
            SwimKey = SButton.J;
            SwimSuitKey = SButton.K;
            DiveKey = SButton.H;
            ManualJumpButton = SButton.MouseRight;

            EnableMod = true;
            ReadyToSwim = true;
            SwimIndoors = false;
            ShowOxygenBar = true;
            SwimSuitAlways = false;
            EnableClickToSwim = true;
            BreatheSound = true;
            SwimRestoresVitals = false;

            JumpTimeInMilliseconds = 500;
            OxygenMult = 2;
            BubbleMult = 1;

            AllowActionsWhileInSwimsuit = true;
            AllowRunningWhileInSwimsuit = false;

            AddFishies = true;
            AddCrabs = true;

            MineralPerThousandMin = 10;
            MineralPerThousandMax = 30;
            CrabsPerThousandMin = 1;
            CrabsPerThousandMax = 5;
            PercentChanceCrabIsMimic = 10;
            MinSmolFishies = 50;
            MaxSmolFishies = 100;
            BigFishiesPerThousandMin = 20;
            BigFishiesPerThousandMax = 50;
            OceanForagePerThousandMin = 1;
            OceanForagePerThousandMax = 10;
            MinOceanChests = 0;
            MaxOceanChests = 3;

            TriggerDistanceMult = 1f;
            TriggerDistanceUp = 144;
            TriggerDistanceDown = 96;
            TriggerDistanceLeft = 130;
            TriggerDistanceRight = 130;
        }
    }
}