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
        public static int ModifyResourceLevel(Farmer instance, string id, int amountToAdd, bool auto = true)
        {
            Dictionary<string, int> dict = GetFarmerResources(instance);
            
            if(!dict.TryGetValue(id, out int oldAmount))
            {
                if (auto && !CanAutoStore(id))
                    return 0;
                oldAmount = 0;
            }
            var newAmount = Math.Max(oldAmount + amountToAdd, 0);
            if(newAmount != oldAmount)
            {
                SMonitor.Log($"Modify resource {id} from {oldAmount} to {newAmount}");
            }
            dict[id] = newAmount;
            return newAmount - oldAmount;
        }

        public static Dictionary<string, int> GetFarmerResources(Farmer instance)
        {
            if (!resourceDict.TryGetValue(instance.UniqueMultiplayerID, out var dict))
            {
                dict = instance.modData.TryGetValue(dictKey, out var str) ? JsonConvert.DeserializeObject<Dictionary<string, int>>(str) : new();
                resourceDict[instance.UniqueMultiplayerID] = dict;
            }
            return dict;
        }

        public static int GetResourceAmount(Farmer instance, string id)
        {
            Dictionary<string, int> dict = GetFarmerResources(instance);

            return dict.TryGetValue(id, out int amount) ? amount : 0;
        }
        public static bool CanStore(Object obj)
        {
            return !(obj.Quality > 0 || obj.modData.Count() > 0 || obj.bigCraftable.Value || obj.GetType() != typeof(Object) || obj.maximumStackSize() == 1);
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
            if (!Config.ModEnabled || !Game1.objectInformation.TryGetValue(recipe.recipeList[recipe.recipeList.Keys.ElementAt(index)], out string data))
                return ingredient_count;
            return ingredient_count + GetResourceAmount(Game1.player, GetIdString(data));
        }

    }
}