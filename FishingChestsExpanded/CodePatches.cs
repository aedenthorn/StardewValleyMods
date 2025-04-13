using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;

namespace FishingChestsExpanded
{
	public partial class ModEntry
	{
		public class ItemGrabMenu_Patch
		{
			public static void Prefix(ref IList<Item> inventory, object context)
			{
				if (!Config.EnableMod || context is not FishingRod)
					return;

				FishingRod fishingRod = context as FishingRod;
				bool treasure = false;

				foreach (Item item in inventory)
				{
					if (item.ItemId != fishingRod.whichFish.LocalItemId)
						treasure = true;
				}
				if (!treasure)
					return;

				Dictionary<string, string> data = Game1.content.Load<Dictionary<string, string>>("Data\\Fish");
				int difficulty = 5;

				if(data.ContainsKey(fishingRod.whichFish.LocalItemId))
				{
					_ = int.TryParse(data[fishingRod.whichFish.LocalItemId].Split('/')[1], out difficulty);
				}

				int coins = advancedLootFrameworkApi.GetChestCoins(difficulty, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax);

				IList<Item> items = advancedLootFrameworkApi.GetChestItems(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, difficulty, Config.IncreaseRate, Config.ItemsBaseMaxValue);
				bool vanilla = Game1.random.NextDouble() < Config.VanillaLootChance / 100f;
				string[] array = !string.IsNullOrEmpty(Config.AlwaysIncludeItems) ? Config.AlwaysIncludeItems.Split(',') : Array.Empty<string>();

				foreach (Item item in inventory)
				{
					ParsedItemData itemData = ItemRegistry.GetData(item.ItemId);

					if (item.ItemId.Equals(fishingRod.whichFish.LocalItemId) || item.ItemId.Equals("890") || item.ItemId.Equals("GoldenBobber") || item.ItemId.Equals("TroutDerbyTag") || vanilla || array.Any(text => item.Name.Equals(text)) || (Config.AlwaysIncludeRoe && item.ItemId.Equals("812")) || (Config.AlwaysIncludeBooks && item.HasContextTag("book_item")) || (Config.AlwaysIncludeGeodes && Utility.IsGeode(item)) || (Config.AlwaysIncludeArtifacts && itemData.HasTypeObject() && itemData.ObjectType.Equals("Arch")))
					{
						items.Add(item);
					}
				}
				inventory = items;
				Game1.player.Money += coins;
				SMonitor.Log($"chest contains {coins} gold");
			}
		}

		public class FishingRod_startMinigameEndFunction_Patch
		{
			public static void Prefix()
			{
				if (!Config.EnableMod)
					return;

				if(Config.ChanceForTreasureChest >= 0)
				{
					FishingRod.baseChanceForTreasure = Config.ChanceForTreasureChest / 100f;
				}
			}
		}
	}
}
