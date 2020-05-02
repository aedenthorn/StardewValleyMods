using System.Collections.Generic;

namespace CustomOreNodes
{
    public class CustomOreNode
    {
        public List<DropItem> dropItems = new List<DropItem>();
        public string nodeDesc;
        public string spritePath;
        public string spriteType;
        public int spriteX;
        public int spriteY;
        public int spriteW;
        public int spriteH;
        public int minLevel;
        public int maxLevel;
        public int spawnChance;
        public int durability;
        public int exp;

        public CustomOreNode(string nodeInfo)
        {
            int i = 0;
            string[] infos = nodeInfo.Split('/');
            this.nodeDesc = infos[i++];
            this.spritePath = infos[i++];
            this.spriteType = infos[i++];
            this.spriteX = int.Parse(infos[i].Split(',')[0]);
            this.spriteY = int.Parse(infos[i++].Split(',')[1]);
            this.spriteW = int.Parse(infos[i].Split(',')[0]);
            this.spriteH = int.Parse(infos[i++].Split(',')[1]);
            this.minLevel = int.Parse(infos[i].Split(',')[0]);
            this.maxLevel = int.Parse(infos[i++].Split(',')[1]);
            this.spawnChance = int.Parse(infos[i++]);
            this.durability = int.Parse(infos[i++]);
            this.exp = int.Parse(infos[i++]);
            string[] dropItems = infos[i++].Split('|');
            foreach(string item in dropItems)
            {
                this.dropItems.Add(new DropItem(item));
            }
        }
    }
}