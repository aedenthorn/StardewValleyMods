using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;

namespace ContentPatcherEditor
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.receiveKeyPress))]
        public class TitleMenu_receiveKeyPress_Patch
        {
            public static void Postfix(TitleMenu __instance, Keys key)
            {
                if (!Config.ModEnabled || TitleMenu.subMenu is null || !Game1.options.doesInputListContain(Game1.options.menuButton, key) || !TitleMenu.subMenu.readyToClose())
                    return;
                TitleMenu.subMenu.receiveKeyPress(key);
            }
        }
    }
}