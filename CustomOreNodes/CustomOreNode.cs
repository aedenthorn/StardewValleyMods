using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CustomOreNodes
{
    public interface ICustomOreNode
    {
        public string itemId { get; set; }
        public List<DropItem> dropItems { get; set; }
        public List<OreLevelRange> oreLevelRanges { get; set; }
        public float spawnChance { get; set; }
        public int durability { get; set; }
        public int exp { get; set; }

    }
    public class CustomOreNode : ICustomOreNode
    {
        public string itemId { get; set; }
        public List<DropItem> dropItems { get; set; } = new List<DropItem>();
        public List<OreLevelRange> oreLevelRanges { get; set; } = new List<OreLevelRange>();
        public float spawnChance { get; set; }
        public int durability { get; set; }
        public int exp { get; set; }

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