using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CustomOreNodes
{
    public class CustomOreNode
    {
        public string id;
        public int parentSheetIndex;
        public List<DropItem> dropItems = new List<DropItem>();
        public List<OreLevelRange> oreLevelRanges = new List<OreLevelRange>();
        public string nodeDesc;
        public string spritePath;
        public string spriteType = "game";
        public int spriteX;
        public int spriteY;
        public int spriteW;
        public int spriteH;
        public float spawnChance;
        public int durability;
        public int exp;
        public Texture2D texture;

        public CustomOreNode()
        {

        }

        public CustomOreNode(string nodeInfo)
        {
            int i = 0;
            string[] infos = nodeInfo.Split('/');
            if (infos.Length != 10)
            {
                ModEntry.context.Monitor.Log($"improper syntax in ore node string: number of elements is {infos.Length} but should be 10", StardewModdingAPI.LogLevel.Error);
                throw new System.ArgumentException();
            }
            nodeDesc = infos[i++];
            spritePath = infos[i++];
            spriteType = infos[i++];
            spriteX = int.Parse(infos[i].Split(',')[0]);
            spriteY = int.Parse(infos[i++].Split(',')[1]);
            spriteW = int.Parse(infos[i].Split(',')[0]);
            spriteH = int.Parse(infos[i++].Split(',')[1]);
            string[] levelRanges = infos[i++].Split('|');
            foreach (string levelRange in levelRanges)
            {
                oreLevelRanges.Add(new OreLevelRange(levelRange));
            }
            spawnChance = float.Parse(infos[i++]);
            durability = int.Parse(infos[i++]);
            exp = int.Parse(infos[i++]);
            string[] drops = infos[i++].Split('|');
            foreach (string item in drops)
            {
                dropItems.Add(new DropItem(item));
            }
        }
    }

    public class OreLevelRange
    {
        public int minLevel = -1;
        public int maxLevel = -1;
        public float spawnChanceMult = 1f;
        public float expMult = 1f;
        public float dropChanceMult = 1f;
        public float dropMult = 1f;
        public int minDifficulty = -1;
        public int maxDifficulty = -1;

        public OreLevelRange()
        {

        }

        public OreLevelRange(string infos)
        {
            string[] infoa = infos.Split(',');
            if (infoa.Length < 2)
            {
                ModEntry.context.Monitor.Log($"improper syntax in ore node level range string: number of elements is {infoa.Length} but should be at least 2", StardewModdingAPI.LogLevel.Error);
                throw new System.ArgumentException();
            }
            minLevel = int.Parse(infoa[0]);
            maxLevel = int.Parse(infoa[1]);
            if (infoa.Length > 2)
                spawnChanceMult = float.Parse(infoa[2]);
            if (infoa.Length > 3)
                expMult = float.Parse(infoa[3]);
            if (infoa.Length > 4)
                dropChanceMult = float.Parse(infoa[4]);
            if (infoa.Length > 5)
                dropMult = float.Parse(infoa[5]);
        }
    }
}