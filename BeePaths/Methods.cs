using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace BeePaths
{
	public partial class ModEntry
	{
		private static void AddHiveDataToDictionary(GameLocation location, Vector2 key, Dictionary<Vector2, HiveData> dictionary)
		{
			Crop crop = Utility.findCloseFlower(location, key, Config.BeeRange, (Crop crop) => !crop.forageCrop.Value);

			if (crop is not null)
			{
				HiveData hiveData = new(key, crop.tilePosition, location.getObjectAtTile((int)crop.tilePosition.X, (int)crop.tilePosition.Y) is IndoorPot);

				ResetHiveDataBees(hiveData);
				dictionary[key] = hiveData;
			}
		}

		private static void AddDictionaryToHives(string key, Dictionary<Vector2, HiveData> dictionary)
		{
			if (dictionary.Any())
			{
				hives[key] = dictionary;
			}
		}

		private static void ResetHiveDataBees(HiveData hiveData)
		{
			hiveData.bees.Clear();
			while (hiveData.bees.Count < Config.NumberBees)
			{
				hiveData.bees.Add(Game1.random.NextDouble() < 0.5 ? GetBee(hiveData.cropTile, hiveData.hiveTile, false) : GetBee(hiveData.hiveTile, hiveData.cropTile, true));
			}
		}

		private static void ResetHives()
		{
			hives.Clear();
			foreach (GameLocation location in Game1.locations)
			{
				Dictionary<Vector2, HiveData> dictionary = new();

				foreach (var kvp in location.Objects.Pairs)
				{
					if(kvp.Value.Name.Equals("Bee House"))
					{
						AddHiveDataToDictionary(location, kvp.Key, dictionary);
					}
				}
				AddDictionaryToHives(location.NameOrUniqueName, dictionary);
			}
		}

		private static void UpdateDictionaryWhenCropRemoveAtTile(GameLocation location, Vector2 tilePosition, Dictionary<Vector2, HiveData> dictionary)
		{
			if (location.GetHoeDirtAtTile(tilePosition) is HoeDirt hoeDirt)
			{
				hoeDirt.crop = null;
			}
			foreach(var kvp in dictionary)
			{
				if (tilePosition == kvp.Value.cropTile)
				{
					Crop crop = Utility.findCloseFlower(location, kvp.Key, Config.BeeRange, (Crop crop) => !crop.forageCrop.Value);

					if(crop is null)
					{
						dictionary.Remove(kvp.Key);
					}
					else
					{
						dictionary[kvp.Key].cropTile = crop.tilePosition;
						dictionary[kvp.Key].isIndoorPot = location.getObjectAtTile((int)crop.tilePosition.X, (int)crop.tilePosition.Y) is IndoorPot;
						ResetHiveDataBees(dictionary[kvp.Key]);
					}
				}
			}
		}

		private static BeeData GetBee(Vector2 startTile, Vector2 endTile, bool IsGoingToFlower, bool random = true)
		{
			Vector2 startPosition = startTile * 64 + new Vector2(Game1.random.Next(64), Game1.random.Next(64) - 32);
			Vector2 endPosition = endTile * 64 + new Vector2(Game1.random.Next(64), Game1.random.Next(64) - 32);
			Vector2 position = random ? Vector2.Lerp(startPosition, endPosition, (float)Game1.random.NextDouble()) : startPosition;

			return new BeeData()
			{
				startPosition = startPosition,
				endPosition = endPosition,
				position = position,
				isGoingToFlower = IsGoingToFlower
			};
		}

		public static bool IsOnScreen(Vector2 point1, Vector2 point2, int acceptableDistanceFromScreen)
		{
			if (Utility.isOnScreen(point1, acceptableDistanceFromScreen) || Utility.isOnScreen(point2, acceptableDistanceFromScreen))
				return true;

			return LineIntersectsRectangle(point1, point2, GetScreenBounds(acceptableDistanceFromScreen));
		}

		private static Rectangle GetScreenBounds(int acceptableDistanceFromScreen)
		{
			xTile.Dimensions.Rectangle viewport = Game1.viewport;

			return new Rectangle(viewport.X - acceptableDistanceFromScreen, viewport.Y - acceptableDistanceFromScreen, viewport.Width + 2 * acceptableDistanceFromScreen, viewport.Height + 2 * acceptableDistanceFromScreen);
		}

		private static bool LineIntersectsRectangle(Vector2 point1, Vector2 point2, Rectangle rectangle)
		{
			return LineIntersectsLine(point1, point2, new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top)) || LineIntersectsLine(point1, point2, new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom)) || LineIntersectsLine(point1, point2, new Vector2(rectangle.Right, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Bottom)) || LineIntersectsLine(point1, point2, new Vector2(rectangle.Left, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Top));
		}

		private static bool LineIntersectsLine(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2)
		{
			float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
			float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

			if (d == 0)
			{
				return false;
			}

			float r = q / d;

			q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);

			float s = q / d;

			return r >= 0 && r <= 1 && s >= 0 && s <= 1;
		}
	}
}
