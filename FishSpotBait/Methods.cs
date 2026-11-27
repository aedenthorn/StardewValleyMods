using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FishSpotBait
{
	public partial class ModEntry
	{
		private static int GetPlayerDirectionTowardTile(Farmer player, Point targetTile)
		{
			Vector2 playerPosition = player.Position / 64f;
			Vector2 targetPosition = new(targetTile.X, targetTile.Y);
			float deltaX = targetPosition.X - playerPosition.X;
			float deltaY = targetPosition.Y - playerPosition.Y;
			float angle = (float)Math.Atan2(deltaY, deltaX);

			return angle switch
			{
				_ when angle >= -Math.PI / 4 && angle < Math.PI / 4 => 1,
				_ when angle >= Math.PI / 4 && angle < 3 * Math.PI / 4 => 2,
				_ when angle >= -3 * Math.PI / 4 && angle < -Math.PI / 4 => 0,
				_ => 3
			};
		}
	}
}
