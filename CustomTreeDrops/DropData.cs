using System.Collections.Generic;

namespace CustomTreeDrops
{
    public class DropData
    {
        public List<ItemData> items = new List<ItemData>();
    }

    public class ItemData
    {
        public string id;
        public int amount;
    }
}