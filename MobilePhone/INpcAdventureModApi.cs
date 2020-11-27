using StardewValley;
using System.Collections.Generic;

namespace MobilePhone
{
    /// <summary>
    /// NPC Adventures mod API
    /// </summary>
    public interface INpcAdventureModApi
    {
        /// <summary>
        /// Checks if farmer is able to recruit any companion
        /// </summary>
        /// <returns></returns>
        bool CanRecruitCompanions();

        /// <summary>
        /// Returns list of companion NPCs
        /// </summary>
        /// <returns></returns>
        IEnumerable<NPC> GetPossibleCompanions();

        /// <summary>
        /// Is this NPC a possible companion?
        /// </summary>
        /// <param name="npcName">NPC name</param>
        /// <returns></returns>
        bool IsPossibleCompanion(string npcName);

        /// <summary>
        /// Is this NPC a possible companion?
        /// </summary>cd
        /// <param name="npc">NPC instance</param>
        /// <returns></returns>
        bool IsPossibleCompanion(NPC npc);

        /// <summary>
        /// Can farmer ask this NPC to follow?
        /// </summary>
        /// <param name="npc">NPC instance</param>
        /// <returns></returns>
        bool CanAskToFollow(NPC npc);

        /// <summary>
        /// Can farmer recruit this NPC?
        /// </summary>
        /// <param name="farmer">Farmer instance</param>
        /// <param name="npc">NPC instance</param>
        /// <returns></returns>
        bool CanRecruit(Farmer farmer, NPC npc);

        /// <summary>
        /// Is this NPC recruited right now?
        /// </summary>
        /// <param name="npc">NPC instance</param>
        /// <returns></returns>
        bool IsRecruited(NPC npc);

        /// <summary>
        /// Is this NPC available for recruit?
        /// </summary>
        /// <param name="npc">NPC instance</param>
        /// <returns></returns>
        bool IsAvailable(NPC npc);

        /// <summary>
        /// Get NPC companion CSM state (as string)
        /// </summary>
        /// <param name="npc">NPC instance</param>
        /// <returns></returns>
        string GetNpcState(NPC npc);

        /// <summary>
        /// Recruit this companion to a farmer
        /// </summary>
        /// <param name="farmer">Farmer instance</param>
        /// <param name="npc">NPC instance</param>
        /// <returns></returns>
        bool RecruitCompanion(Farmer farmer, NPC npc);

        /// <summary>
        /// Load one string from strings dictionary content data asset
        /// </summary>
        /// <param name="path">Path to string in asset with whole asset name (like `Strings/Strings:companionRecruited.yes`</param>
        /// <returns>A loaded string from asset dictionary</returns>
        string LoadString(string path);

        /// <summary>
        /// Load one string from strings dictionary asset with substituions.
        /// Placeholders `{%number%}` in string wil be replaced with substitution.
        /// </summary>
        /// <param name="path">Path to string in asset with whole asset name (like `Strings/Strings:companionRecruited.yes`)</param>
        /// <param name="substitutions">A substitution for replace placeholder in string</param>
        /// <returns>A loaded string from asset dictionary</returns>
        string LoadString(string path, params object[] substitutions);

    }
}