using StardewValley;
using System.Collections.Generic;

namespace MobilePhone
{
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
        string GetFriendSpecificDialogueText(Farmer farmer, NPC npc, string key);
        string LoadString(string path);
        string LoadString(string path, string substitution);
        string LoadString(string path, string[] substitutions);
    }
}