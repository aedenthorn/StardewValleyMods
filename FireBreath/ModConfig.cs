using StardewModdingAPI;

namespace FireBreath
{
    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public SButton FireButton { get; set; } = SButton.Insert;
        public bool ScaleWithSkill { get; set; } = true;
        public int FireDamage { get; set; } = 100;
        public int FireDistance { get; set; } = 256;
        public string FireSound { get; set; } = "furnace";
    }
}
