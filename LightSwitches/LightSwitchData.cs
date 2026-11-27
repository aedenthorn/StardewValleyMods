namespace LightSwitches
{
    public class LightSwitchData
    {
        public string OffTexture { get; set; }
        public string OnTexture { get; set; }
        public bool Prismatic { get; set; }
        public string PrismaticStart { get; set; }
        public string PrismaticEnd { get; set; }
        public int PrismaticSpeed { get; set; } = 1;
        public int StrobeSpeed { get; set; } = -1;
    }
}