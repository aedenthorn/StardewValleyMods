using StardewValley;
using StardewValley.Quests;
using System.Collections.Generic;
using System.Linq;

namespace HelpWanted
{
    public class HelpWantedAPI : IHelpWantedAPI
    {
        public void AddQuestTomorrow(IQuestData data)
        {
            ModEntry.SMonitor.Log($"Adding mod quest data {data.quest.GetType()}");
            ModEntry.modQuestList.Add(data);
        }
        public void AddQuestToday(IQuestData data)
        {
            ModEntry.SMonitor.Log($"Adding quest data {data.quest.GetType()}");
            QuestType questType = QuestType.ItemDelivery;
            if (data.quest is ResourceCollectionQuest)
            {
                questType = QuestType.ResourceCollection;
            }
            else if (data.quest is SlayMonsterQuest)
            {
                questType = QuestType.SlayMonster;
            }
            else if (data.quest is FishingQuest)
            {
                questType = QuestType.Fishing;
            }
            if(ModEntry.questList.Count == 0)
            {
                ModEntry.questList = OrdersBillboard.questDict.Values.ToList();
            }
            ModEntry.AddQuest(data.quest, questType, data.icon, data.iconSource, data.iconOffset);
        }
        public IList<IQuestData> GetQuests()
        {
            ModEntry.SMonitor.Log($"Getting quest list");
            return ModEntry.modQuestList;
        }
    }

    public interface IHelpWantedAPI
    {
        public void AddQuestTomorrow(IQuestData data);
        public void AddQuestToday(IQuestData data);
        public IList<IQuestData> GetQuests();
    }
}