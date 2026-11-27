using System.Security.Principal;

namespace PetHats
{
    public class HatOffsetData
    {
        public int X;
        public int Y;
        public int flippedX;
        public int flippedY;
        public FrameOffsetData facingUp;
        public FrameOffsetData facingRight;
        public FrameOffsetData facingDown;
        public FrameOffsetData facingLeft;
    }
}