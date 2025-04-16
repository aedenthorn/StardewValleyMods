using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace CustomStarterPackage
{
	public partial class ModEntry
	{
		public static void ReplaceDefaultStarterPackage()
		{
			dataDictionary = Game1.content.Load<Dictionary<string, StarterItemData>>(dictionaryPath);
			SMonitor.Log($"Loaded {dataDictionary.Count} items from content patcher.");
			foreach ((Vector2 position, Object obj) in Game1.player.currentLocation.objects.Pairs)
			{
				if (obj is Chest chest && chest.giftbox.Value && chest.giftboxIsStarterGift.Value)
				{
					SMonitor.Log($"Found starter chest at {position}. Replacing...");
					Inventory items = new();

					foreach ((string id, StarterItemData starterItemData) in dataDictionary)
					{
						if (starterItemData.FarmTypes is not null)
						{
							if (starterItemData.FarmTypes.All(farmType => farmType.StartsWith('!')))
							{
								if (starterItemData.FarmTypes.Any(farmType => farmType[1..].Equals(Game1.GetFarmTypeKey())))
								{
									continue;
								}
							}
							else
							{
								if (!starterItemData.FarmTypes.Contains(Game1.GetFarmTypeKey()))
								{
									continue;
								}
							}
						}
						if (starterItemData.ChancePercent < Game1.random.Next(100))
						{
							continue;
						}
						SMonitor.Log($"Adding {id}...");

						Item item;
						string itemId;

						switch (starterItemData.Type)
						{
							case "Object":
								int amount = (starterItemData.MinAmount < starterItemData.MaxAmount) ? Game1.random.Next(starterItemData.MinAmount, starterItemData.MaxAmount + 1) : starterItemData.MinAmount;
								int quality = (starterItemData.MinQuality < starterItemData.MaxQuality) ? Game1.random.Next(starterItemData.MinQuality, starterItemData.MaxQuality + 1) : starterItemData.MinQuality;

								if ((itemId = GetItemIdForNewDataModel(starterItemData.Type, Game1.objectData, starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = new Object(itemId, amount, quality: quality);
								break;
							case "BigCraftable":
							case "Chest":
								if ((itemId = GetItemIdForNewDataModel(starterItemData.Type, Game1.bigCraftableData, starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = new Object(Vector2.Zero, itemId, false);
								break;
							case "Hat":
								if ((itemId = GetItemIdForOldDataModel(starterItemData.Type, SHelper.GameContent.Load<Dictionary<string, string>>("Data/hats"), starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = new Hat(itemId);
								break;
							case "Boots":
								if ((itemId = GetItemIdForOldDataModel(starterItemData.Type, SHelper.GameContent.Load<Dictionary<string, string>>("Data/Boots"), starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = new Boots(itemId);
								break;
							case "Ring":
								if ((itemId = GetItemIdForNewDataModel(starterItemData.Type, Game1.objectData, starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = new Ring(itemId);
								break;
							case "Clothing":
								if ((itemId = GetItemIdForNewDataModel(starterItemData.Type, Game1.shirtData, starterItemData.NameOrIndex, false)) is null && (itemId = GetItemIdForNewDataModel(starterItemData.Type, Game1.pantsData, starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = new Clothing(itemId);
								break;
							case "Furniture":
								if ((itemId = GetItemIdForOldDataModel(starterItemData.Type, SHelper.GameContent.Load<Dictionary<string, string>>("Data/Furniture"), starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = Furniture.GetFurnitureInstance(itemId, Vector2.Zero);
								break;
							case "Tool":
							case "Axe":
							case "FishingRod":
							case "Hoe":
							case "Pan":
							case "Pickaxe":
							case "Shears":
							case "WateringCan":
								if ((itemId = GetItemIdForNewDataModel(starterItemData.Type, Game1.toolData, starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = ItemRegistry.Create("(T)" + itemId, 1, 0, true);
								if (item is WateringCan wateringCan)
								{
									wateringCan.WaterLeft = 0;
								}
								break;
							case "MeleeWeapon":
								if ((itemId = GetItemIdForNewDataModel(starterItemData.Type, Game1.weaponData, starterItemData.NameOrIndex)) is null)
								{
									continue;
								}
								item = new MeleeWeapon(itemId);
								break;
							default:
								SMonitor.Log($"Object type {starterItemData.Type} not recognized", LogLevel.Warn);
								continue;
						}
						if (item is not null)
						{
							items.Add(item);
						}
						else
						{
							SMonitor.Log($"Object {id} not recognized.", LogLevel.Warn);
						}
					}
					if (items.Count > 0)
					{
						chest.Items.Clear();
						chest.Items.AddRange(items);
						chest.dropContents.Value = true;
						obj.modData[chestKey] = "true";
						SMonitor.Log($"Added {items.Count} items to starter package.", LogLevel.Info);
						return;
					}
					SMonitor.Log($"No items added to the starter package (The default starter package will be used).", LogLevel.Info);
					return;
				}
			}
		}

		private static string GetItemIdForOldDataModel(string type, Dictionary<string, string> data, string nameOrId, bool log = true)
		{
			if (!data.TryGetValue(nameOrId, out _))
			{
				try
				{
					return data.First(entry => entry.Value.StartsWith(nameOrId + '/')).Key;
				}
				catch
				{
					if (log)
					{
						SMonitor.Log($"{type} {nameOrId} not found.", LogLevel.Warn);
					}
					return null;
				}
			}
			return nameOrId;
		}

		private static string GetItemIdForNewDataModel<T>(string type, IDictionary<string, T> data, string nameOrId, bool log = true)
		{
			if (!data.TryGetValue(nameOrId, out _))
			{
				try
				{
					return data.First(entry => typeof(T).GetField("Name").GetValue(entry.Value).Equals(nameOrId)).Key;
				}
				catch
				{
					if (log)
					{
						SMonitor.Log($"{type} {nameOrId} not found.", LogLevel.Warn);
					}
					return null;
				}
			}
			return nameOrId;
		}
	}
}
