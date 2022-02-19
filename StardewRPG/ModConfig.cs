
namespace StardewRPG
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool PermaDeath { get; set; } = true;
        public int PermaDeathScreenTicks { get; set; } = 1500;
        public bool ManualSkillUpgrades { get; set; } = true;
        public bool NotifyOnLevelUp { get; set; } = true;
        public float ToolLevelReqMult { get; set; } = 2f;
        public float WeaponLevelReqMult { get; set; } = 0.5f;
        public float ExperienceLossPercentOnDeath { get; set; } = 1f;
        public bool OnlyLoseExpToNextLevel { get; set; } = true;
        public float LevelIncrementExpMult { get; set; } = 1f;
        public int BaseHealthPerLevel { get; set; } = 5;
        public int BaseStaminaPerLevel { get; set; } = 10;
        public int StatPointsPerStardrop { get; set; } = 3;
        public int DefaultStatValue { get; set; } = 12;
        public int BaseStatValue { get; set; } = 9;
        public int StartStatExtraPoints { get; set; } = 20;
        public int MinStatValue { get; set; } = 2;
        public int MaxStatValue { get; set; } = 18;
        public string StatBonusLevels { get; set; } = "13,16,18";
        public string StatPenaltyLevels { get; set; } = "8,5,2";
    }
}
