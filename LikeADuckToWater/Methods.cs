using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Pathfinding;

namespace LikeADuckToWater
{
	public partial class ModEntry
	{
		private static bool NotReadyToSwim(FarmAnimal animal)
		{
			return !Config.ModEnabled || animal.controller is not null || !animal.currentLocation.IsOutdoors || animal.currentLocation.waterTiles is null || !animal.CanSwim() || animal.isSwimming.Value || (!animal.wasPet.Value && (!Config.SwimAfterAutoPet || !animal.wasAutoPet.Value)) || (Config.EatBeforeSwimming && animal.fullness.Value < 195) || ducksToCheck.ContainsKey(animal) || animal.modData.ContainsKey(swamTodayKey);
		}

		private bool CheckDuck(FarmAnimal animal, HopInfo info)
		{
			if (animal is null || info is null || !animal.currentLocation.IsOutdoors || pickedTiles.Contains(info.hopTile) || FarmAnimal.NumPathfindingThisTick >= FarmAnimal.MaxPathfindingPerTick)
				return false;

			foreach(FarmAnimal farmAnimal in animal.currentLocation.Animals.Values)
			{
				if (farmAnimal is not null && farmAnimal.GetBoundingBox().Intersects(info.hoppedBox))
				{
					return false;
				}
			}

			PathFindController controller = new(animal, animal.currentLocation, new Point((int)info.hopTile.X, (int)info.hopTile.Y), info.dir, new PathFindController.endBehavior(TryHop), 200);

			FarmAnimal.NumPathfindingThisTick++;
			if (controller.pathToEndPoint is not null)
			{
				pickedTiles.Add(info.hopTile);
				animal.controller = controller;
				SMonitor.Log($"{animal.displayName} is travelling from {animal.Tile} to {info.hopTile} to swim");
				return true;
			}
			return false;
		}

		private static void DoHop(FarmAnimal animal, Vector2 key)
		{
			animal.Position = key * 64;
			animal.isSwimming.Value = true;
			animal.hopOffset = hopTileDictionary[animal.home.GetParentLocation()][key][0].offset;
			animal.pauseTimer = 0;
			SwamToday(animal);
			SMonitor.Log($"{animal.displayName} is hopping into the water");
		}

		private static bool IsCollidingWater(GameLocation __instance, Character character, int x, int y)
		{
			if (!Config.ModEnabled || !__instance.IsOutdoors || __instance.waterTiles is null || character is not FarmAnimal || !(character as FarmAnimal).CanSwim() || !(character as FarmAnimal).isSwimming.Value)
				return false;

			return __instance.isOpenWater(x, y);
		}

		private static void TryHop(Character character, GameLocation location)
		{
			if (!Config.ModEnabled || !hopTileDictionary.ContainsKey(location) || !hopTileDictionary[location].Any())
				return;

			Vector2 center = character.GetBoundingBox().Center.ToVector2();
			List<Vector2> hopTiles = hopTileDictionary[location].Keys.ToList();

			hopTiles.Sort(delegate (Vector2 a, Vector2 b)
			{
				return Vector2.Distance((a + new Vector2(0.5f, 0.5f)) * 64, center).CompareTo(Vector2.Distance((b + new Vector2(0.5f, 0.5f)) * 64, center));
			});
			foreach (Vector2 hopTile in hopTiles)
			{
				if (Vector2.Distance(center, (hopTile + new Vector2(0.5f, 0.5f)) * 64) < 64)
				{
					DoHop((FarmAnimal) character, hopTile);
					return;
				}
			}
		}

		private static void TryAddToQueue(FarmAnimal animal, GameLocation location)
		{
			if (!Config.ModEnabled || !hopTileDictionary.ContainsKey(location) || !hopTileDictionary[location].Any())
				return;

			Vector2 center = animal.GetBoundingBox().Center.ToVector2();
			List<Vector2> hopTiles = hopTileDictionary[location].Keys.ToList();

			hopTiles.Sort(delegate (Vector2 a, Vector2 b)
			{
				return Vector2.Distance((b + new Vector2(0.5f, 0.5f)) * 64, center).CompareTo(Vector2.Distance((a + new Vector2(0.5f, 0.5f)) * 64, center));
			});

			Stack<HopInfo> stack = new();

			foreach (Vector2 hopTile in hopTiles)
			{
				float distance = Vector2.Distance(center, (hopTile + new Vector2(0.5f, 0.5f)) * 64);

				if (distance < 64)
				{
					DoHop(animal, hopTile);
					return;
				}
				else if(distance / 64 > Config.MaxDistance)
				{
					continue;
				}
				foreach(HopInfo hopInfo in hopTileDictionary[location][hopTile])
				{
					stack.Push(hopInfo);
				}
			}
			ducksToCheck[animal] = stack;
		}

		private static void RebuildHopSpots(GameLocation location)
		{
			if (!Config.ModEnabled || !location.IsOutdoors || location.waterTiles is null)
				return;

			Stopwatch s = new();
			s.Start();

			long id = Game1.Multiplayer.getNewID();
			FarmAnimal animal = new("Duck", id, Game1.player.UniqueMultiplayerID);

			hopTileDictionary[location] = new();
			for (int y = 0; y < location.map.Layers[0].LayerHeight; y++)
			{
				for (int x = 0; x < location.map.Layers[0].LayerWidth; x++)
				{
					if(location.isWaterTile(x, y) && location.doesTileHaveProperty(x, y, "Passable", "Back") != null)
					{
						location.removeTileProperty(x, y, "Back", "Passable");
					}
					animal.Position = new Vector2(x, y) * 64;
					if (!location.waterTiles.waterTiles[x,y].isWater && !location.isCollidingPosition(animal.GetBoundingBox(), Game1.viewport, false, 0, false, animal, false, false, false))
					{
						List<HopInfo> hoppableDirs = GetHoppableDirs(location, animal);
						if(hoppableDirs.Any())
						{
							hopTileDictionary[location].Add(new Vector2(x, y), hoppableDirs);
						}
					}
				}
			}
			s.Stop();
			SMonitor.Log($"Rebuild of {location.NameOrUniqueName} took {s.ElapsedMilliseconds}ms");
			SMonitor.Log($"Got {hopTileDictionary[location].Count} hoppable tiles");
		}

		private static List<HopInfo> GetHoppableDirs(GameLocation location, FarmAnimal animal)
		{
			List<HopInfo> list = new();
			Point tile = animal.TilePoint;

			for(int i = 0; i < 4; i++)
			{
				Vector2 offset = Utility.getTranslatedVector2(Vector2.Zero, i, 1f);
				Point offsetPoint = Utility.Vector2ToPoint(offset);

				if (offset != Vector2.Zero)
				{
					Point hop_over_tile = tile + offsetPoint;
					Point hop_tile = hop_over_tile + offsetPoint;
					Rectangle hop_destination = animal.GetBoundingBox();

					hop_destination.Offset(offset * 128);
					if (location.isWaterTile(hop_over_tile.X, hop_over_tile.Y) && location.doesTileHaveProperty(hop_over_tile.X, hop_over_tile.Y, "Passable", "Buildings") == null && !location.isCollidingPosition(hop_destination, Game1.viewport, false, 0, false, animal) && (!IsCollidingWater(location, animal, hop_destination.X / 64, hop_destination.Y / 64) || location.isOpenWater(hop_tile.X, hop_tile.Y)))
					{
						list.Add(new HopInfo() {
							hopTile = tile.ToVector2(),
							dir = i,
							offset = offset * 128,
							hoppedBox = hop_destination
						});
					}
				}
			}
			return list;
		}

		private static void SwamToday(FarmAnimal animal)
		{
			if (!animal.modData.ContainsKey(swamTodayKey) && Config.FriendshipGain > 0)
			{
				animal.friendshipTowardFarmer.Value = Math.Min(1000, animal.friendshipTowardFarmer.Value + Config.FriendshipGain);
			}
			animal.modData[swamTodayKey] = "true";
		}
	}
}
