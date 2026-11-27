using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FriendshipTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Utility), nameof(Utility.GetMaximumHeartsForCharacter))]
        public class Utility_GetMaximumHeartsForCharacter_Patch
        {
            public static bool Prefix(Character character, ref int __result)
            {
                if (!Config.ModEnabled || character is not NPC)
                    return true;

                if (!Game1.player.friendshipData.TryGetValue(character.Name, out Friendship f) || !f.IsMarried())
                    return true;
                //f.Points = 250 * Config.MaxHearts;
                __result = Config.MaxHearts;
                return false;
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.changeFriendship))]
        public class Farmer_changeFriendship_Patch
        {
            public static void Prefix(ref int amount)
            {
                if (!Config.ModEnabled)
                    return;
                amount = (int)Math.Round(amount * (Math.Sign(amount) > 0 ? Config.IncreaseModifier : Config.DecreaseModifier));
            }
        }
    }
}