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
        private static List<int> GetPossibleCrops(List<int> oldList)
        {
            if (!Config.ModEnabled)
                return oldList;
            List<int> newList = GetRandomItemList(oldList);
            SMonitor.Log($"possible crops: {newList?.Count}");
            return (newList is null || !newList.Any()) ? oldList : newList;
        }
        private static int GetRandomItem(int result, List<int> possibleItems)
        {
            List<int> items = GetRandomItemList(possibleItems);

            if (items is null)
                return result;
            if(items.Contains(result) && !Config.IgnoreVanillaItemSelection) 
                return result;
            for(int i = items.Count - 1; i >= 0; i--)
            {
                if (!Game1.objectInformation.ContainsKey(items[i]))
                    items.RemoveAt(i);
            }
            if (!items.Any())
                return result;
            var ii = items[random.Next(items.Count)];
            if (!Game1.objectInformation.ContainsKey(ii))
                return result;
            SMonitor.Log($"our random: {ii}");
            //SMonitor.Log($"found our random: {ii}");
            return ii;
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
                Object obj = new Object(i, 1);
                if (!Config.AllowArtisanGoods && obj is not null && obj.Category == Object.artisanGoodsCategory)
                    continue;
                if (Config.MaxPrice > 0 && obj is not null && obj.Price > Config.MaxPrice)
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
                        if (obj is null || !items.Contains(obj.Category) || (!Config.AllowArtisanGoods && obj.Category == Object.artisanGoodsCategory) || (Config.MaxPrice > 0 && obj.Price > Config.MaxPrice))
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

        private Quest MakeQuest(QuestInfo quest)
        {
            switch (quest.questType)
            {
                case QuestType.ResourceCollection:
                    var q = new ResourceCollectionQuest();
                    q.target.Value = quest.target;
                    q.questTitle = quest.questTitle;
                    q.questDescription = quest.questDescription;
                    q.resource.Value = GetIndexFromString(quest.item);
                    q.number.Value = quest.number;
                    q.targetMessage.Value = quest.targetMessage;
                    q.currentObjective = quest.currentObjective;
                    return q;
                case QuestType.Fishing:
                    var q2 = new FishingQuest();
                    q2.target.Value = quest.target;
                    q2.questTitle = quest.questTitle;
                    q2.questDescription = quest.questDescription;
                    q2.whichFish.Value = GetIndexFromString(quest.item);
                    q2.numberToFish.Value = quest.number;
                    q2.targetMessage = quest.targetMessage;
                    q2.currentObjective = quest.currentObjective;
                    return q2;
                case QuestType.ItemDelivery:
                    var q3 = new ItemDeliveryQuest();
                    q3.target.Value = quest.target;
                    q3.questTitle = quest.questTitle;
                    q3.item.Value = GetIndexFromString(quest.item);
                    q3.targetMessage = quest.targetMessage;
                    q3.currentObjective = quest.currentObjective;
                    q3.questDescription = quest.questDescription;
                    return q3;
                case QuestType.SlayMonster:
                    var q4 = new SlayMonsterQuest();
                    q4.target.Value = quest.target;
                    q4.questTitle = quest.questTitle;
                    q4.questDescription = quest.questDescription;
                    q4.monsterName.Value = quest.item;
                    q4.numberToKill.Value = quest.number;
                    q4.targetMessage = quest.targetMessage;
                    q4.currentObjective = quest.currentObjective;
                    return q4;
                default:
                    return null;
            }
        }

        private int GetIndexFromString(string item)
        {
            if (int.TryParse(item, out int idx))
                return idx;
            return Game1.objectInformation.First(o => o.Value.StartsWith(item + "/")).Key;
        }

    }
}