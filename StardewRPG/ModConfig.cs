
namespace StardewRPG
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;


        // leveling
        public bool ManualSkillUpgrades { get; set; } = true;
        public bool NotifyOnLevelUp { get; set; } = true;
        public float LevelIncrementExpMult { get; set; } = 1f;
        public int BaseHealthPerLevel { get; set; } = 5;
        public int BaseStaminaPerLevel { get; set; } = 10;
        public int HealthRegen { get; set; } = 1;
        public float StaminaRegen { get; set; } = 1f;


        // stats
        public int BaseStatValue { get; set; } = 9;
        public int StartStatExtraPoints { get; set; } = 20;
        public int StatPointsPerStardrop { get; set; } = 3;
        public int MinStatValue { get; set; } = 2;
        public int MaxStatValue { get; set; } = 18;
        public string StatBonusLevels { get; set; } = "13,16,18";
        public string StatPenaltyLevels { get; set; } = "8,5,2";


        // bonuses
        public float StrClubDamageBonus { get; set; } = 0.1f;
        public int StrClubSpeedBonus { get; set; } = 20;
        public int StrPickaxeDamageBonus { get; set; } = 1;
        public int StrAxeDamageBonus { get; set; } = 1;
        public float StrCritDamageBonus { get; set; } = 1f;
        public float StrFishingReelSpeedBonus { get; set; } = 0.001f;
        public float StrFishingTreasureSpeedBonus { get; set; } = 0.00625f;
        
        public float ConSwordDamageBonus { get; set; } = 0.1f;
        public int ConSwordSpeedBonus { get; set; } = 20;
        public float ConHealthBonus { get; set; } = 0.1f;
        public float ConStaminaBonus { get; set; } = 0.1f;
        public int ConHealthRegenBonus { get; set; } = 1;
        public float ConStaminaRegenBonus { get; set; } = 1f;
        public bool ConRollToResistDebuff { get; set; } = true;
        public float ConDebuffDurationBonus { get; set; } = 0.1f;
        public int ConDefenseBonus { get; set; } = 1;
        
        public float DexDaggerDamageBonus { get; set; } = 0.1f;
        public float DexRangedDamageBonus { get; set; } = 0.1f;
        public int DexDaggerSpeedBonus { get; set; } = 20;
        public int DexFishingBobberSizeBonus { get; set; } = 8;
        public float DexCritChanceBonus { get; set; } = 1f;
        public bool DexRollForMiss { get; set; } = true;
        
        public float IntSkillLevelsBonus { get; set; } = 0.334f;
        public float IntCropQualityBonus { get; set; } = 0.1f;
        public float IntArtifactSpotChanceBonus { get; set; } = 0.1f;
        public int IntForagingSpotChanceBonus { get; set; } = 1;
        public float IntFishSpotChanceBonus { get; set; } = 0.1f;
        public float IntPanSpotChanceBonus { get; set; } = 0.1f;
        public bool IntRollCraftingChance { get; set; } = true;
        //public bool IntRollObjectQualityBonus { get; set; } = true;

        public float WisCraftResourceReqBonus { get; set; } = 0.1f;
        public float WisCraftTimeBonus { get; set; } = 0.1f;
        public float WisExpBonus { get; set; } = 0.1f;
        public float WisSpotVisibility { get; set; } = 0.1f;
        
        public float ChaFriendshipBonus { get; set; } = 0.1f;
        public float ChaPriceBonus { get; set; } = 0.1f;
        public bool ChaRollRomanceChance { get; set; } = true;


        // death
        public bool PermaDeath { get; set; } = true;
        public int PermaDeathScreenTicks { get; set; } = 60000;
        public float ExperienceLossPercentOnDeath { get; set; } = 100f;

        // tools
        public float ToolLevelReqMult { get; set; } = 2f;
        public float WeaponLevelReqMult { get; set; } = 0.5f;

    }
}
