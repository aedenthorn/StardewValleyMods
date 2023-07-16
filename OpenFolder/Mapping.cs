using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace OpenFolder
{
    public class Mapping
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public int xo;
        public int yo;
        public int xa;
    }
    public class SpriteFontMapping
    {
        public int LineSpacing;
        public float Spacing = -1;
        public char DefaultCharacter;
        public List<char> Characters = new();
        public Dictionary<char, SpriteFont.Glyph> Glyphs = new();
    }
}