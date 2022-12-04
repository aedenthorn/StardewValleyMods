using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace HelpWanted
{
    public partial class ModEntry
    {
        private static int GetRandomItem(int result, List<int> possibleItems)
        {
            List<int> items = GetRandomItemList(possibleItems);
            if (items is null)
                return result;
            if(items.Contains(result)) 
                return result;
            var item = items[random.Next(items.Count)];
            return item;
        }

        private static List<int> GetRandomItemList(List<int> possibleItems)
        {

            if (!Config.ModEnabled || (!Config.MustLikeItem && !Config.MustLoveItem) || Game1.questOfTheDay is not ItemDeliveryQuest)
                return null;
            string name = (Game1.questOfTheDay as ItemDeliveryQuest)?.target?.Value;
            if (name is null || !Game1.NPCGiftTastes.TryGetValue(name, out string data))
                return null;
            var listString = Game1.NPCGiftTastes["Universal_Love"];
            if (!Config.MustLoveItem)
            {
                listString += " " + Game1.NPCGiftTastes["Universal_Like"];
            }
            var split = data.Split('/');
            if (split.Length < 4)
                return null;
            listString += " " + split[1] + (Config.MustLoveItem ? "" : " " + split[3]);
            if (string.IsNullOrEmpty(listString))
                return null;
            split = listString.Split(' ');
            List<int> items = new List<int>();
            foreach (var str in split)
            {
                if (!int.TryParse(str, out int i))
                    continue;
                items.Add(i);
            }
            if (!items.Any() || (!Config.IgnoreVanillaItemSelection && possibleItems?.Any() != true))
                return null;
            if (Config.IgnoreVanillaItemSelection)
            {
                return items;
            }
            for (int i = possibleItems.Count - 1; i >= 0; i--)
            {
                int idx = possibleItems[i];
                if (!items.Contains(idx))
                {
                    if (idx >= 0)
                    {
                        Object obj = new Object(idx, 1);
                        if (obj is null || !items.Contains(obj.Category))
                        {
                            possibleItems.RemoveAt(i);
                        }
                    }
                }
            }
            if (possibleItems.Any())
            {
                return possibleItems;
            }
            else
            {
                return items;
            }
        }


        public static Color GetRandomColor()
        {
            return new Color((byte)random.Next(Config.RandomColorMin, Config.RandomColorMax), (byte)random.Next(Config.RandomColorMin, Config.RandomColorMax), (byte)random.Next(Config.RandomColorMin, Config.RandomColorMax));
        }

        public static void RefreshQuestOfTheDay(Random r)
        {
            var mine = (MineShaft.lowestLevelReached > 0 && Game1.stats.DaysPlayed > 5U);
            float totalWeight = Config.ResourceCollectionWeight + (mine ? Config.SlayMonstersWeight : 0) + Config.FishingWeight + Config.ItemDeliveryWeight;
            double d = r.NextDouble();
            float currentWeight = Config.ResourceCollectionWeight;
            if (d < currentWeight / totalWeight)
            {
                Game1.questOfTheDay = new ResourceCollectionQuest();
                return;
            }
            if (mine)
            {
                currentWeight += Config.SlayMonstersWeight;
                if (d < currentWeight / totalWeight)
                {
                    Game1.questOfTheDay = new SlayMonsterQuest();
                    return;
                }
            }
            currentWeight += Config.FishingWeight;
            if (d < currentWeight / totalWeight)
            {
                Game1.questOfTheDay = new FishingQuest();
                return;
            }
            Game1.questOfTheDay = new ItemDeliveryQuest();
        }
    }
}