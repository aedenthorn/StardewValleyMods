using Microsoft.Xna.Framework;

namespace RainbowTrail
{
    public class PositionInfo
    {
        public Vector2 position;
        public int direction;

        public PositionInfo(Vector2 pos, int d)
        {
            position = pos;
            direction = d;
        }
    }
}