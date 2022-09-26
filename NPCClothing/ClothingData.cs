using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace NPCClothing
{
    public class ClothingData
    {
        public string giftName;
        public string spriteTexturePath;
        public string portraitTexturePath;
        public List<string> nameRestrictions;
        public List<string> ageRestrictions;
        public List<string> genderRestrictions;
        public List<Color> skinColors;
        public int percentChance = 100;
    }
}