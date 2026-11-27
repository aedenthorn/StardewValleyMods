
namespace FishNibblingIndicator
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int SourceX { get; set; } = 403;
        public int SourceY { get; set; } = 496;
        public int SourceW { get; set; } = 5;
        public int SourceH { get; set; } = 14;
        public float Scale { get; set; } = 5;
        public float AlphaFade { get; set; } = 0.01f;
        public float ScaleChange { get; set; } = 0.005f;
        public float MotionX { get; set; } = 0;
        public float MotionY { get; set; } = -0.25f;
        public int OffsetX { get; set; } = -5;
        public int OffsetY { get; set; } = -30;
        public string SourceTexture { get; set; } = "LooseSprites\\Cursors";
    }
}
