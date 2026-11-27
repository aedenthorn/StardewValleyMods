using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace FoodOnTheTable
{
    public partial class ModEntry
    {
		private static bool TryToEatFood(NPC __instance, PlacedFoodData food)
		{
			if (food != null && (__instance.currentLocation is FarmHouse || __instance.currentLocation is Cabin || !Config.OnlyInFarmhouse) && Vector2.Distance(food.foodTile, __instance.Tile) < Config.MaxDistanceToEat)
			{
				SMonitor.Log($"eating {food.foodObject.Name} at {food.foodTile}");
				using (IEnumerator<Furniture> enumerator = __instance.currentLocation.furniture.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.boundingBox.Value != food.furniture.boundingBox.Value)
							continue;
						if (food.slot > -1)
						{
							enumerator.Current.modData.Remove("aedenthorn.FurnitureDisplayFramework/" + food.slot);
							SMonitor.Log($"ate food at slot {food.slot} in {enumerator.Current.Name}");
						}
						else
						{
							enumerator.Current.heldObject.Value = null;
							SMonitor.Log($"ate held food in {enumerator.Current.Name}");
						}

						if (__instance.currentLocation is FarmHouse)
						{
							Farmer owner = (__instance.currentLocation as FarmHouse).owner;

							if (owner.friendshipData.ContainsKey(__instance.Name) && (owner.friendshipData[__instance.Name].IsMarried() || owner.friendshipData[__instance.Name].IsRoommate()))
							{
								int points = 80;
								switch (food.value)
								{
									case 1:
										points = 45;
										break;
									case 2:
										points = 20;
										break;
									default:
										__instance.doEmote(20);
										break;
								}
								owner.friendshipData[__instance.Name].Points += (int)(points * Config.PointsMult);
                                if (Config.CountAsFedSpouse && SHelper.ModRegistry.IsLoaded("spacechase0.AnotherHungerMod"))
								{
									owner.modData["spacechase0.AnotherHungerMod/FedSpouse"] = "true";
                                }
								SMonitor.Log($"Friendship with {owner.Name} increased by {(int)(points * Config.PointsMult)} points!");
							}
						}
						__instance.modData["aedenthorn.FoodOnTheTable/LastFood"] = Game1.timeOfDay.ToString();
						return true;
					}
				}
			}
			return false;
		}
		private static PlacedFoodData GetClosestFood(NPC npc, GameLocation location)
		{
			if (location is not FarmHouse && location is not Cabin && Config.OnlyInFarmhouse)
				return null;
			List<PlacedFoodData> foodList = new List<PlacedFoodData>();
			foreach (var f in location.furniture)
			{
				if (f.heldObject.Value != null && f.heldObject.Value.Edibility > 0)
				{
					for (int x = f.boundingBox.X / 64; x < (f.boundingBox.X + f.boundingBox.Width) / 64; x++)
					{
						for (int y = f.boundingBox.Y / 64; y < (f.boundingBox.Y + f.boundingBox.Height) / 64; y++)
						{
							foodList.Add(new PlacedFoodData(f, new Vector2(x, y), f.heldObject.Value, -1));
						}
					}
				}
				if (fdfAPI != null)
				{
                    List<Object> objList = fdfAPI.GetSlotObjects(f);
					if (objList is null || objList.Count == 0)
						continue;
					for (int i = 0; i < objList.Count; i++)
					{
                        if (objList[i] is not null && objList[i].Edibility > 0)
                        {
                            var slotRect = fdfAPI.GetSlotRect(f, i);
                            if (slotRect != null)
                                foodList.Add(new PlacedFoodData(f, new Vector2((f.boundingBox.X + slotRect.Value.X) / 64, (f.boundingBox.Y + slotRect.Value.Y) / 64), objList[i], i));
                        }
                    }
				}
			}
			if (foodList.Count == 0)
			{
				//SMonitor.Log("Got no food");
				return null;
			}
			List<string> favList = new List<string>(Game1.NPCGiftTastes["Universal_Love"].Split(' '));
			List<string> likeList = new List<string>(Game1.NPCGiftTastes["Universal_Like"].Split(' '));
			List<string> okayList = new List<string>(Game1.NPCGiftTastes["Universal_Neutral"].Split(' '));

			if (Game1.NPCGiftTastes.TryGetValue(npc.Name, out string NPCLikes) && NPCLikes != null)
			{
				favList.AddRange(NPCLikes.Split('/')[1].Split(' '));
				likeList.AddRange(NPCLikes.Split('/')[3].Split(' '));
				okayList.AddRange(NPCLikes.Split('/')[5].Split(' '));
			}
			for (int i = foodList.Count - 1; i >= 0; i--)
			{
				if (favList.Contains(foodList[i].foodObject.ParentSheetIndex + ""))
				{
					foodList[i].value = 3;
				}
				else
				{
					if (likeList.Contains(foodList[i].foodObject.ParentSheetIndex + ""))
					{
						foodList[i].value = 2;
					}
					else
					{
						if (okayList.Contains(foodList[i].foodObject.ParentSheetIndex + ""))
						{
							foodList[i].value = 1;
						}
						else
							foodList.RemoveAt(i);
					}
				}
			}
			if (foodList.Count == 0)
			{
				//SMonitor.Log("Got no food");
				return null;
			}

			foodList.Sort(delegate (PlacedFoodData a, PlacedFoodData b)
			{
				var compare = b.value.CompareTo(a.value);
				if (compare != 0)
					return compare;
				return (Vector2.Distance(a.foodTile, npc.Tile).CompareTo(Vector2.Distance(b.foodTile, npc.Tile)));
			});

			SMonitor.Log($"Got {foodList.Count} possible food for {npc.Name}; best: {foodList[0].foodObject.Name} at {foodList[0].foodTile}, value {foodList[0].value}");
			return foodList[0];
		}
		private static Item GetObjectFromID(string id, int amount, int quality)
		{
            return ItemRegistry.Create(id, amount, quality, true);
        }
        private static bool WantsToEat(NPC spouse)
		{
			if (!spouse.modData.TryGetValue("aedenthorn.FoodOnTheTable/LastFood", out var str) || string.IsNullOrEmpty(str))
			{
				return true;
			}

			return GetMinutes(Game1.timeOfDay) - GetMinutes(int.Parse(spouse.modData["aedenthorn.FoodOnTheTable/LastFood"])) > Config.MinutesToHungry;
		}

		private static int GetMinutes(int timeOfDay)
		{
			return timeOfDay % 100 + timeOfDay / 100 * 60;
		}
	}

}