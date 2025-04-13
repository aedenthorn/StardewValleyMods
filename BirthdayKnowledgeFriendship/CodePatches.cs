using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace BirthdayKnowledgeFriendship
{
	public partial class ModEntry
	{
		public class Billboard_Patch
		{
			public static void Postfix(Dictionary<int, List<NPC>> __result)
			{
				__result.Values.ToList().ForEach(npcList => npcList.RemoveAll(npc => !CheckBirthday(npc)));
			}
		}
	}
}
