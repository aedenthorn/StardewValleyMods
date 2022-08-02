using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace FoodOnTheTable
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry
    {
		private static void NPC_dayUpdate_Postfix(NPC __instance)
		{
			if (!Config.EnableMod)
				return;
			__instance.modData["aedenthorn.FoodOnTheTable/LastFood"] = "0";
		}
		private static void NPC_performTenMinuteUpdate_Postfix(NPC __instance)
		{
			if (!Config.EnableMod || Game1.eventUp || __instance.currentLocation is null || !__instance.isVillager() || !WantsToEat(__instance))
				return;
			PlacedFoodData food = GetClosestFood(__instance, __instance.currentLocation);
			TryToEatFood(__instance, food);
		}

        private static void FarmHouse_updateEvenIfFarmerIsntHere_Postfix(FarmHouse __instance)
        {
            if (!Config.EnableMod || !Game1.IsMasterGame)
                return;
			foreach (NPC npc in __instance.characters)
			{
				if (npc.isVillager())
				{
					NPC villager = npc;
					if (villager != null && WantsToEat(villager) && Game1.random.NextDouble() < Config.MoveToFoodChance / 100f && villager.controller == null && villager.Schedule == null && !villager.getTileLocation().Equals(Utility.PointToVector2(__instance.getSpouseBedSpot(villager.Name))) && __instance.furniture.Count > 0)
					{
						PlacedFoodData food = GetClosestFood(npc, __instance);
						if (food == null)
							return;
						if (TryToEatFood(villager, food))
							return;

						Vector2 possibleLocation = food.foodTile;
						int tries = 0;
						int facingDirection = -3;
						while (tries < 3)
						{
							int xMove = Game1.random.Next(-1, 2);
							int yMove = Game1.random.Next(-1, 2);
							possibleLocation.X += xMove;
							if (xMove == 0)
							{
								possibleLocation.Y += yMove;
							}
							if (xMove == -1)
							{
								facingDirection = 1;
							}
							else if (xMove == 1)
							{
								facingDirection = 3;
							}
							else if (yMove == -1)
							{
								facingDirection = 2;
							}
							else if (yMove == 1)
							{
								facingDirection = 0;
							}
							if (__instance.isTileLocationTotallyClearAndPlaceable(possibleLocation))
							{
								break;
							}
							tries++;
						}
						if (tries < 3)
						{
							SMonitor.Log($"Moving to {possibleLocation}");

							villager.controller = new PathFindController(villager, __instance, new Point((int)possibleLocation.X, (int)possibleLocation.Y), facingDirection, false, false);
						}
					}
				}
			}
		}
    }
}