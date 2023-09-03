namespace BossCreatures
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int PercentChanceOfBossInMonsterArea { get; set; } = 100;
        public int PercentChanceOfBossInFarm { get; set; } = 0;
        public int PercentChanceOfBossInTown { get; set; } = 0;
        public int PercentChanceOfBossInForest { get; set; } = 0;
        public int PercentChanceOfBossInMountain { get; set; } = 0;
        public int PercentChanceOfBossInDesert { get; set; } = 10;
        public int PercentChanceOfBossInCrimsonBadlands { get; set; } = 20;
        public float BaseUndergroundDifficulty { get; set; } = 1f;
        public float MinOverlandDifficulty { get; set; } = 0.5f;
        public float MaxOverlandDifficulty { get; set; } = 1.0f;
        public float WeightBugBossChance { get; set; } = 1f;
        public float WeightGhostBossChance { get; set; } = 1f;
        public float WeightSerpentBossChance { get; set; } = 1f;
        public float WeightSkeletonBossChance { get; set; } = 1f;
        public float WeightSkullBossChance { get; set; } = 1f;
        public float WeightSquidBossChance { get; set; } = 1f;
        public float WeightSlimeBossChance { get; set; } = 1f;
        public bool UseAlternateTextures { get; set; } = false;
        public float BugBossScale { get; set; } = 3.0f;
        public int BugBossHeight { get; set; } = 16;
        public int BugBossWidth { get; set; } = 16;
        public float GhostBossScale { get; set; } = 3.0f;
        public int GhostBossHeight { get; set; } = 24;
        public int GhostBossWidth { get; set; } = 16;
        public float SerpentBossScale { get; set; } = 2.0f;
        public int SerpentBossHeight { get; set; } = 32;
        public int SerpentBossWidth { get; set; } = 32;
        public float SkeletonBossScale { get; set; } = 3.0f;
        public int SkeletonBossHeight { get; set; } = 32;
        public int SkeletonBossWidth { get; set; } = 16;
        public float SkullBossScale { get; set; } = 3.0f;
        public int SkullBossHeight { get; set; } = 16;
        public int SkullBossWidth { get; set; } = 16;
        public float SquidKidBossScale { get; set; } = 4.0f;
        public int SquidKidBossHeight { get; set; } = 16;
        public int SquidKidBossWidth { get; set; } = 16;
        public float SlimeBossScale { get; set; } = 2.0f;
        public int SlimeBossHeight { get; set; } = 32;
        public int SlimeBossWidth { get; set; } = 32;
    }
}