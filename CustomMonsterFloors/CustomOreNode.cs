using System.Collections.Generic;

namespace CustomMonsterFloors
{
    public class CustomOreNode
    {
        public Dictionary<int, double> dropItems = new Dictionary<int, double>();
        public string spriteName;
        public int spawnChance;

        public CustomOreNode(string nodeInfo)
        {
            string[] infos = nodeInfo.Split('/');
            this.spriteName = infos[0];
            this.spawnChance = int.Parse(infos[1]);
            string[] dropItems = infos[2].Split(';');
            foreach(string item in dropItems)
            {
                string[] itema = item.Split(',');
                this.dropItems.Add(int.Parse(itema[0]), double.Parse(itema[1]));
            }
        }
    }
}