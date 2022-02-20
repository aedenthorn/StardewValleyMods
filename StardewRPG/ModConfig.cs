
namespace StardewRPG
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        // death
        public bool PermaDeath { get; set; } = true;
        public int PermaDeathScreenTicks { get; set; } = 1500;
        public float ExperienceLossPercentOnDeath { get; set; } = 1f;
        public bool OnlyLoseExpToNextLevel { get; set; } = true;


        // levelling
        public bool ManualSkillUpgrades { get; set; } = true;
        public bool NotifyOnLevelUp { get; set; } = true;
        public float LevelIncrementExpMult { get; set; } = 1f;
        public int BaseHealthPerLevel { get; set; } = 5;
        public int BaseStaminaPerLevel { get; set; } = 10;


        // stats
        public int BaseStatValue { get; set; } = 9;
        public int DefaultStatValue { get; set; } = 12;
        public int StartStatExtraPoints { get; set; } = 20;
        public int StatPointsPerStardrop { get; set; } = 3;
        public int MinStatValue { get; set; } = 2;
        public int MaxStatValue { get; set; } = 18;
        public string StatBonusLevels { get; set; } = "13,16,18";
        public string StatPenaltyLevels { get; set; } = "8,5,2";


        // bonuses
        public float StrClubDamageBonus { get; set; } = 0.1f;
        public int StrClubSpeedBonus { get; set; } = 20;
        public float StrCritDamageBonus { get; set; } = 0.1f;
        public float StrFishingReelSpeedBonus { get; set; } = 0.001f;
        public float StrFishingTreasureSpeedBonus { get; set; } = 0.00625f;
        
        public float ConSwordDamageBonus { get; set; } = 0.1f;
        public int ConSwordSpeedBonus { get; set; } = 20;
        public float ConHealthBonus { get; set; } = 0.1f;
        public float ConStaminaBonus { get; set; } = 0.1f;
        
        public float DexDaggerDamageBonus { get; set; } = 0.1f;
        public int DexDaggerSpeedBonus { get; set; } = 20;
        public int DexFishingBobberSizeBonus { get; set; } = 8;
        public float DexCritChanceBonus { get; set; } = 0.1f;
        
        public float WisCraftResourceReqBonus { get; set; } = 0.1f;
        public float WisCraftTimeBonus { get; set; } = 0.1f;
        public float WisExpBonus { get; set; } = 0.1f;


        // tools
        public float ToolLevelReqMult { get; set; } = 2f;
        public float WeaponLevelReqMult { get; set; } = 0.5f;

    }
}
