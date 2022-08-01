using Microsoft.Xna.Framework;

namespace PrismaticFire
{
    public class ColorData
    {
        public Color startColor = Color.Transparent;
        public Color endColor = Color.Transparent;
        public float speed = -1;

        public ColorData()
        {

        }
        public ColorData(Color start, Color end, float speed = 1)
        {
            startColor = start;
            endColor = end;
            this.speed = speed;
        }
        public ColorData(Color start)
        {
            startColor = start;
        }
    }
}