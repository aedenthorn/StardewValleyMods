using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Object = StardewValley.Object;

namespace Restauranteer
{
    public partial class ModEntry
    {
        private void UpdateOrders()
        {
            foreach (var c in Game1.player.currentLocation.characters)
            {

                if (c.IsVillager && !Config.IgnoredNPCs.Contains(c.Name))
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
            if (Game1.random.NextDouble() < Config.OrderChance)
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
            foreach (var str in Game1.NPCGiftTastes["Universal_Love"].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectData.TryGetValue(i.ToString(), out ObjectData data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name))
                {
                    loves.Add(int.Parse(str));
                }
            }
            foreach (var str in Game1.NPCGiftTastes[npc.Name].Split('/')[1].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectData.TryGetValue(i.ToString(), out ObjectData data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name))
                {
                    loves.Add(int.Parse(str));
                }
            }
            List<int> likes = new();
            foreach (var str in Game1.NPCGiftTastes["Universal_Like"].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectData.TryGetValue(i.ToString(), out ObjectData data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name))
                {
                    likes.Add(int.Parse(str));
                }
            }
            foreach (var str in Game1.NPCGiftTastes[npc.Name].Split('/')[3].Split(' '))
            {
                if (int.TryParse(str, out int i) && Game1.objectData.TryGetValue(i.ToString(), out ObjectData data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Name))
                {
                    likes.Add(int.Parse(str));
                }
            }
            if (!loves.Any() && !likes.Any())
                return;
            bool loved = true;
            int dishID;
            if (loves.Any() && (!likes.Any() || (Game1.random.NextDouble() <= Config.LovedDishChance)))
            {
                dishID = loves[Game1.random.Next(loves.Count)];
            }
            else
            {
                loved = false;
                dishID = likes[Game1.random.Next(likes.Count)];
            }
            var name = Game1.objectData[dishID.ToString()].Name;
            int price = Game1.objectData[dishID.ToString()].Price;
            Monitor.Log($"{npc.Name} is going to order {name}");
            npc.modData[orderKey] = JsonConvert.SerializeObject(new OrderData(dishID, name, price, loved));
            if (Config.AutoFillFridge)
            {
                FillFridge(location);
            }
        }

        private static NetRef<Chest> GetFridge(GameLocation location)
        {
            if (location is FarmHouse)
            {
                return (location as FarmHouse).fridge;
            }
            if (location is IslandFarmHouse)
            {
                return (location as IslandFarmHouse).fridge;
            }
            location.objects.Remove(fridgeHideTile);

            if (!fridgeDict.TryGetValue(location.Name, out NetRef<Chest> fridge))
            {
                fridge = fridgeDict[location.Name] = new NetRef<Chest>(new Chest(true, "130"));
            }
            return fridge;
        }
        private void FillFridge(GameLocation __instance)
        {
            var fridge = GetFridge(__instance);
            fridge.Value.Items.Clear();
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
                                    string type = key.Split("/")[1];
                                    // TODO: Double check this 
                                    if (kvp.Value.Type == type)
                                    {
                                        list.Add(kvp.Key.ToString());
                                    }
                                }
                                if (list.Any())
                                {
                                    var obj = new Object(list[Game1.random.Next(list.Count)].ToString(), r.recipeList[key]);
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