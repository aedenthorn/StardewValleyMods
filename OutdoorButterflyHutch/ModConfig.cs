namespace OutdoorButterflyHutch
{
    class ModConfig
    {
        public int SpriteSheetOffsetRows { get; set; } = 0;
        public object HutchCost { get; set; } = "338 40 709 40 425 4";
        public object SkillReq { get; set; } = "Farming 6";
        public float MinButterfliesDensity { get; set; } = 0.01f;
        public float MaxButterfliesDensity  { get; set; } = 0.05f;
    }
}
