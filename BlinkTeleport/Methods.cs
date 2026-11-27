using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace BlinkTeleport
{
	public partial class ModEntry
	{
		public static void DoBlink(Vector2 tile)
		{

            TemporaryAnimatedSpriteList sprites = new();

			if (Game1.random.NextDouble() < 0.5)
			{
				sprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(Game1.player.Tile.X * 64f, Game1.player.Tile.Y * 64f), false, Game1.random.NextDouble() < 0.5));
			}
			else
			{
				sprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(Game1.player.Tile.X * 64f, Game1.player.Tile.Y * 64f), false, Game1.random.NextDouble() < 0.5));
			}
			Game1.Multiplayer.broadcastSprites(Game1.player.currentLocation, sprites);
			if (!string.IsNullOrEmpty(Config.BlinkSound))
			{
				Game1.player.currentLocation.playSound(Config.BlinkSound);
			}
			Game1.player.Position = tile * 64;
		}
	}
}
