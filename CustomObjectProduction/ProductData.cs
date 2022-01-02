using System.Collections.Generic;

namespace CustomObjectProduction
{
    public class ProductData
    {
        public string id;
        public List<ProductInfo> infoList = new List<ProductInfo>();
        public int amount;
        public int quality;
    }

    public class ProductInfo
    {
        public string id;
        public int min;
        public int max;
        public int minQuality;
        public int maxQuality;
        public int weight;
    }
}