using HarmonyLib;
using StardewValley.Menus;

namespace LocationMap
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameMenu), nameof(GameMenu.changeTab))]
        public static class GameMenu_changeTab_Patch
        {
            public static void Postfix()
            {
                CheckForMapPage();
            }
        }

    }
}