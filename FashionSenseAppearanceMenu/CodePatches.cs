using FashionSense.Framework.UI;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace FashionSenseAppearanceMenu
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(HandMirrorMenu), nameof(HandMirrorMenu.receiveKeyPress))]
        public class HandMirrorMenu_receiveKeyPress_Patch
        {

            public static bool Prefix(HandMirrorMenu __instance, Keys key)
            {
                if (!Config.ModEnabled || Config.MenuKey != (SButton)key)
                    return true;
                OpenMenu(__instance);
                return false;
            }

        }

        [HarmonyPatch(typeof(HandMirrorMenu), nameof(HandMirrorMenu.receiveLeftClick))]
        public class HandMirrorMenu_receiveLeftClick_Patch
        {

            public static bool Prefix(HandMirrorMenu __instance, int x, int y, ClickableComponent ___appearanceLabel, ClickableComponent ___descriptionLabel)
            {
                if (!Config.ModEnabled || (!___descriptionLabel.containsPoint(x, y) && !___appearanceLabel.containsPoint(x, y)) || __instance.leftSelectionButtons.Where(b => b.containsPoint(x, y)).Any() || __instance.rightSelectionButtons.Where(b => b.containsPoint(x, y)).Any())
                    return true;
                OpenMenu(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1.playSound), new Type[] { typeof(string), typeof(int) })]
        public class Game1_playSound_Patch
        {

            public static bool Prefix()
            {
                return (!Config.ModEnabled || !mute.Value);
            }
        }
    }
}