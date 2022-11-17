using Microsoft.Xna.Framework;

namespace CustomMineCartSteam
{
    public class SteamData
    {
        public string location;
        public bool replaceSteam;
        public int animationRow = 27;
        public Point position;
        public int animationLength = 8;
        public bool flipped = true;
        public Color color = Color.White;
        public float animationInterval = 60;
        public int animationLoops = 999999;
        public int sourceRectWidth = -1;
        public float layerDepth = 1;
        public int sourceRectHeight = -1;
        public int delay = 0;

        public string texturePath;
        public Rectangle sourceRect;
        public bool flicker;
        public float alphaFade = 0;
        public float scale = 1;
        public float scaleChange = 0;
        public float rotation = 0;
        public float rotationChange = 0;
        public bool local;
        public bool drawAboveAlwaysFront;
        public XY motion;
        public XY acceleration;

        public int id;
    }

    public class XY
    {
        public float X;
        public float Y;
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }
}