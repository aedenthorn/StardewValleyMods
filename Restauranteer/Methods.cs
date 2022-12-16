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
                StartOrder(npc, location);
            }
        }

        private void UpdateOrder(NPC npc, OrderData orderData)
        {
            if (!npc.IsEmoting)
            {
                npc.doEmote(424242, false);
            }
        }

        private void StartOrder(NPC npc, GameLocation location)
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
            if (Config.AutoFillFridge)
            {
                FillFridge(location);
            }
        }

        private static NetRef<Chest> GetFridge(GameLocation location)
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
                fridge = fridgeDict[location.Name] = new NetRef<Chest>(new Chest(true, 130));
            }
            return fridge;
        }
        private void FillFridge(GameLocation __instance)
        {
            var fridge = GetFridge(__instance);

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
    }
}