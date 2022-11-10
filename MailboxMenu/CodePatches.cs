using HarmonyLib;
using StardewValley;
using System.Linq;

namespace MailboxMenu
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.mailbox))]
        public class GameLocation_mailbox_Patch
        {
            public static bool Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled)
                    return true;

                if (Game1.mailbox.Count > 0 && !Game1.player.mailReceived.Contains(Game1.mailbox.First()) && !Game1.mailbox.First().Contains("passedOut") && !Game1.mailbox.First().Contains("Cooking"))
                {
                    Game1.player.mailReceived.Add(Game1.mailbox.First());
                }
                Game1.activeClickableMenu = new MailMenu();
                return false;
            }
        }
    }
}