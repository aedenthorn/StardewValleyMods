namespace FarmSwitch
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool RevealAllTastes { get; set; } = false;
        public bool IncreaseForFirst { get; set; } = true;
        public bool IncreaseForBirthday { get; set; } = true;
        public string ResetPeriod { get; set; } = "Season";
        public int LovedToLiked { get; set; } = 2;
        public int LikedToNeutral { get; set; } = 3;
        public int NeutralToDisliked { get; set; } = -1;
        public int DislikedToHated { get; set; } = 1;
    }
}
