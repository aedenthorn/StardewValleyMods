namespace GemIsles
{
    public class ModConfig
    {

        public bool EnableMod { get; set; } = true;
        public int MaxIsles { get; set; } = 5;
        public float TreesChance { get; set; } = 0.7f;
        public float TreesPortion { get; set; } = 0.05f;
        public float GrassChance { get; set; } = 0.6f;
        public float GrassPortion { get; set; } = 0.2f;
        public float MineralChance { get; set; } = 0.6f;
        public float MineralPortion { get; set; } = 0.05f;
        public float TreasureChance { get; set; } = 0.1f;
        public float CoconutChance { get; set; } = 0.7f;
        public float CoconutPortion { get; set; } = 0.005f;
        public float FaunaChance { get; set; } = 0.7f;
        public float FaunaPortion { get; set; } = 0.005f;
        public float ArtifactChance { get; set; } = 0.7f;
        public float ArtifactPortion { get; set; } = 0.005f;
        public float MonsterChance { get; set; } = 0.2f;
        public float MonsterPortion { get; set; } = 0.02f;
    }
}
