using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TakeAll
{
    public partial class ModEntry
    {
        private static bool TryTakeItems(InventoryMenu menu)
        {
            if (menu.actualInventory == Game1.player.Items)
            {
                SMonitor.Log("found player inventory");
                playerItems = menu.actualInventory;
            }
            else
            {
                SMonitor.Log("found other inventory");
                otherItems = menu.actualInventory;
            }
            if (playerItems is not null && otherItems is not null)
            {
                TakeItems();
                playerItems = null;
                otherItems = null;
                return true;
            }
            return false;
        }
        private static void TakeItems()
        {
            bool same = SHelper.Input.IsDown(Config.ModButton) != Config.TakeSameByDefault;
            for(int i = 0; i < otherItems.Count; i++)
            {
                var item = otherItems[i];
                if (same)
                {
                    bool contains = false;
                    foreach(var m in Game1.player.Items)
                    {
                        if (m is not null && m.Name == item.Name)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                        continue;
                }
                var newItem = Game1.player.addItemToInventory(item);
                if(newItem is null)
                {
                    otherItems[i] = null;
                }
                else
                {
                    otherItems[i].Stack = newItem.Stack;
                }
            }
        }
    }
}