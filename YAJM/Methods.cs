using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace YetAnotherJumpMod
{
	public partial class ModEntry
	{
		public static bool IsOutOfMap(GameLocation location, Rectangle position)
		{
			return position.X < 0 ||
					position.Y < 0 ||
					position.X >= location.map.Layers[0].LayerWidth * Game1.tileSize ||
					position.Y >= location.map.Layers[0].LayerHeight * Game1.tileSize;
		}

		public static bool IsOnWater(GameLocation location, Rectangle position)
		{
			if (location.waterTiles is not null && location.waterTiles.waterTiles is not null)
			{
				int x1 = position.X / Game1.tileSize;
				int y1 = position.Y / Game1.tileSize;
				int x2 = (position.X + position.Width) / Game1.tileSize;
				int y2 = (position.Y + position.Height) / Game1.tileSize;

				if (x1 >= 0 && y1 >= 0 && x1 < location.waterTiles.waterTiles.GetLength(0) && y1 < location.waterTiles.waterTiles.GetLength(1) &&
					x2 >= 0 && y2 >= 0 && x2 < location.waterTiles.waterTiles.GetLength(0) && y2 < location.waterTiles.waterTiles.GetLength(1) &&
					x1 >= 0 && y2 >= 0 && x1 < location.waterTiles.waterTiles.GetLength(0) && y2 < location.waterTiles.waterTiles.GetLength(1) &&
					x2 >= 0 && y1 >= 0 && x2 < location.waterTiles.waterTiles.GetLength(0) && y1 < location.waterTiles.waterTiles.GetLength(1))
				{
					return location.waterTiles.waterTiles[x1, y1].isWater == true ||
							location.waterTiles.waterTiles[x2, y2].isWater == true ||
							location.waterTiles.waterTiles[x1, y2].isWater == true ||
							location.waterTiles.waterTiles[x2, y1].isWater == true;
				}
			}
			return false;
		}

		public static void TryToJump()
		{
			GameLocation location = Game1.player.currentLocation;
			int maxJumpDistance = Math.Max(2, Config.MaxJumpDistance);
			(int ox, int oy) = GetDirectionOffset(Game1.player.facingDirection.Value);
			List<bool> collisions = GetCollisions(location, ox, oy, maxJumpDistance);

			if (Config.PlayJumpSound && !string.IsNullOrWhiteSpace(Config.JumpSound))
			{
				Game1.playSound(Config.JumpSound);
			}
			PlayerJumpingWithHorse = Game1.player.isRidingHorse();
			BlockedJump = false;
			VelX = 0;
			VelY = 0;
			if (!collisions[0] && !collisions[1])
			{
				PerformBlockedJump();
				return;
			}
			for (int i = 1; i < collisions.Count; i++)
			{
				if (!collisions[i])
				{
					PerformFreeJump(ox, oy, i);
					return;
				}
			}
			PerformBlockedJump();
		}

		private static (int ox, int oy) GetDirectionOffset(int facingDirection)
		{
			return facingDirection switch
			{
				0 => (0, -1),
				1 => (1, 0),
				2 => (0, 1),
				3 => (-1, 0),
				_ => (0, 0)
			};
		}

		private static List<bool> GetCollisions(GameLocation location, int ox, int oy, int maxJumpDistance)
		{
			List<bool> collisions = new();

			for (int i = 0; i < maxJumpDistance; i++)
			{
				Rectangle box = GetBoundingBox(ox, oy, i);

				collisions.Add(location.isCollidingPosition(box, Game1.viewport, true, 0, false, Game1.player) || IsOutOfMap(location, box) || (IsOnWater(location, box) && !SHelper.ModRegistry.IsLoaded("aedenthorn.Swim")));
			}
			return collisions;
		}

		private static Rectangle GetBoundingBox(int ox, int oy, int distance)
		{
			Rectangle box = Game1.player.isRidingHorse() && Game1.player.mount is not null ? Game1.player.mount.GetBoundingBox() : Game1.player.GetBoundingBox();

			box.X += ox * Game1.tileSize * distance;
			box.Y += oy * Game1.tileSize * distance;
			return box;
		}

		private static void PerformFreeJump(int ox, int oy, int distance)
		{
			VelX = ox * (float)Math.Sqrt(distance * 16);
			VelY = oy * (float)Math.Sqrt(distance * 16);
			LastYJumpVelocity = 0;
			Game1.player.CanMove = false;
			PerformJump((float)Math.Sqrt(distance * 16));
		}

		private static void PerformBlockedJump()
		{
			BlockedJump = true;
			PerformJump(Config.OrdinaryJumpHeight);
		}

		private static void PerformJump(float v)
		{
			Game1.player.synchronizedJump(v);
			SHelper.Events.GameLoop.UpdateTicked += context.GameLoop_UpdateTicked;
		}
	}
}
