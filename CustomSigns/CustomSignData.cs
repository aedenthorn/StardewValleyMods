using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CustomSigns
{
    public class CustomSignData
    {
        public string texturePath;
        public string packID;
        public int tileWidth;
        public int tileHeight;
        public int heldObjectX;
        public int heldObjectY;
        public float scale = 4;
        public string[] types;
        public SignText[] text;
        public Texture2D texture;
    }

    public class SignText
    {
        public string text;
        public int X;
        public int Y;
        public bool center = true;
        public string fontPath;
        public float scale;
        public Color color = Color.White;
    }
}