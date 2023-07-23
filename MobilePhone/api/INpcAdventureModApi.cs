using StardewValley;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MobilePhone.Api
{
    /// <summary>
    /// Minimal API for NPC Adventures mod.
    /// This is for V1.0.2 (Polycule).
    /// </summary>
    public interface INpcAdventureModApi
    {
        /// <summary>
        /// Checks if the companion can be recruited.
        /// </summary>
        /// <param name="farmer">Farmer who checks if they can recruit a companion</param>
        /// <param name="npc">Companion NPC to check</param>
        /// <returns>
        /// False when this npc is not a companion, is locked for farmer,
        /// is unavailable (recruited, festival day ...)
        /// or doesn't accept adventure today.
        /// </returns>
        bool CanRecruit(Farmer farmer, NPC npc);

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
        /// .
        /// </summary>
        /// <param name="farmer">Leader</param>
        /// <param name="npc">Companion NPC (follower)</param>
        /// <returns>True if NPC companion is successfully recruited</returns>
        bool Recruit(Farmer farmer, NPC npc);
    }
}
