using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace CropStacking
{
    public partial class ModEntry
    {
        private static List<ItemData> GetDataList(string dataString)
        {
            return JsonConvert.DeserializeObject<List<ItemData>>(dataString);
        }
        private static int GetRemainder(List<ItemData> list, ItemData item)
        {
            int total = item.stack;
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a.id == item.id && a.quality == item.quality && a.preservedParentSheetIndex == item.preservedParentSheetIndex && a.color == item.color && a.preserveType == item.preserveType)
                {
                    total += a.stack;
                }
            }
            return Math.Max(0, total - 999);
        }
        private static void SortList(List<ItemData> list)
        {

            for(int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j)
                        continue;
                    var b = list[j];
                    if(a.id == b.id && a.quality == b.quality && a.preservedParentSheetIndex == b.preservedParentSheetIndex && a.color == b.color && a.preserveType == b.preserveType)
                    {
                        list[i].stack += list[j].stack;
                        list[j].stack = 0;
                    }
                }
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].stack == 0)
                    list.RemoveAt(i);
            }
            list.Sort(delegate (ItemData a, ItemData b)
            {
                return a.quality.CompareTo(b.quality);
            });
        }
        private static Item CreateItem(ItemData data)
        {
            Item item = null;
            if (data.color is not null)
            {
                item = new ColoredObject(data.id, data.stack, data.color.Value);
                item.Quality = data.quality;
            }
            else if (data.preservedParentSheetIndex is not null)
            {
                ObjectDataDefinition objectData = ItemRegistry.GetObjectTypeDefinition();
                item = objectData.CreateFlavoredItem(data.preserveType, new Object(data.preservedParentSheetIndex, data.stack, quality: data.quality));
                item.Quality = data.quality;
                item.Stack = data.stack;
            }
            else
            {
                item = new Object(data.id, data.stack, quality: data.quality);
            }
            return item;
        }
    }
}