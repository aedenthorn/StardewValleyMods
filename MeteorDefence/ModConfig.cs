namespace MeteorDefence
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool StrikeAnywhere { get; set; } = false;
        public int MinMeteorites { get; set; } = 3;
        public int MaxMeteorites { get; set; } = 10;
        public string DefenceObject { get; set; } = "9";
        public string DefenceSound { get; set; } = "debuffSpell";
        public string ExplodeSound { get; set; } = "explosion";

    }
}
