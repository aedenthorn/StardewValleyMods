using HarmonyLib;
using StardewValley;
using System.Collections.Generic;

namespace SeedInfo
{
    public class SeedEntryInfo
    {
        public List<QualityInfo> info = new();
        public int needFertilizer;

        public SeedEntryInfo(Object seed)
        {
            needFertilizer = ModEntry.NeedFertilizer(seed);
            foreach (int q in ModEntry.qualities)
            {
                var i = new QualityInfo(seed, q);
                info.Add(i);
            }
        }
    }

    public class QualityInfo
    {
        public int quality;
        public ObjectInfo crop;
        public ObjectInfo pickle;
        public ObjectInfo keg;
        public QualityInfo(Object seed, int q)
        {
            quality = q;
            crop = ModEntry.GetCrop(seed, q);
            if (crop == null)
                return;
            pickle = ModEntry.GetPickle(crop.obj, q);
            keg = ModEntry.GetKeg(crop.obj, q);
        }
    }

    public class ObjectInfo
    {
        public ObjectInfo(Object obj)
        {
            this.obj = obj;
            name = obj.DisplayName;
            desc = obj.getDescription();
            price = obj.sellToStorePrice();
        }
        public Object obj;
        public string name;
        public string desc;
        public int price;
    }
}