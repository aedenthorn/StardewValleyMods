using HarmonyLib;
using Netcode;
using StardewValley;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace Spoilage
{
    public partial class ModEntry
    {
        private static int GetSpoilAge(Item item)
        {
            int category = item.Category;
            if(spoilageDict.TryGetValue(item.Name, out SpoilData data) || spoilageDict.TryGetValue(item.ParentSheetIndex+"", out data))
            {
                if(data.category != 9999)
                {
                    category = data.category;
                }
                else if(data.age > -1)
                {
                    return data.age;
                }
            }
            if(category == Object.GreensCategory)
            {
                return Config.GreensDays;
            }
            if(category == Object.VegetableCategory)
            {
                return Config.VegetablesDays;
            }
            if(category == Object.FishCategory)
            {
                return Config.FishDays;
            }
            if(category == Object.EggCategory)
            {
                return Config.EggDays;
            }
            if(category == Object.MilkCategory)
            {
                return Config.MilkDays;
            }
            if(category == Object.CookingCategory)
            {
                return Config.CookingDays;
            }
            if(category == Object.FruitsCategory)
            {
                return Config.FruitsDays;
            }
            if(category == Object.flowersCategory)
            {
                return Config.FlowersDays;
            }
            if(category == Object.meatCategory)
            {
                return Config.MeatDays;
            }
            return 0;
        }

        public static void SpoilItems(IList<Item> items, float mult)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is not Object || (item as Object).bigCraftable.Value)
                    continue;
                var spoil = GetSpoilAge(item);
                if (spoil < 1)
                    continue;
                float age = 0;
                if (item.modData.TryGetValue(ageKey, out string ageString))
                {
                    float.TryParse(ageString, NumberStyles.Any, CultureInfo.InvariantCulture, out age);
                }
                age += mult;
                if (age >= spoil && !item.modData.ContainsKey(spoiledKey))
                {
                    if (Config.QualityReduction && (item as Object).Quality > 0)
                    {
                        SMonitor.Log($"Reducing quality of {item.Name} x{item.Stack}");
                        (items[i] as Object).Quality--;
                        age = 0;
                    }
                    else if (Config.Spoiling && (item as Object).Quality == 0)
                    {
                        SMonitor.Log($"Spoiling {item.Name} x{item.Stack}");
                        if(Config.RemoveSpoiled)
                        {
                            items[i] = null;
                            continue;
                        }
                        int spoiledIndex = Config.SpoiledIndex;
                        if ((spoilageDict.TryGetValue(item.Name, out SpoilData data) || spoilageDict.TryGetValue(item.ParentSheetIndex + "", out data)) && data.spoiled is not null)
                        {
                            if(int.TryParse(data.spoiled, out int index))
                                spoiledIndex = index;
                            else
                            {
                                try
                                {
                                    spoiledIndex = Game1.objectInformation.First(k => k.Value.StartsWith(data.spoiled + "/")).Key;
                                }
                                catch { }
                            }
                        }
                        items[i] = new Object(spoiledIndex, items[i].Stack);
                        (items[i] as Object).Quality = -4;
                        items[i].modData[spoiledKey] = (string)AccessTools.Method(item.GetType(), "loadDisplayName").Invoke(item, new object[] { });
                    }
                }
                item.modData[ageKey] = age + "";
            }
        }
    }
}