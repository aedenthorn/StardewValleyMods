//using Harmony;
using Microsoft.Xna.Framework.Graphics;
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
        public float rarity;
        public int parentSheetIndex;
        public bool isDGA = false;
    }
}