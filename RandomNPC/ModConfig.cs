namespace RandomNPC
{
    public class ModConfig
    {
        public double FemaleChance { get; set; } = 1.0;
        public double[] ageDist { get; set; } = { 0.1, 0.3, 0.6 };
        public double DatableChance { get; set; } = 1;
        public double NaturalHairChance { get; set; } = 0.7;
        public double LightSkinChance { get; set; } = 0.7;
        public int RNPCMax { get; set; } = 5;
    }
}