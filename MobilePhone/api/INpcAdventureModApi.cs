using StardewValley;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MobilePhone.Api
{ 
    public interface INpcAdventureModApi
    {
        /// <summary>
        /// Checks if the companion can be recruited 
        /// and (maybe) get the acceptal or rejectal dialogue
        /// </summary>
        /// <param name="farmer">Farmer who checks if they can recruit a companion</param>
        /// <param name="npc">Companion NPC to check</param>
        /// <param name="dialogueKey">
        /// A dialogue key string when npc is a valid companion, 
        /// it's unlocked for farmer and is available to recruit. Otherwise is null
        /// </param>
        /// <param name="cooldown">
        /// Cooldown in seconds during which player can't ask again 
        /// for an adventure if it was rejected
        /// </param>
        /// <returns>
        /// False when this npc is not a companion, is locked for farmer, 
        /// is unavailable (recruited, festival day ...) 
        /// or doesn't accept adventure today
        /// </returns>
        bool CanRecruit(Farmer farmer, NPC npc, [MaybeNullWhen(false)] out string dialogueKey, out int cooldown);

        /// <summary>
        /// Checks if the companion can be recruited 
        /// </summary>
        /// <param name="farmer">Farmer who checks if they can recruit a companion</param>
        /// <param name="npc">Companion NPC to check</param>
        /// <returns>
        /// False when this npc is not a companion, is locked for farmer, 
        /// is unavailable (recruited, festival day ...) 
        /// or doesn't accept adventure today
        /// </returns>
        bool CanRecruit(Farmer farmer, NPC npc);

        /// <summary>
        /// Get NPC instance of a valid companion
        /// </summary>
        /// <param name="name">Name of possibly companion NPC</param>
        /// <returns></returns>
        NPC GetCompanionNpc(string name);

        /// <summary>
        /// Get all valid companions
        /// </summary>
        /// <returns></returns>
        IEnumerable<NPC> GetCompanions();

        /// <summary>
        /// Check if this NPC is available to be potentially recruited
        /// </summary>
        /// <param name="npc">Possible companion NPC</param>
        /// <returns></returns>
        bool IsAvailable(NPC npc);

        /// <summary>
        /// Check if the NPC is recruited as a companion
        /// </summary>
        /// <param name="npc">Possibly recruited NPC</param>
        /// <returns></returns>
        bool IsRecruited(NPC npc);

        /// <summary>
        /// Check if companion NPC is unlocked to recruit by the farmer
        /// </summary>
        /// <param name="farmer">The farmer for which npc is (un)locked</param>
        /// <param name="npc">NPC to be (un)locked for a farmer</param>
        /// <returns></returns>
        bool IsUnlockedFor(Farmer farmer, NPC npc);

        /// <summary>
        /// Check if the NPC is a valid companion
        /// </summary>
        /// <param name="npc">Possibly NPC companion</param>
        /// <returns></returns>
        bool IsValidCompanion(NPC npc);

        /// <summary>
        /// Check if the NPC is a valid companion
        /// </summary>
        /// <param name="npcName">Name of possibly NPC companion</param>
        /// <returns></returns>
        bool IsValidCompanion(string npcName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="farmer">Leader</param>
        /// <param name="npc">Companion NPC (follower)</param>
        /// <returns>True if NPC companion is successfully recruited</returns>
        bool Recruit(Farmer farmer, NPC npc);
        bool TryGetCompanionDialogue(Farmer farmer, string npcName, string dialogueKey, out string dialogueText);
    }
}
