namespace MeteorDefence
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool StrikeAnywhere { get; set; } = false;
        public int MinMeteorites { get; set; } = 1;
        public int MaxMeteorites { get; set; } = 1;
        public string DefenceObject { get; set; } = "Space Laser";
        public int MeteorsPerObject { get; set; } = 1;
        public string DefenceSound { get; set; } = "debuffSpell";
        public string ExplodeSound { get; set; } = "explosion";

    }
}
