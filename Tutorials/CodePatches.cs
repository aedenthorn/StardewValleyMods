using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace Tutorials
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string), typeof(Farmer),typeof(xTile.Dimensions.Location) })]
        public static class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string fullActionString, Farmer who)
            {
                if (!Config.ModEnabled || !TriggerDict.TryGetValue(fullActionString, out var trigger))
                {
                    return true;
                }
                if (trigger.Tutorial is not null && OpenTutorial(trigger.Tutorial, null, trigger.Categories))
                    return false;
                if (trigger.Category is not null && OpenTutorial(null, trigger.Category, trigger.Categories))
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || !Config.OpenTutorialKey.JustPressed())
                {
                    return;
                }
                var mousePos = Game1.getMousePosition(true);
                for (int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if (cc.containsPoint(mousePos.X, mousePos.Y))
                    {
                        if (__instance.actualInventory.Count > i && __instance.actualInventory[i] is Item obj)
                        {
                            var list = TriggerDict.Where(p => p.Key == obj.QualifiedItemId);
                            foreach(var kvp in list)
                            {
                                if (kvp.Value.Tutorial is not null && OpenTutorial(kvp.Value.Tutorial, null, kvp.Value.Categories))
                                    return;
                                if (kvp.Value.Category is not null && OpenTutorial(null, kvp.Value.Category, kvp.Value.Categories))
                                    return;
                            }
                        }
                        return;
                    }
                }

            }
        }
    }
}