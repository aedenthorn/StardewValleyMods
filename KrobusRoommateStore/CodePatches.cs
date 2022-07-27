using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Linq;

namespace KrobusRoommateStore
{
    public partial class ModEntry
    {
        public static bool checkingPos;
        public static Vector2 lastPos;
        public static Vector2 lastSpeed;
        public static Vector2 speed;

        [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
        public class NPC_checkAction_Patch
        {
            public static void Postfix(NPC __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Equals("Krobus") || __instance.CurrentDialogue.Count > 0 || !Game1.player.friendshipData.TryGetValue(__instance.Name, out Friendship f) || (!f.IsRoommate() && !f.IsMarried()))
                    return;
                Sewer s = (Sewer)Game1.locations.FirstOrDefault(l => l is Sewer);
                Game1.activeClickableMenu = new ShopMenu(s.getShadowShopStock(), 0, "Krobus", new Func<ISalable, Farmer, int, bool>(s.onShopPurchase), null, null);
            }
        }
        
   }
}