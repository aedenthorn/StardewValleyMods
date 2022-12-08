using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace Spoilage
{
    public partial class ModEntry
    {
        private int GetSpoilAge(Item item)
        {
            if(item.Category == Object.GreensCategory)
            {
                return Config.GreensDays;
            }
            if(item.Category == Object.VegetableCategory)
            {
                return Config.VegetablesDays;
            }
            if(item.Category == Object.FishCategory)
            {
                return Config.FishDays;
            }
            if(item.Category == Object.EggCategory)
            {
                return Config.EggDays;
            }
            if(item.Category == Object.MilkCategory)
            {
                return Config.MilkDays;
            }
            if(item.Category == Object.CookingCategory)
            {
                return Config.CookingDays;
            }
            if(item.Category == Object.FruitsCategory)
            {
                return Config.FruitsDays;
            }
            if(item.Category == Object.flowersCategory)
            {
                return Config.FlowersDays;
            }
            if(item.Category == Object.meatCategory)
            {
                return Config.MeatDays;
            }
            return -1;
        }

        private void CheckItems(NetObjectList<Item> items, float mult)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is not Object || (item as Object).bigCraftable.Value)
                    continue;
                var spoil = GetSpoilAge(item);
                if (spoil < 0)
                    continue;
                float age = 0;
                if (item.modData.TryGetValue(ageKey, out string ageString))
                {
                    float.TryParse(ageString, NumberStyles.Any, CultureInfo.InvariantCulture, out age);
                }
                age += mult;
                if (age >= spoil)
                {
                    if (Config.QualityReduction && (item as Object).Quality > 0)
                    {
                        Monitor.Log($"Reducing quality of {item.Name} x{item.Stack}");
                        (items[i] as Object).Quality--;
                        age = 0;
                    }
                    else if (Config.Rotting && (item as Object).Quality == 0)
                    {
                        item.modData[nameKey] = (string)AccessTools.Method(item.GetType(), "loadDisplayName").Invoke(item, new object[] { });
                        Monitor.Log($"Spoiling {item.Name} x{item.Stack}");
                        (items[i] as Object).Quality = -4;
                        (items[i] as Object).ParentSheetIndex = 168;
                        item.modData[spoiledKey] = "true";
                    }
                }
                item.modData[ageKey] = age + "";
            }
        }
    }
}