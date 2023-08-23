using StardewValley;

namespace BirthdayFriendship
{
    public partial class ModEntry
    {
        private static bool CheckBirthday(NPC npc)
        {
            if (!Config.ModEnabled)
                return npc.isVillager();
            return npc.isVillager() && Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship f) && f.Points >= Config.Hearts * 250;
        }
    }
}