using Newtonsoft.Json;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace Restauranteer
{
    public partial class ModEntry
    {
        private void UpdateOrders()
        {
            foreach(var c in Game1.player.currentLocation.characters)
            {

                if (c.isVillager() && c.Name != "Gus" && c.Name != "Emily")
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
                if (Game1.objectInformation.TryGetValue(int.Parse(str), out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
                {
                    loves.Add(int.Parse(str));
                }
            }
            foreach(var str in Game1.NPCGiftTastes[npc.Name].Split('/')[1].Split(' '))
            {
                if (Game1.objectInformation.TryGetValue(int.Parse(str), out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
                {
                    loves.Add(int.Parse(str));
                }
            }
            List<int> likes = new();
            foreach(var str in Game1.NPCGiftTastes["Universal_Like"].Split(' '))
            {
                if (Game1.objectInformation.TryGetValue(int.Parse(str), out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
                {
                    likes.Add(int.Parse(str));
                }
            }
            foreach (var str in Game1.NPCGiftTastes[npc.Name].Split('/')[3].Split(' '))
            {
                if (Game1.objectInformation.TryGetValue(int.Parse(str), out string data) && CraftingRecipe.cookingRecipes.ContainsKey(data.Split('/')[0]))
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
            Monitor.Log($"{npc.Name} is going to order {name}");
            npc.modData[orderKey] = JsonConvert.SerializeObject(new OrderData(dish, name, loved));
        }
    }
}