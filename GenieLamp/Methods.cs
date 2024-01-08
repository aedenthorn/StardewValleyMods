using HarmonyLib;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using System.Collections.Generic;

namespace GenieLamp
{
    public partial class ModEntry
    {

        private static void SpawnItem(string target)
        {
            if (!string.IsNullOrEmpty(target))
            {
                string itemId = GetItemId(target);
                if (!string.IsNullOrEmpty(itemId))
                {
                    var item = ItemRegistry.Create(itemId, 1, 0, true);
                    if (item is not null)
                    {
                        int wishes = Game1.player.ActiveObject.modData.TryGetValue(modKey, out var w) ? int.Parse(w) : 0;
                        wishes++;
                        Game1.createItemDebris(item, Game1.player.Position, Game1.player.FacingDirection);
                        Game1.playSound(Config.WishSound, null);
                        if(wishes >= Config.WishesPerItem)
                        {
                            Game1.player.reduceActiveItemByOne();
                            if(Game1.player.ActiveObject != null)
                            {
                                Game1.player.ActiveObject.modData[modKey] = "0";
                            }
                        }
                        else
                        {
                            Game1.player.ActiveObject.modData[modKey] = wishes + "";
                        }
                    }
                }
            }
            Game1.activeClickableMenu.exitThisMenu();
        }

        private static string GetItemId(string target)
        {
            var dict = AccessTools.StaticFieldRefAccess<Dictionary<string, ItemMetadata>>(typeof(ItemRegistry), "CachedItems");
            foreach (var kvp in dict)
            {
                var data = kvp.Value.GetParsedData();
                if (data.DisplayName.Equals(target))
                    return kvp.Key;
            }
            return null;
        }
    }
}