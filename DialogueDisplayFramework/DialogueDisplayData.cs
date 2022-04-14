using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace DialogueDisplayFramework
{
    public class DialogueDisplayData
    {
        public string packName;
        public int xOffset;
        public int yOffset;
        public int width;
        public int height;
        public DialogueData dialogue;
        public PortraitData portrait;
        public NameData name;
        public JewelData jewel;
        public ButtonData button;
        public SpriteData sprite;
        public GiftsData gifts;
        public HeartsData hearts;
        public ImageData[] images = new ImageData[0];
        public TextData[] texts = new TextData[0];
        public DividerData[] dividers = new DividerData[0];
        public bool disabled;
    }
    public class BaseData
    {
        public int xOffset;
        public int yOffset;
        public bool right;
        public bool bottom;
        public int width = -1;
        public int height;
        public float alpha = 1;
        public float scale = 4;
        public float layerDepth = 0.88f;
        public bool variable;
        public bool disabled;
    }
    public class NameData : BaseData
    {
        public int color = -1;
        public string placeholderText;
        public bool centered;
        public int scrollType = 0;
        public bool junimo;
        public bool scroll;
        public SpriteText.ScrollTextAlignment alignment; // left, center, right
    }
    public class DialogueData : BaseData
    {
        public int color = -1;
        public SpriteText.ScrollTextAlignment alignment; // left, center, right
    }
    public class PortraitData : BaseData
    {
        public string texturePath;
        public int x;
        public int y;
        public int w = 64;
        public int h = 64;
        public bool tileSheet = true;
    }
    public class JewelData : BaseData
    {
        public bool animate = true;
    }
    public class ButtonData : BaseData
    {
    }
    public class HeartsData : BaseData
    {
        public int heartsPerRow = 14;
        public bool showEmptyHearts = true;
        public bool centered;
    }
    public class GiftsData : BaseData
    {
        public bool showGiftIcon = true;
        public bool inline = false;
    }
    public class SpriteData : BaseData
    {
        public bool background;
        public int frame;
    }
    public class ImageData : BaseData
    {
        public string texturePath;
        public int x;
        public int y;
        public int w;
        public int h;
    }
    public class TextData : BaseData
    {
        public int color = -1;
        public string text;
        public bool centered;
        public bool junimo;
        public bool scroll;
        public string placeholderText;
        public int scrollType = 0;
        public SpriteText.ScrollTextAlignment alignment; // left, center, right
    }
    public class DividerData : BaseData
    {
        public bool horizontal;
        public bool small;
        public int red = -1;
        public int green = -1;
        public int blue = -1;
    }
}