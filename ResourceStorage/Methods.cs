using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace ResourceStorage
{
    public partial class ModEntry
    {
        public static long ModifyResourceLevel(Farmer instance, string id, int amountToAdd, bool auto = true)
        {
            Dictionary<string, long> dict = GetFarmerResources(instance);
            
            if(!dict.TryGetValue(id, out long oldAmount))
            {
                if (auto && !CanAutoStore(id))
                    return 0;
                oldAmount = 0;
            }
            var newAmount = Math.Max(oldAmount + amountToAdd, 0);
            if(newAmount != oldAmount)
            {
                SMonitor.Log($"Modified resource {id} from {oldAmount} to {newAmount}");
                if (Config.ShowMessage)
                {
                    Object item = new Object(GetIndex(id), (int)(newAmount - oldAmount));
                    try
                    {
                        var hm = new HUDMessage(string.Format(newAmount > oldAmount ? SHelper.Translation.Get("added-x-y") : SHelper.Translation.Get("removed-x-y"), (int)Math.Abs(newAmount - oldAmount), item.DisplayName), Color.WhiteSmoke, 1000) { whatType = newAmount > oldAmount ? 4 : 3 };
                        Game1.addHUDMessage(hm);
                    }
                    catch { }
                }
            }
            if (newAmount <= 0)
                dict.Remove(id);
            else
                dict[id] = newAmount;
            return newAmount - oldAmount;
        }

        public static Dictionary<string, long> GetFarmerResources(Farmer instance)
        {
            if (!resourceDict.TryGetValue(instance.UniqueMultiplayerID, out var dict))
            {
                dict = instance.modData.TryGetValue(dictKey, out var str) ? JsonConvert.DeserializeObject<Dictionary<string, long>>(str) : new();
                resourceDict[instance.UniqueMultiplayerID] = dict;
            }
            return dict;
        }

        public static long GetResourceAmount(Farmer instance, string id)
        {
            Dictionary<string, long> dict = GetFarmerResources(instance);

            return dict.TryGetValue(id, out long amount) ? amount : 0;
        }
        public static bool CanStore(Object obj)
        {
            return !(obj.Quality > 0 || obj.preserve.Value is not null || obj.orderData.Value is not null || obj.preservedParentSheetIndex.Value != 0 || obj.bigCraftable.Value || obj.GetType() != typeof(Object) || obj.maximumStackSize() == 1);
        }
        public static bool CanAutoStore(Object obj)
        {
            if (!CanStore(obj) || !Game1.objectInformation.TryGetValue(obj.ParentSheetIndex, out string data))
                return false;
            
            return CanAutoStore(GetIdString(data));
        }

        public static bool CanAutoStore(string indexOrId)
        {
            int index = GetIndex(indexOrId);
            foreach(var str in Config.AutoStore.Split(','))
            {
                if (int.TryParse(str.Trim(), out int idx))
                {
                    if(idx == index)
                        return true;
                }
                else
                {
                    foreach(var kvp in Game1.objectInformation.Where(p => p.Value.StartsWith(str.Trim() + "/")))
                    {
                        if(kvp.Key == index)
                            return true;
                    }
               }
            }
            return false;
        }

        public static int GetIndex(string indexOrId)
        {
            if (int.TryParse(indexOrId, out var i))
                return i;
            try
            {
                return Game1.objectInformation.First(p => p.Value.StartsWith(indexOrId + "/")).Key;
            }
            catch
            {
                return -1;
            }
        }
        public static string GetIdString(string data)
        {
            return string.Join("/", data.Split('/').Take(4));
        }

        private static int AddIngredientAmount(int ingredient_count, CraftingRecipe recipe, int index)
        {
            if (!Config.ModEnabled || !Config.AutoUse || !Game1.objectInformation.TryGetValue(recipe.recipeList[recipe.recipeList.Keys.ElementAt(index)], out string data))
                return ingredient_count;
            return (int)(ingredient_count + GetResourceAmount(Game1.player, GetIdString(data)));
        }

    }
}