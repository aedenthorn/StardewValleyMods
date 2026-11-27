using Newtonsoft.Json;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.SaveSerialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Pockets
{
    public partial class ModEntry
    {

        public static bool TryGetPocket(Farmer who, out PocketData data, out IList<Item> items, int x = -1, int y = -1)
        {
            if (who.pantsItem.Value is Clothing p)
            {
                if (TryGetPocketInventory(p, out data, out items, x, y))
                    return true;
                if (Config.GlobalDefaultPockets) 
                {
                    foreach (var kvp in Config.DefaultPockets)
                    {
                        if (kvp.Value.ClothesType == Clothing.ClothesType.PANTS)
                        {
                            if (TryGetDefaultPocketInventory(who.UniqueMultiplayerID, kvp.Key, kvp.Value, out items, x, y))
                            {
                                data = kvp.Value;
                                return true;
                            }
                        }
                    }
                }
            }
            if (who.shirtItem.Value is Clothing s)
            {
                if (TryGetPocketInventory(s, out data, out items, x, y))
                    return true;
                if (Config.GlobalDefaultPockets)
                {
                    foreach (var kvp in Config.DefaultPockets)
                    {
                        if (kvp.Value.ClothesType == Clothing.ClothesType.SHIRT)
                        {
                            if (TryGetDefaultPocketInventory(who.UniqueMultiplayerID, kvp.Key, kvp.Value, out items, x, y))
                            {
                                data = kvp.Value;
                                return true;
                            }
                        }
                    }
                }
            }
            data = null;
            items = null;
            return false;
        }
        private static bool TryGetPocketInventory(Item item, out PocketData data, out IList<Item> items, int x, int y)
        {
            if (TryGetPocketData(item, out var clothesPockets))
            {
                foreach(var kvp in clothesPockets)
                {
                    if (kvp.Value.HotKey.GetState() == StardewModdingAPI.SButtonState.Pressed || (x > -1 && kvp.Value.StartX <= x && kvp.Value.StartX + kvp.Value.Width > x && kvp.Value.StartY <= y && kvp.Value.StartY + kvp.Value.Height > y))
                    {
                        data = kvp.Value;
                        if (!InventoryDict.TryGetValue(item, out var list))
                        {
                            if(item.modData.TryGetValue(modKey, out var str))
                            {
                                try
                                {
                                    list = new Dictionary<string, Inventory>();
                                    foreach(var kvp2 in JsonConvert.DeserializeObject<Dictionary<string, string>>(str))
                                    {
                                        list[kvp2.Key] = MakeInventoryFromXML(kvp2.Value, data.PocketSlots);
                                    }
                                    InventoryDict[item] = list;
                                }
                                catch
                                {
                                    item.modData.Remove(modKey);
                                }

                            }
                        }
                        if (list is null)
                        {
                            list = new Dictionary<string, Inventory>();
                            InventoryDict[item] = list;
                        }
                        if (!list.TryGetValue(kvp.Key, out var inv))
                        {
                            inv = new Inventory();
                            list[kvp.Key] = inv;
                        }
                        while (inv.Count < data.PocketSlots)
                        {
                            inv.Add(null);
                        }
                        items = inv;
                        return true;
                    }
                }
            }
            data = null;
            items = null;
            return false;
        }

        public static bool TryGetPocketData(Item item, out Dictionary<string, PocketData> clothesPockets)
        {
            if (PocketDict.TryGetValue(item.ItemId, out clothesPockets))
                return true;
            if (!Config.GlobalDefaultPockets && item is Clothing c)
            {
                clothesPockets = new();
                foreach(var kvp in Config.DefaultPockets)
                {
                    if (kvp.Value.ClothesType == c.clothesType.Value)
                    {
                        clothesPockets[kvp.Key] = kvp.Value;
                    }
                }
                return true;
            }
            return false;
        }

        private static bool TryGetDefaultPocketInventory(long who, string pocketId, PocketData data, out IList<Item> items, int x, int y)
        {
            if (data.HotKey.GetState() == StardewModdingAPI.SButtonState.Pressed || (x > -1 && data.StartX <= x && data.StartX + data.Width > x && data.StartY <= y && data.StartY + data.Height > y))
            {
                if (!DefaultInventoryDict.TryGetValue(who, out var list))
                {
                    if (Game1.GetPlayer(who).modData.TryGetValue(modKey, out var str))
                    {
                        try
                        {
                            list = new Dictionary<string, Inventory>();
                            foreach (var kvp2 in JsonConvert.DeserializeObject<Dictionary<string, string>>(str))
                            {
                                list[kvp2.Key] = MakeInventoryFromXML(kvp2.Value, data.PocketSlots);
                            }
                            DefaultInventoryDict[who] = list;
                        }
                        catch
                        {
                            Game1.GetPlayer(who).modData.Remove(modKey);
                        }

                    }
                }
                if (list is null)
                {
                    list = new Dictionary<string, Inventory>();
                    DefaultInventoryDict[who] = list;
                }
                if (!list.TryGetValue(pocketId, out var inv))
                {
                    inv = new Inventory();
                    list[pocketId] = inv;
                }
                while (inv.Count < data.PocketSlots)
                {
                    inv.Add(null);
                }
                items = inv;
                return true;
            }

            items = null;
            return false;
        }
        public static void OpenPocket(InventoryPage page, PocketData data, IList<Item> inventory, bool playSound = true)
        {
            if (openPocket == data)
            {
                ClosePocket(page, playSound);
            }
            else
            {
                openPocket = data;
                page.inventory = new InventoryMenu(page.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, page.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, false, inventory, null, data.PocketSlots, Math.Clamp(data.PocketRows, 1, 3), 0, 0, true);
                if (playSound)
                {
                    Game1.playSound("bigSelect");
                }
            }
        }

        public static void ClosePocket(InventoryPage page, bool playSound)
        {
            openPocket = null;
            page.inventory = new InventoryMenu(page.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, page.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, true, null, null, -1, 3, 0, 0, true);
            if (playSound)
            {
                Game1.playSound("bigDeSelect");
            }
        }

        public static Inventory MakeInventoryFromXML(string xml, int max)
        {
            Inventory inv = new();
            StringReader stringReader;
            stringReader = new StringReader(xml);
            XmlTextReader reader;
            reader = new XmlTextReader(stringReader);
            //inv.ReadXml(reader);
            bool isEmptyElement = reader.IsEmptyElement;
            reader.Read();
            reader.ReadStartElement();
            if (isEmptyElement)
            {
                return inv;
            }
            while (reader.NodeType != XmlNodeType.EndElement && inv.Count < max)
            {
                Item item = SaveSerializer.Deserialize<Item>(reader);
                inv.Add(item);
            }
            reader.Close();
            stringReader.Close();
            return inv;
        }

        public static string MakeXMLFromInventories(Dictionary<string, Inventory> value)
        {
            Dictionary<string, string> dict = new();

            foreach (var kvp in value) 
            {
                using (var sw = new StringWriter())
                {
                    using (var writer = XmlWriter.Create(sw))
                    {
                        writer.WriteStartDocument();
                        SaveSerializer.GetSerializer(typeof(Inventory)).SerializeFast(writer, kvp.Value);
                        writer.WriteEndDocument();
                        writer.Flush();
                    }
                    dict[kvp.Key] = sw.ToString();
                }
            }
            return JsonConvert.SerializeObject(dict);
        }
        public static Item AddItemToInventory(IList<Item> inv, Item item)
        {
            if (item == null)
            {
                return null;
            }
            int originalStack = item.Stack;
            int stackLeft = originalStack;
            foreach (Item slot in inv)
            {
                if (item.canStackWith(slot))
                {
                    int stack = item.Stack;
                    stackLeft = slot.addToStack(item);
                    int added = stack - stackLeft;
                    if (added > 0)
                    {
                        item.Stack = stackLeft;
                        if (stackLeft < 1)
                        {
                            break;
                        }
                    }
                }
            }
            if (stackLeft > 0)
            {
                int i = 0;
                while (i < inv.Count)
                {
                    if (inv[i] == null)
                    {
                        item.onDetachedFromParent();
                        inv[i] = item;
                        stackLeft = 0;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            if (originalStack > stackLeft)
            {
                Game1.player.ShowItemReceivedHudMessageIfNeeded(item, originalStack - stackLeft);
            }
            if (stackLeft <= 0)
            {
                return null;
            }
            return item;
        }


        private void PantsItem_fieldChangeEvent(Netcode.NetRef<Clothing> field, Clothing oldValue, Clothing newValue)
        {
            if (openPocket?.ClothesType == Clothing.ClothesType.PANTS)
            {
                ForceClosePocket();
            }
        }


        private void ShirtItem_fieldChangeEvent(Netcode.NetRef<Clothing> field, Clothing oldValue, Clothing newValue)
        {
            if (openPocket?.ClothesType == Clothing.ClothesType.SHIRT)
            {
                ForceClosePocket();
            }
        }
        public static void ForceClosePocket()
        {
            if (Game1.activeClickableMenu is GameMenu menu && menu.GetCurrentPage() is InventoryPage page)
            {
                ClosePocket(page, false);
            }
            else
            {
                openPocket = null;
            }

        }
    }
}