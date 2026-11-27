using System.Collections.Generic;

namespace LogSpamFilter
{
    public class DropData
    {
        public List<ItemData> stumpWood = new List<ItemData>();
        public List<ItemData> stumpSap = new List<ItemData>();
        public List<ItemData> stumpHardwood = new List<ItemData>();
        public List<ItemData> stumpItems = new List<ItemData>();
        public List<ItemData> wood = new List<ItemData>();
        public List<ItemData> sap = new List<ItemData>();
        public List<ItemData> hardwood = new List<ItemData>();
        public List<ItemData> items = new List<ItemData>();
    }

    public class ItemData
    {
        public string id;
        public float min = 1;
        public float max = 1;
        public int minQuality;
        public int maxQuality;
        public float mult = 1;
        public float chance = 100;
    }
}