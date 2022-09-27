using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace NPCClothing
{
    public class ClothingData
    {
        public string id;
        public string giftName;
        public string giftReaction;
        public string clothingSlot;
        public string spriteTexturePath;
        public string portraitTexturePath;
        public List<OffsetData> spriteOffsets;
        public List<OffsetData> portraitOffsets;
        public List<string> namesAllow;
        public List<string> namesForbid;
        public List<string> agesAllow;
        public List<string> gendersAllow;
        public List<Color> skinColors;
        public int percentChance = 100;
        public int zIndex;
    }

    public class OffsetData
    {
        public Point offset = new Point(0, 0);
        public List<string> names;
        public List<string> ages;
        public List<string> genders;
    }
}