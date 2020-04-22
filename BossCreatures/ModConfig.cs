namespace BossCreatures
{
    public class ModConfig
    {
        public int PercentChanceOfBossInMonsterArea { get; set; } = 100;
        public int PercentChanceOfBossInTown { get; set; } = 5;
        public int PercentChanceOfBossInForest { get; set; } = 20;
        public int PercentChanceOfBossInMountain { get; set; } = 20;
        public int PercentChanceOfBossInDesert { get; set; } = 20;
        public float BaseUndergroundDifficulty { get; set; } = 1f;
        public float MinOverlandDifficulty { get; set; } = 0.75f;
        public float MaxOverlandDifficulty { get; set; } = 1.25f;
        public int PercentChanceOfBossInFarm { get; set; } = 10;
    }
}