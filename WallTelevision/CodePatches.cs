using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace WallTelevisions
{
	public partial class ModEntry
	{
		public class Furniture_canBePlacedHere_Patch
		{
			public static bool Prefix(Furniture __instance, GameLocation l, Vector2 tile, ref bool __result)
			{
				if (!Config.ModEnabled || __instance is not TV || __instance.Name.Contains("Budget") || __instance.Name.Contains("Floor") || !typeof(DecoratableLocation).IsAssignableFrom(l.GetType()) || !(l as DecoratableLocation).isTileOnWall((int)tile.X, (int)tile.Y) || (l as DecoratableLocation).isTileOnWall((int)tile.X, (int)tile.Y + 1))
					return true;

				__result = true;
				return false;
			}
		}

		public class Furniture_GetAdditionalFurniturePlacementStatus_Patch
		{
			public static bool Prefix(Furniture __instance, GameLocation location, int x, int y, ref int __result)
			{
				if (!Config.ModEnabled || __instance is not TV || __instance.Name.Contains("Budget") || __instance.Name.Contains("Floor") || !typeof(DecoratableLocation).IsAssignableFrom(location.GetType()) || !(location as DecoratableLocation).isTileOnWall(x / 64, y / 64) || (location as DecoratableLocation).isTileOnWall(x / 64, y / 64 + 1))
					return true;

				__instance.TileLocation = new Vector2(x / 64, y / 64);
				__result = 0;
				return false;
			}
		}

		public class Utility_playerCanPlaceItemHere_Patch
		{
			public static bool Prefix(GameLocation location, Item item, int x, int y, Farmer f, ref bool __result)
			{
				if (!Config.ModEnabled || item is not TV || item.Name.Contains("Budget") || item.Name.Contains("Floor") || !typeof(DecoratableLocation).IsAssignableFrom(location.GetType()) || !(location as DecoratableLocation).isTileOnWall(x / 64, y / 64) || (location as DecoratableLocation).isTileOnWall(x / 64, y / 64 + 1) || !Utility.isWithinTileWithLeeway(x, y, item, f))
					return true;

				__result = true;
				return false;
			}
		}

		public class Furniture_draw_Patch
		{
			public static bool Prefix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, NetVector2 ___drawPosition)
			{
				if (!Config.ModEnabled || __instance is not TV || (!__instance.Name.Contains("Plasma") && !__instance.Name.Contains("Tropical")) || !typeof(DecoratableLocation).IsAssignableFrom(Game1.currentLocation.GetType()) || !(Game1.currentLocation as DecoratableLocation).isTileOnWall((int)__instance.TileLocation.X, (int)__instance.TileLocation.Y))
					return true;

				Texture2D texture = Game1.content.Load<Texture2D>(__instance.Name.Contains("Plasma") ? "aedenthorn.WallTelevisions/plasma" : "aedenthorn.WallTelevisions/tropical");
				Rectangle sourceRectangle = new(0, 0, 48, 48);

				if (Furniture.isDrawingLocationFurniture)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), sourceRectangle, Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 8) / 10000f);
				}
				else
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (__instance.sourceRect.Height * 4 - __instance.boundingBox.Height) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), sourceRectangle, Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 8 ) / 10000f);
				}
				return false;
			}
		}
	}
}
