using Microsoft.Xna.Framework;

namespace FishTankCapacity
{
    public class DecorationInfo
    {
        public string Texture;
        public Rectangle? SourceRect;
        public float Scale = 4f;
        public int Frames = 1;
        public int FrameDuration = 30;
        public Vector2 DrawOffset = Vector2.Zero;
    }
}