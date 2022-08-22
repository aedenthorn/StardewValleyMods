using StardewValley;
using System.Collections.Generic;

namespace MobilePhone
{
    /// <summary>
    /// One big change: API functionality updated to latest stability ('Blackhole') version.
    /// </summary>
    public interface INpcAdventureModApi
    {
        bool CanRecruitCompanions();
        IEnumerable<NPC> GetPossibleCompanions();
        bool IsPossibleCompanion(string npc);
        bool IsPossibleCompanion(NPC npc);
        bool CanAskToFollow(NPC npc);
        bool CanRecruit(Farmer farmer, NPC npc);
        bool IsRecruited(NPC npc);
        bool IsAvailable(NPC npc);
        string GetNpcState(NPC npc);
        bool RecruitCompanion(Farmer farmer, NPC npc);
    }
}