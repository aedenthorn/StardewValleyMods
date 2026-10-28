using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace ShipAbsolutelyEverything
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(CollectionsPage), new Type[] {typeof(int), typeof(int), typeof(int), typeof(int)})]
        [HarmonyPatch(MethodType.Constructor)]
        public static class CollectionsPage_Patch
        {
            public static void Postfix(CollectionsPage __instance, int x, int y, int width, int height)
            {
                if(!Config.ModEnabled)
                    return;

                int baseX = __instance.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
                int baseY = __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
                int collectionWidth = 10;
                var last = __instance.collections[0].Last().Last().bounds;
                var widthUsed = (last.X - baseX) / 68 + collectionWidth * (last.Y - baseY) / 68 + 1;

                List<ParsedItemData> dataEntries = new List<ParsedItemData>(from entry in ItemRegistry.GetObjectTypeDefinition().GetAllData() orderby entry.Category, entry.TextureName, entry.SpriteIndex select entry);

                foreach (ParsedItemData data in dataEntries)
                {
                    string id = data.ItemId;
                    foreach(var page in __instance.collections[0])
                    {
                        foreach(var cc in page)
                        {
                            if (cc.name.StartsWith(id + " "))
                                goto cont;
                        }
                    }
                    var obj = ItemRegistry.Create(data.QualifiedItemId, 1, 0, true);
                    if(!ShouldIncludeForShipping(obj))
                    {
                        continue;
                    }
                    bool farmerHas = Game1.player.basicShipped.ContainsKey(id);


                    int xPos = baseX + widthUsed % collectionWidth * 68;
                    int yPos = baseY + widthUsed / collectionWidth * 68;
                    if (yPos > __instance.yPositionOnScreen + height - 128)
                    {
                        __instance.collections[0].Add(new List<ClickableTextureComponent>());
                        widthUsed = 0;
                        xPos = baseX;
                        yPos = baseY;
                    }
                    if (__instance.collections[0].Count == 0)
                    {
                        __instance.collections[0].Add(new List<ClickableTextureComponent>());
                    }
                    List<ClickableTextureComponent> list = __instance.collections[0].Last<List<ClickableTextureComponent>>();
                    ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
                    list.Add(new ClickableTextureComponent(string.Concat(new string[]
                    {
                    id,
                    " ",
                    farmerHas.ToString(),
                    " ",
                    "false"
                    }), new Rectangle(xPos, yPos, 64, 64), null, "", itemData.GetTexture(), itemData.GetSourceRect(0, null), 4f, farmerHas)
                    {
                        myID = list.Count,
                        rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
                        leftNeighborID = ((list.Count % collectionWidth == 0) ? 7001 : (list.Count - 1)),
                        downNeighborID = ((yPos + 68 > __instance.yPositionOnScreen + height - 128) ? (-7777) : (list.Count + collectionWidth)),
                        upNeighborID = ((list.Count < collectionWidth) ? 12347 : (list.Count - collectionWidth)),
                        fullyImmutable = true
                    });
                    widthUsed++;
                cont:
                    continue;
                }
            }

        }

        [HarmonyPatch(typeof(Utility), nameof(Utility.getFarmerItemsShippedPercent))]
        public class Utility_getFarmerItemsShippedPercent_Patch
        {
            public static bool Prefix(Farmer who, ref float __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (who == null)
                {
                    who = Game1.player;
                }
                Utility.recentlyDiscoveredMissingBasicShippedItem = null;
                int farmerShipped = 0;
                int total = 0;
                foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
                {
                    var item = ItemRegistry.Create(data.QualifiedItemId, 1, 0, true);
                    if(!ShouldIncludeForShipping(item))
                    {
                        continue;
                    }
                    total++;
                    if (who.basicShipped.ContainsKey(data.ItemId))
                    {
                        farmerShipped++;
                    }
                    else if (Utility.recentlyDiscoveredMissingBasicShippedItem == null)
                    {
                        Utility.recentlyDiscoveredMissingBasicShippedItem = ItemRegistry.Create(data.QualifiedItemId, 1, 0, false);
                    }
                }
                __result = (float)farmerShipped / (float)total;
                return false;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.countsForShippedCollection))]
        public class Object_countsForShippedCollection_Patch
        {
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                __result = ShouldIncludeForShipping(__instance);
                return false;
            }

        }
    }
}