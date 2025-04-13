using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace CustomStarterFurniture
{
	public partial class ModEntry
	{
		private static bool IsFarm(int farmType)
		{
			return Game1.whichFarm == farmType || farmType < 0;
		}

		private static Furniture GetFurniture(string nameOrIndex, int x, int y)
		{
			string itemId = GetFurnitureItemId(nameOrIndex);

			if (itemId is not null)
			{
				return Furniture.GetFurnitureInstance(itemId, new Vector2(x, y));
			}
			SMonitor.Log($"Furniture {nameOrIndex} not found", LogLevel.Warn);
			return null;
		}

		private static Object GetBigCraftable(string nameOrIndex, int x, int y)
		{
			string itemId = GetBigCraftableItemId(nameOrIndex);

			if (itemId is not null)
			{
				return new Object(new Vector2(x, y), itemId);
			}
			SMonitor.Log($"BigCraftable {nameOrIndex} not found", LogLevel.Warn);
			return null;
		}

		private static string GetObjectItemId(string nameOrIndex)
		{
			IDictionary<string, ObjectData> dictionary = Game1.objectData;

			foreach (KeyValuePair<string, ObjectData> data in dictionary)
			{
				if (data.Key == $"(O){nameOrIndex}" || data.Key == nameOrIndex || data.Value.Name == nameOrIndex)
				{
					return data.Key;
				}
			}
			return null;
		}

		private static string GetFurnitureItemId(string nameOrIndex)
		{
			IDictionary<string, string> dictionary = SHelper.GameContent.Load<Dictionary<string, string>>("Data/Furniture");

			foreach (KeyValuePair<string, string> data in dictionary)
			{
				if (data.Key == $"(F){nameOrIndex}" || data.Key == nameOrIndex || data.Value.Split('/')[0] == nameOrIndex)
				{
					return data.Key;
				}
			}
			return null;
		}

		private static string GetBigCraftableItemId(string nameOrIndex)
		{
			IDictionary<string, BigCraftableData> dictionary = Game1.bigCraftableData;

			foreach (KeyValuePair<string, BigCraftableData> data in dictionary)
			{
				if (data.Key == $"(BC){nameOrIndex}" || data.Key == nameOrIndex || data.Value.Name == nameOrIndex)
				{
					return data.Key;
				}
			}
			return null;
		}

		private static Object GetObject(string nameOrIndex, string itemType)
		{
			switch (itemType)
			{
				case "Object":
					string objectItemId = GetObjectItemId(nameOrIndex);

					if (objectItemId is not null)
					{
						return new Object(objectItemId, 1);
					}
					break;
				case "Furniture":
					string furnitureItemId = GetFurnitureItemId(nameOrIndex);

					if (furnitureItemId is not null)
					{
						return Furniture.GetFurnitureInstance(furnitureItemId, Vector2.Zero);
					}
					break;
				default:
					SMonitor.Log($"Object type {itemType} not recognized", LogLevel.Warn);
					break;
			}
			return null;
		}
	}
}
