using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FloatingGardenPots
{
	public partial class ModEntry
	{
		private static Vector2 GetPotOffset(GameLocation location, Vector2 tileLocation)
		{
			if (!offsetDictionary.TryGetValue(location, out Dictionary<Vector2, Vector2> dictionary))
			{
				dictionary = new();
				offsetDictionary[location] = dictionary;
			}
			if (!dictionary.TryGetValue(tileLocation, out Vector2 offset))
			{
				offset = Vector2.Zero;
				if (CheckLocation(location, tileLocation.X - 1f, tileLocation.Y))
				{
					offset += new Vector2(32f, 0f);
				}
				if (CheckLocation(location, tileLocation.X + 1f, tileLocation.Y))
				{
					offset += new Vector2(-32f, 0f);
				}
				if (offset.X != 0f && CheckLocation(location, tileLocation.X + Math.Sign(offset.X), tileLocation.Y + 1f))
				{
					offset += new Vector2(0f, -42f);
				}
				if (CheckLocation(location, tileLocation.X, tileLocation.Y - 1f))
				{
					offset += new Vector2(0f, 32f);
				}
				if (CheckLocation(location, tileLocation.X, tileLocation.Y + 1f))
				{
					offset += new Vector2(0f, -42f);
				}
				dictionary[tileLocation] = offset;
			}
			return offset;
		}

		public static bool CheckLocation(GameLocation location, float tile_x, float tile_y)
		{
			return location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Water", "Back") == null || location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Passable", "Buildings") != null;
		}
	}
}
