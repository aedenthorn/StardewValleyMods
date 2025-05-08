using StardewValley;

namespace BirthdayKnowledgeFriendship
{
	public partial class ModEntry
	{
		private static bool CheckBirthday(NPC npc)
		{
			if (!Config.ModEnabled)
				return npc.IsVillager;
			return npc.IsVillager && Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship f) && f.Points >= Config.Hearts * 250;
		}
	}
}
