using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace AnimatedParrotAndPerch
{
	public partial class ModEntry : Mod
	{
		private static readonly Vector2 jungleParrotPerchOffset = new(12, -104);
		private static readonly Vector2 stoneParrotPerchOffset = new(14, -104);
		private static readonly Vector2 woodenParrotPerchOffset = new(18, -104);

		private static bool IsPerch(Object obj)
		{
			return IsPerch(obj, out _);
		}

		private static bool IsPerch(Object obj, out Vector2 offset)
		{
			if (obj.name.Equals("Jungle Parrot Perch"))
			{
				offset = jungleParrotPerchOffset;
				return true;
			}
			else if (obj.name.Equals("Stone Parrot Perch"))
			{
				offset = stoneParrotPerchOffset;
				return true;
			}
			else if (obj.name.Equals("Wooden Parrot Perch"))
			{
				offset = woodenParrotPerchOffset;
				return true;
			}
			else
			{
				offset = Vector2.Zero;
				return false;
			}
		}

		private static Item GetRandomParrotGift(Object obj)
		{
			return advancedLootFrameworkApi.GetChestItems(giftList, possibleGifts, 1, 1, 100, obj.Price > 0 ? obj.Price : 1, 0.2f, 100)[0];
		}

		private static void ShowParrots(GameLocation location, Object excluded = null)
		{
			context.Monitor.Log($"Showing perch parrots for {location.Name}");
			for (int i = location.TemporarySprites.Count - 1; i >= 0; i--)
			{
				if (location.TemporarySprites[i] is PerchParrot)
				{
					location.TemporarySprites.RemoveAt(i);
				}
			}
			foreach (KeyValuePair<Vector2, Object> kvp in location.objects.Pairs)
			{
				if (IsPerch(kvp.Value, out Vector2 offset) && kvp.Value != excluded)
				{
					context.Monitor.Log($"Showing parrot for tile {kvp.Key}");
					location.temporarySprites.Add(new PerchParrot(kvp.Key * 64 + offset, kvp.Value.TileLocation));
				}
			}
		}
	}
}
