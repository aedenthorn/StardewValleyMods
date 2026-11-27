using StardewModdingAPI;

namespace PlaygroundMod
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Festivals { get; set; } = false;
        public float swingSpeed { get; set; } = 1f;
        public float springSpeed { get; set; } = 1f;
        public float slideSpeed { get; set; } = 1f;
        public float climbSpeed { get; set; } = 1f;
        public string swingBackSound { get; set; } = "doorCreakReverse";
        public string swingForthSound { get; set; } = "doorCreak";
        public string springSound { get; set; } = "dustMeep";
        public string slideSound { get; set; } = "shwip";
        public string climbSound { get; set; } = "stoneStep";
    }
}
