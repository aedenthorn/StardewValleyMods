using Microsoft.Xna.Framework;

namespace CustomLightSource
{
    public class LightData
    {
        public Color color;
        public int textureIndex;
        public string texturePath;
        public float radius;
        public XY offset;
        public bool isLamp;
    }

    public class XY
    {
        public float X;
        public float Y;
    }
}