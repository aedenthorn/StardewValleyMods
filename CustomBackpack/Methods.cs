using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace CustomBackpack
{
    public partial class ModEntry
    {
        public static ClickableTextureComponent upArrow;
        public static ClickableTextureComponent downArrow;
        public static int scrolled = 0;
        public static List<Item> tempInventory = new List<Item>();

        public enum Where
        {
            above,
            below,
            within,
            absent
        }

        private static Where GetGridPosition(InventoryMenu __instance, int x, int y)
        {
            int width = __instance.capacity / __instance.rows;

            for (int i = 0; i < __instance.inventory.Count; i++)
            {
                if (__instance.inventory[i] != null && __instance.inventory[i].containsPoint(x, y))
                {
                    if (i < scrolled * width)
                    {
                        return Where.above;
                    }
                    else if (i >= scrolled * width + __instance.capacity && __instance.actualInventory.Count >= __instance.capacity / __instance.rows * (scrolled + 1) + __instance.capacity)
                    {
                        return Where.below;
                    }
                    else
                    {
                        return Where.within;
                    }
                }
            }
            return Where.absent;
        }

        private static void SetPlayerSlots(int i)
        {
            if (!Config.ModEnabled || Game1.player is null)
                return;
            if (i % 12 != 0)
                i = (i / 12 + 1) * 12;
            SMonitor.Log($"Setting player backpack slots to {i}");
            Game1.player.maxItems.Value = i;
            while (Game1.player.Items.Count < Game1.player.maxItems.Value)
            {
                Game1.player.Items.Add(null);
            }
        }
    }
}