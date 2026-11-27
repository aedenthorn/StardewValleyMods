using StardewValley;
using StardewValley.Menus;
using System;

namespace BirthdayFriendship
{
    public partial class ModEntry
    {
        private static bool CheckBirthday(NPC npc)
        {
            if (!Config.ModEnabled || npc is null)
                return true;
            return npc.IsVillager && Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship f) && f.Points >= Config.Hearts * 250;
        }


        public static int GetSeasonNumber(int value, ProfileMenu profile)
        {
            if (CheckBirthday(profile.Current?.Character as NPC))
                return value;
            return -1;
        }
    }
}