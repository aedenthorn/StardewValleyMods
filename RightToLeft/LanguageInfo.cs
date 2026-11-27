using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;

namespace RightToLeft
{
    public class LanguageInfo
    {
        public string name;
        public string code;
        public char defaultCharacter;
        public int xOffset;
        public bool useXAdvance;
        public int dialogueFontLineSpacing = 50;
        public int smallFontLineSpacing = 33;
        public int tinyFontLineSpacing = 25;
        public float dialogueFontSpacing = -2;
        public float smallFontSpacing = -1;
        public float tinyFontSpacing = 1;
        public ModLanguage metaData;

        public SpriteFont dialogueFont;
        public SpriteFont smallFont;
        public SpriteFont tinyFont;
        public string path;

    }
}