using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using System;
using System.Linq;

namespace ShippingBonuses
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(ShippingMenu), nameof(ShippingMenu.draw), new Type[] { typeof(SpriteBatch) })]
        public class ShippingMenu_draw_Patch
        {
            public static void Prefix(ShippingMenu __instance, SpriteBatch b)
            {
                if (!Config.EnableMod || !todayBonuses.Any())
                    return;

            }
        }
    }
}