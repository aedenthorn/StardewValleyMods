using Microsoft.Xna.Framework;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace Restauranteer
{
    public partial class ModEntry
    {
        private void UpdateOrders()
        {
            foreach(var c in Game1.player.currentLocation.characters)
            {

                if (c.IsVillager && !Config.IgnoredNPCs.Contains(c.Name))
                {
                    npcEmotesDict.Remove(c.Name);
                    CheckOrder(c, Game1.player.currentLocation);
                }
                else
                {
                    c.modData.Remove(orderKey);
                }
            }
        }

        private void CheckOrder(NPC npc, GameLocation location)
        {
            if (npc.modData.ContainsKey(orderKey))
            {
                try
                {
                    UpdateOrder(npc);
                }
                catch 
                {
                    SMonitor.Log($"error updating order, removing mod data from {npc.Name}");
                    npc.modData.Remove(orderKey);
                }
                return;
            }
            if (!Game1.NPCGiftTastes.ContainsKey(npc.Name) || npcOrderNumbers.Value.TryGetValue(npc.Name, out int amount) && amount >= Config.MaxNPCOrdersPerNight)
                return;
            if(Game1.random.NextDouble() < Config.OrderChance)
            {
                StartOrder(npc, location);
            }
        }

        public static void UpdateOrder(NPC npc)
        {
            if (!npc.IsEmoting)
            {
                npc.doEmote(emoteBaseIndex, false);
            }
        }

        public static void StartOrder(NPC npc, GameLocation location)
        {
            List<string> loves = new();
            foreach(var str in Game1.NPCGiftTastes["Universal_Love"].Split(' '))
            {
                if (Game1.objectData.TryGetValue(str, out var data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name) && !IsIgnored(str, data))
                {
                    loves.Add(str);
                }
            }
            foreach(var str in Game1.NPCGiftTastes[npc.Name].Split('/')[1].Split(' '))
            {
                if (Game1.objectData.TryGetValue(str, out var data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name) && !IsIgnored(str, data))
                {
                    loves.Add(str);
                }
            }
            List<string> likes = new();
            foreach(var str in Game1.NPCGiftTastes["Universal_Like"].Split(' '))
            {
                if (Game1.objectData.TryGetValue(str, out var data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name) && !IsIgnored(str, data))
                {
                    likes.Add(str);
                }
            }
            foreach (var str in Game1.NPCGiftTastes[npc.Name].Split('/')[3].Split(' '))
            {
                if (Game1.objectData.TryGetValue(str, out var data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name) && !IsIgnored(str, data))
                {
                    likes.Add(str);
                }
            }
            if (!loves.Any() && !likes.Any())
                return;
            bool loved = true;
            string dish;
            if (loves.Any() && (!likes.Any() || (Game1.random.NextDouble() <= Config.LovedDishChance)))
            {
                dish = loves[Game1.random.Next(loves.Count)];
            }
            else
            {
                loved = false;
                dish = likes[Game1.random.Next(likes.Count)];
            }
            var dobj = ItemRegistry.Create(dish);
            int price = dobj.sellToStorePrice();
            SMonitor.Log($"{npc.Name} is going to order {dobj.Name}");
            npc.modData[orderKey] = JsonConvert.SerializeObject(new OrderData(dish, dobj.Name, dobj.DisplayName, price, loved));
            if (Config.AutoFillFridge)
            {
                FillFridge(location);
            }
        }
        public static bool IsIgnored(string key, ObjectData data)
        {
            if (Config.IgnoredRecipes.Contains(key))
                return true;
            if (Config.IgnoredRecipes.Contains(data.Name))
                return true;
            if (Config.IgnoredRecipes.Any(r => r.EndsWith("*") && (key.StartsWith(r[..^1]) || data.Name.StartsWith(r[..^1]))))
                return true;
            return false;
        }
        public static NetRef<Chest> GetFridge(GameLocation location)
        {
            if(location is FarmHouse)
            {
                return (location as FarmHouse).fridge;
            }
            if(location is IslandFarmHouse)
            {
                return (location as IslandFarmHouse).fridge;
            }
            location.objects.Remove(fridgeHideTile);
            
            if (!fridgeDict.TryGetValue(location.Name, out NetRef<Chest> fridge))
            {
                fridge = fridgeDict[location.Name] = new NetRef<Chest>(new Chest(true));
                
            }
            return fridge;
        }
        public static void FillFridge(GameLocation __instance)
        {
            var fridge = GetFridge(__instance);

            fridge.Value.Items.Clear();
            foreach (var c in __instance.characters)
            {
                if (c.modData.TryGetValue(orderKey, out string dataString))
                {
                    try
                    {
                        OrderData data = JsonConvert.DeserializeObject<OrderData>(dataString, new JsonSerializerSettings());

                        CraftingRecipe r = new CraftingRecipe(data.dishName, true);
                        if (r is not null)
                        {
                            foreach (var key in r.recipeList.Keys)
                            {
                                if (Game1.objectData.ContainsKey(key))
                                {
                                    var obj = new Object(key, r.recipeList[key]);
                                    SMonitor.Log($"Adding {obj.Name} ({obj.ParentSheetIndex}) x{obj.Stack} to fridge");
                                    fridge.Value.addItem(obj);
                                }
                                else
                                {
                                    List<string> list = new List<string>();
                                    foreach (var kvp in Game1.objectData)
                                    {
                                        if (kvp.Value.Category.ToString() == key)
                                        {
                                            list.Add(kvp.Key);
                                        }
                                    }
                                    if (list.Any())
                                    {
                                        var obj = new Object(list[Game1.random.Next(list.Count)], r.recipeList[key]);
                                        SMonitor.Log($"Adding {obj.Name} ({obj.ParentSheetIndex}) x{obj.Stack} to fridge");
                                        fridge.Value.addItem(obj);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        c.modData.Remove(orderKey);
                        continue;
                    }
                }
            }
        }
    }
}