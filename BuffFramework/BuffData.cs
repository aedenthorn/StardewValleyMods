using Microsoft.Xna.Framework;
using StardewValley;

namespace BuffFramework
{
    public class BuffData
    {
        public int which = -1;
        public int sheetIndex = -1;
        public Color? glow;

        public int farming = -1;
        public int fishing = -1;
        public int mining = -1;
        public int digging = -1;
        public int luck = -1;
        public int foraging = -1;
        public int crafting = -1;
        public int maxStamina = -1;
        public int magneticRadius = -1;
        public int speed = -1;
        public int defense = -1;
        public int attack = -1;

        public string source;
        public string displaySource;
        
        public int buffId;
        public float glowRate = 0.05f;
        public string sound;
    }
}