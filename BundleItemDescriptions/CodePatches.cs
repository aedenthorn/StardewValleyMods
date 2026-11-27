using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BundleItemDescriptions
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(JunimoNoteMenu), nameof(JunimoNoteMenu.gameWindowSizeChanged))]
        public static class JunimoNoteMenu_gameWindowSizeChanged_Patch
        {
            public static void Postfix(JunimoNoteMenu __instance)
            {
                if (!Config.ModEnabled || !__instance.specificBundlePage)
                    return;
                AdjustText(__instance);
            }
        }
        [HarmonyPatch(typeof(JunimoNoteMenu), "setUpBundleSpecificPage")]
        public static class JunimoNoteMenu_setUpBundleSpecificPage_Patch
        {
            public static void Postfix(JunimoNoteMenu __instance)
            {
                if (!Config.ModEnabled || !__instance.specificBundlePage)
                    return;
                AdjustText(__instance);
            }
        }
        [HarmonyPatch(typeof(JunimoNoteMenu), nameof(JunimoNoteMenu.draw))]
        public static class JunimoNoteMenu_draw_Patch
        {
            public static void Prefix(JunimoNoteMenu __instance, ref string[] __state)
            {
                if (!Config.ModEnabled || !__instance.specificBundlePage || JunimoNoteMenu.hoverText?.Contains("^") != true)
                    return;
                __state = JunimoNoteMenu.hoverText.Split('^');
                JunimoNoteMenu.hoverText = null;
            }
            public static void Postfix(JunimoNoteMenu __instance, SpriteBatch b, string[] __state)
            {
                if (__state is null)
                    return;
                IClickableMenu.drawToolTip(b, __state[1], __state[0], null, false, -1, 0, null, -1, null, -1, null);
            }
        }
    }
}