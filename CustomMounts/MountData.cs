using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;

namespace CustomMounts
{
    public class MountData
    {
        public string Name;
        public string Stable;
        public Point SpawnOffset = new Point(1,1);
        public string FootstepSound;
        public string FootstepSoundWood;
        public string FootstepSoundStone;
        public int Speed = 2;
        public float HorizontalSizeDiff;
        public float VerticalSizeDiff;
        public Point Size = Point.Zero;
        public string TexturePath;
        public int FrameWidth = 32;
        public int FrameHeight = 32;
        public string EatItem = "(O)Carrot";
        public string EatSound = "eat";
        public float EatSpeedBonus = 0.4f;
        public string FluteItem = "(O)911";
        public string FluteSound = "horse_flute";
        public bool AllowHats = true;
        public float HatScale = 1;
        public Vector2[] HatOffsets = {
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero
        };
        public Dictionary<int, HatFrame> HatFrames;
        public Dictionary<int, HatFrame> HatFramesFlipped;
        public Dictionary<string,CustomFrameset> CustomAnimations;
    }

    public class HatFrame
    {
        public Vector2 Offset;
        public float Rotation;
    }

    public class CustomFrameset
    {
        public double Chance;
        public int FacingDirection;
        public CustomFrame[] Frames;
    }
    public class CustomFrame
    {
        public int Frame;
        public bool Flip;
        public int MinLength;
        public int MaxLength;
        public double SoundChance = 1;
        public string Sound;
    }
}