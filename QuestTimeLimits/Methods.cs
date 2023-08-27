using System;

namespace QuestTimeLimits
{
    public partial class ModEntry
    {
        private static int MultiplyQuestDays(int days)
        {
            if(!Config.ModEnabled || Config.DailyQuestMult <= 0)
                return days;
            return (int)Math.Round(days * Config.DailyQuestMult);
        }

    }
}