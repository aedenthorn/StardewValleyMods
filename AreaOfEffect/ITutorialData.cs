using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public class TutorialData
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public List<TutorialFrame> Frames { get; set; } = new();
    }

    public class TutorialFrame
    {
        public string Subtitle { get; set; }
        public string Texture { get; set; }
        public int Frames { get; set; } = 1;
        public int FrameRate { get; set; }
        public float Scale { get; set; } = 4;
        public Rectangle? StartRect { get; set; }
        public string Text { get; set; }
    }

}