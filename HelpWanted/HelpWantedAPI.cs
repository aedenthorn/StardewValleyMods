namespace HelpWanted
{
    public class HelpWantedAPI : IHelpWantedAPI
    {
        public void AddQuestToday(IQuestData data)
        {
            ModEntry.SMonitor.Log($"Adding mod quest data {data.quest.GetType()}");
            ModEntry.modQuestList.Add(data);
        }
    }

    public interface IHelpWantedAPI
    {
        public void AddQuestToday(IQuestData data);
    }
}