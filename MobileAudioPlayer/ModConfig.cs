using Microsoft.Xna.Framework;

namespace MapTeleport
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string ListLineOne { get; set; } = "{1}";
        public string ListLineTwo { get; set; } = "{2} - {3}";
        public Color BackgroundColor { get; set; } = new Color(25, 60, 107);
        public Color LineOneColor { get; set; } = Color.White;
        public Color LineTwoColor { get; set; } = Color.LightGray;
        public Color HighlightColor { get; set; } = new Color(90, 139, 227, 255);
        public Color ButtonBarColor { get; set; } = Color.DarkGray;
        public Color VolumeBarColor { get; set; } = Color.Lime;
        public float MarginX { get; set; } = 4;
        public int MarginY { get; set; } = 4;
        public int VolumeBarWidth { get; set; } = 8;
        public bool PlayAll { get; set; } = true;
        public bool LoopPlaylist { get; set; } = true;
        public float LineOneScale { get; set; } = 0.5f;
        public float LineTwoScale { get; set; } = 0.4f;
        public bool MuteGameMusicWhilePlaying { get; set; } = true;
        public bool MuteAmbientSoundWhilePlaying { get; set; } = true;
        public int VolumeLevel { get; set; } = 100;
    }
}
