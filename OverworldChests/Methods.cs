using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace OverworldChests
{
	public partial class ModEntry
	{
		public static void UpdateTreasuresList()
		{
			treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
		}

		public static void SpawnChests(bool ignoreModEnabled = false)
		{
			if (!Config.ModEnabled && !ignoreModEnabled)
				return;

			Utility.ForEachLocation(location =>
			{
				if (location is not FarmHouse && (Config.IncludeIndoorLocations || location.IsOutdoors) && IsLocationAllowed(location))
				{
					int width = location.map.Layers[0].LayerWidth;
					int height = location.map.Layers[0].LayerHeight;

					bool IsTileFree(Vector2 position)
					{
						return location.CanItemBePlacedHere(position) && !location.isWaterTile((int)position.X, (int)position.Y) && !location.isCropAtTile((int)position.X, (int)position.Y);
					}

					int freeTiles = Enumerable.Range(0, width * height).Count(i => IsTileFree(new Vector2(i % width, i / width)));
					int spawnedChestCount = Math.Min(freeTiles, (int)Math.Floor(freeTiles * Config.ChestDensity) + (Config.RoundNumberOfChestsUp ? 1 : 0));
					int i = 0;

					while (i < spawnedChestCount)
					{
						Vector2 freeTile = location.getRandomTile();

						if (IsTileFree(freeTile))
						{
							double fraction = Math.Pow(random.NextDouble(), 1 / Config.RarityChance);
							int level = (int)Math.Ceiling(fraction * Config.Mult);
							Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, level, Config.IncreaseRate, Config.ItemsBaseMaxValue, freeTile);

							chest.playerChoiceColor.Value = MakeTint(fraction);
							chest.modData.Add(modKey, "T");
							chest.modData.Add(modCoinKey, advancedLootFrameworkApi.GetChestCoins(level, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax).ToString());
							location.overlayObjects[freeTile] = chest;
							i++;
						}
					}
					SMonitor.Log($"Spawned {spawnedChestCount} chests in location {location.NameOrUniqueName}");
				}
				return true;
			});
		}

		public static void RemoveChests(bool ignoreModEnabled = false)
		{
			if (!Config.ModEnabled && !ignoreModEnabled)
				return;

			Utility.ForEachLocation(location =>
			{
				if (location is not FarmHouse)
				{
					List<Vector2> objectsToRemove = new();
					List<Vector2> overlayObjectsToRemove = new();

					foreach (Dictionary<Vector2, Object> dictionary in location.objects)
					{
						foreach ((Vector2 key, Object value) in dictionary)
						{
							if (value is Chest chest && chest.modData.ContainsKey(modKey))
							{
								objectsToRemove.Add(key);
							}
						}
					}
					foreach ((Vector2 key, Object value) in location.overlayObjects)
					{
						if (value is Chest chest && chest.modData.ContainsKey(modKey))
						{
							overlayObjectsToRemove.Add(key);
						}
					}
					foreach (Vector2 tilelocation in objectsToRemove)
					{
						location.objects.Remove(tilelocation);
					}
					foreach (Vector2 position in overlayObjectsToRemove)
					{
						location.overlayObjects.Remove(position);
					}
					SMonitor.Log($"Removed {objectsToRemove.Count + overlayObjectsToRemove.Count} chests in location {location.NameOrUniqueName}");
				}
				return true;
			});
		}

		public static void RespawnChests(bool ignoreModEnabled = false)
		{
			RemoveChests(ignoreModEnabled);
			SpawnChests(ignoreModEnabled);
		}

		public static bool IsLocationAllowed(GameLocation location)
		{
			if (Config.OnlyAllowLocations.Length > 0)
			{
				return Config.OnlyAllowLocations.Split(',').Contains(location.Name);
			}
			return !Config.DisallowLocations.Split(',').Contains(location.Name);
		}

		public static Color MakeTint(double fraction)
		{
			return tintColors[(int)Math.Floor(fraction * tintColors.Length)];
		}
	}
}
