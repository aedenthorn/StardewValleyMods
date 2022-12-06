using Netcode;
using Newtonsoft.Json;
using StardewValley;
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

                if (c.isVillager() && !Config.IgnoredNPCs.Contains(c.Name))
                {
                    CheckOrder(c);
                }
                else
                {
                    c.modData.Remove(orderKey);
                }
            }
        }

        private void CheckOrder(NPC npc)
        {
            if (npc.modData.TryGetValue(orderKey, out string orderData))
            {
                //npc.modData.Remove(orderKey);
                UpdateOrder(npc, JsonConvert.DeserializeObject<OrderData>(orderData));
                return;
            }
            if (!Game1.NPCGiftTastes.ContainsKey(npc.Name) || npcOrderNumbers.Value.TryGetValue(npc.Name, out int amount) && amount >= Config.MaxNPCOrdersPerNight)
                return;
            if(Game1.random.NextDouble() < Config.OrderChance)
            {
                StartOrder(npc);
            }
        }

        private void UpdateOrder(NPC npc, OrderData orderData)
        {
            if (!npc.IsEmoting)
            {
                npc.doEmote(424242, false);
            }
        }

        private void StartOrder(NPC npc)
        {
            List<int> loves = new();
            foreach(var str in Game1.NPCGiftTastes["Universal_Love"].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectInformation.TryGetValue(i, out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
                {
                    loves.Add(int.Parse(str));
                }
            }
            foreach(var str in Game1.NPCGiftTastes[npc.Name].Split('/')[1].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectInformation.TryGetValue(i, out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
                {
                    loves.Add(int.Parse(str));
                }
            }
            List<int> likes = new();
            foreach(var str in Game1.NPCGiftTastes["Universal_Like"].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectInformation.TryGetValue(i, out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
                {
                    likes.Add(int.Parse(str));
                }
            }
            foreach (var str in Game1.NPCGiftTastes[npc.Name].Split('/')[3].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectInformation.TryGetValue(i, out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
                {
                    likes.Add(int.Parse(str));
                }
            }
            if (!loves.Any() && !likes.Any())
                return;
            bool loved = true;
            int dish;
            if (loves.Any() && (!likes.Any() || (Game1.random.NextDouble() <= Config.LovedDishChance)))
            {
                dish = loves[Game1.random.Next(loves.Count)];
            }
            else
            {
                loved = false;
                dish = likes[Game1.random.Next(likes.Count)];
            }
            var name = Game1.objectInformation[dish].Split('/')[0];
            int price = 0;
            int.TryParse(Game1.objectInformation[dish].Split('/')[1], out price);
            Monitor.Log($"{npc.Name} is going to order {name}");
            npc.modData[orderKey] = JsonConvert.SerializeObject(new OrderData(dish, name, price, loved));
        }

        private static NetRef<Chest> GetFridge(GameLocation __instance)
        {
            if(__instance is FarmHouse)
            {
                return (__instance as FarmHouse).fridge;
            }
            if(__instance is IslandFarmHouse)
            {
                return (__instance as IslandFarmHouse).fridge;
            }
            __instance.objects.Remove(fridgeHideTile);
            
            if (!fridgeDict.TryGetValue(__instance.Name, out NetRef<Chest> fridge))
            {
                fridge = fridgeDict[__instance.Name] = new NetRef<Chest>(new Chest(true, 130));
            }
            if (Config.AutoFillFridge)
            {
                fridge.Value.items.Clear();
                foreach (var c in __instance.characters)
                {
                    if (c.modData.TryGetValue(orderKey, out string dataString))
                    {
                        OrderData data = JsonConvert.DeserializeObject<OrderData>(dataString);
                        CraftingRecipe r = new CraftingRecipe(data.dishName, true);
                        if (r is not null)
                        {
                            foreach (var key in r.recipeList.Keys)
                            {
                                if (Game1.objectInformation.ContainsKey(key))
                                {
                                    var obj = new Object(key, r.recipeList[key]);
                                    SMonitor.Log($"Adding {obj.Name} ({obj.ParentSheetIndex}) x{obj.Stack} to fridge");
                                    fridge.Value.addItem(obj);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    foreach (var kvp in Game1.objectInformation)
                                    {
                                        string[] objectInfoArray = kvp.Value.Split('/', StringSplitOptions.None);
                                        string[] typeAndCategory = objectInfoArray[3].Split(' ', StringSplitOptions.None);
                                        if (typeAndCategory.Length > 1 && typeAndCategory[1] == key.ToString())
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
                }
            }
            return fridge;
        }
    }
}