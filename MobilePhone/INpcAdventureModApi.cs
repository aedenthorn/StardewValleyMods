using StardewValley;
using System.Collections.Generic;

namespace MobilePhone
{
	public interface INpcAdventureModApi
	{
		bool CanRecruit(Farmer farmer, NPC npc, out string dialogueKey, out int cooldown);

		bool CanRecruit(Farmer farmer, NPC npc);

		NPC GetCompanionNpc(string name);

		IEnumerable<NPC> GetCompanions();

		bool IsAvailable(NPC npc);

		bool IsRecruited(NPC npc);

		bool IsUnlockedFor(Farmer farmer, NPC npc);

		bool IsValidCompanion(NPC npc);

		bool IsValidCompanion(string npcName);

		bool Recruit(Farmer farmer, NPC npc);

		bool TryGetCompanionDialogue(Farmer farmer, string npcName, string dialogueKey, out string dialogueText);
	}
}