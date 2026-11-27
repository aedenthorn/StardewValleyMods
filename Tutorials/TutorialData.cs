using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Tutorials
{
    public class TutorialData
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public List<TutorialFrame> Frames { get; set; } = new();
    }

    public class TutorialFrame
    {
        public string Texture { get; set; }
        public int Frames { get; set; } = 1;
        public int FrameRate { get; set; }
        public Rectangle? StartRect { get; set; }
        public string Text { get; set; }
    }

    public class TutorialTrigger
    {
        public string Tutorial { get; set; }
        public string Category { get; set; }
        public List<string> Categories { get; set; }

    }
    public class TutorialAddedFrames
    {
        public string Tutorial { get; }
        public int Position { get; } = -1;
        public List<TutorialFrame> Frames { get; }
    }

}