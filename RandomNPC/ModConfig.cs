namespace RandomNPC
{
    public class ModConfig
    {
        public double FemaleChance { get; set; } = 0.5;
        //public double[] ageDist { get; set; } = { 0.1, 0.3, 0.6 };
        public double[] AgeDist { get; set; } = { 0.5, 0.5 }; // teen:adult
        public double DatableChance { get; set; } = 1;
        public double NaturalHairChance { get; set; } = 0.7;
        public double LightSkinChance { get; set; } = 0.7;
        public bool DarkSkinDarkHair { get; set; } = true;
        public int RNPCMaxVisitors { get; set; } = 5;
        public int RNPCTotal { get; set; } = 15;
        public bool RequireHeartsForDialogue { get; set; } = false;
        public int LeaveTime { get; set; } = 1800;
        public bool DestroyObjectsUnderfoot { get; set; } = false;
        public int QuestReward { get; set; } = 200;
    }
}