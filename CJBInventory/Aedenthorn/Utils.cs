using CJB.Common;
using CJB.Common.UI;
using CJBInventory.Framework;
using CJBInventory.Framework.Constants;
using CJBInventory.Framework.ItemData;
using CJBInventory.Framework.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace CJBInventory
{
    public static partial class AedenthornInventory
    {

        private static IList<Item> playerInventory = new List<Item>(); 
        private static List<ClickableComponent> inventorySlots;
        private static SpawnableItem[] AllItems;
        private static int totalCapacity = 0;
        private static int scrolled = 0;
        private static int scrollChange = 0;
        private static bool leftMouse;
        private static bool rightMouse;
        private static ModConfig Config;
        private static ModEntry modEntry;
        private static ItemMenu menu;
        public static bool shouldResetItemView;

        private static void ResetItemView(ref InventoryMenu instance)
        {
            modEntry.Monitor.Log("RefreshMenu");

            // update items in view
            playerInventory.Clear();

            foreach (var item in Game1.player.Items)
            {
                if (item is not Object || (!menu.ItemsWithoutQuality.Contains(item.ParentSheetIndex) && menu.Quality != ItemQuality.Normal && (item as Object).Quality != (int)menu.Quality) || !menu.FilteredItems.Exists(i => i.NameEquivalentTo(item.Name)))
                    continue;
                playerInventory.Add(item);
            }
            for (int i = 0; i < Game1.player.maxItems.Value; i++)
            {
                if (playerInventory.Count <= i)
                {
                    playerInventory.Add(null);
                }
            }
            inventorySlots = new List<ClickableComponent>();
            totalCapacity = Math.Max(48, ((playerInventory.Count - 1) / 12 + 2) * 12);
            instance.rows = 3;
            instance.capacity = 36;
            instance.actualInventory = playerInventory.Skip(scrolled * 12).Take(36).ToList();
            shouldResetItemView = false;
        }
    }
}