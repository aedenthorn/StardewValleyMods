using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static StardewValley.Menus.CharacterCustomization;
using Object = StardewValley.Object;

namespace QualityCondensing
{
    public partial class ModEntry
    {
            
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || Game1.player.CursorSlotItem is not Object held || !Config.CondenseButton.JustPressed())
                {
                    return;
                }
                var mousePos = Game1.getMousePosition(true);
                for(int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if(cc.containsPoint(mousePos.X, mousePos.Y))
                    {
                        if(__instance.actualInventory.Count > i)
                        {
                            bool add = false;
                            if(__instance.actualInventory[i] is Object target && target.QualifiedItemId == held.QualifiedItemId && target.Quality > held.Quality && target.Stack < target.maximumStackSize())
                            {
                                add = true;
                            }
                            else if(__instance.actualInventory[i] is null)
                            {
                                int quality = GetNextQuality(held.Quality);
                                if (quality < 0)
                                    return;
                                target = held.getOne() as Object;
                                target.Quality = quality;
                            }
                            else
                            {
                                return;
                            }
                            int req = RequiredForCondensing(held.Quality, target.Quality);
                            if (req <= 0 || held.Stack < req)
                                return;
                            held.Stack -= req;
                            if (held.Stack <= 0)
                            {
                                Game1.player.CursorSlotItem = null;
                            }
                            if (add)
                            {
                                target.Stack++;
                            }
                            else
                            {
                                __instance.actualInventory[i] = target;
                            }
                            Game1.playSound("coin");
                            foreach (var k in Config.CondenseButton.Keybinds)
                            {
                                foreach (var b in k.Buttons)
                                {
                                    SHelper.Input.Suppress(b);
                                }
                            }
                        }
                        return;
                    }
                }

            }

        }

    }
}