using System.Collections.Generic;

namespace FarmerCommissions
{
    public interface IHelpWantedAPI
    {
        public void AddQuestToday(IQuestData data);
        public void AddQuestTomorrow(IQuestData data);
        public IList<IQuestData> GetQuests();
    }
}