using System.Collections.Generic;

namespace CoinCollector
{
    public class CoinData
    {
        public int index;
        public string name;
        public string setName;
        public string texturePath;
        public List<string> locations;
        public float rarity;
        public int parentSheetIndex;
        public bool isDGA = false;
    }
}