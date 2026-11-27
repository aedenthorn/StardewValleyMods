using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CustomResourceClumps
{
    public class CustomResourceClump
    {
        public List<DropItem> dropItems = new List<DropItem>();
        public string id;
        public string clumpDesc;
        public int debrisType;
        public string[] hitSounds;
        public string[] breakSounds;
        public string[] failSounds;
        public int shake;
        public string toolType;
        public int toolMinLevel;
        public int tileWidth;
        public int tileHeight;
        public string spritePath;
        public string spriteType;
        public int spriteX;
        public int spriteY;
        public int minLevel = -1;
        public int maxLevel = -1;
        public float baseSpawnChance;
        public float additionalChancePerLevel;
        public int durability;
        public string expType;
        public int exp;
        public Texture2D texture;
        public int index;
    }
}