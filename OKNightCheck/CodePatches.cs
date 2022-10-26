using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace OKNightCheck
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.showEndOfNightStuff))]
        public class Game1_showEndOfNightStuff_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;
                if(Game1.endOfNightMenus.Count == 0 && Game1.activeClickableMenu is SaveGameMenu)
                {
                    SMonitor.Log("Showing pause menu");
                    Game1.activeClickableMenu = new OKMenu();
                }
            }
        }
    }
}