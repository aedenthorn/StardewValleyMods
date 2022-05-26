using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace CropGrowthInformation
{
    public class TextData
    {
        public Color color;
        public string text;

        public TextData(string _text, Color _color)
        {
            text = _text;
            color = _color;
        }
    }
}