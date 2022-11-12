using HarmonyLib;
using StardewModdingAPI;
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
                if (!Config.ModEnabled || !Config.MenuOnMailbox || (Config.ModKey != SButton.None && !SHelper.Input.IsDown(Config.ModKey)))
                    return true;

                Game1.activeClickableMenu = new MailMenu();
                return false;
            }
        }
    }
}