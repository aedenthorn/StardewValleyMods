using System.Collections.Generic;

namespace CoinCollector
{
    public class CoinDataDict
    {
        public List<CoinData> data = new List<CoinData>();
    }

    public class CoinData
    {
        public string id;
        public string setName;
        public List<string> locations;
        public float rarity;
        public int parentSheetIndex;
        public bool isDGA = false;
    }
}