using StardewValley;

namespace MobilePhone.Api
{
    /// <summary>
    /// !Updated Code. <br />
    /// One big change: API functionality updated to latest stability ('Blackhole') version. <br />
    /// All unused functions has been removed, to increase compatibility with next versions.
    /// </summary>
    public interface INpcAdventureModApi
    {
        bool CanRecruitCompanions();
        bool IsPossibleCompanion(NPC npc);
        bool CanRecruit(Farmer farmer, NPC npc);
        bool IsRecruited(NPC npc);
        bool RecruitCompanion(Farmer farmer, NPC npc);
    }
}