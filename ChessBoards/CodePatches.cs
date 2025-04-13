using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Object = StardewValley.Object;

namespace ChessBoards
{
	public partial class ModEntry
	{
		private static bool drawingPieces;

		public class GameLocation_draw_Patch
		{
			public static void Postfix(GameLocation __instance, SpriteBatch b)
			{
				if (!Config.EnableMod)
					return;

				drawingPieces = true;
				foreach (var obj in __instance.objects.Values)
				{
					if (obj.modData.ContainsKey(pieceKey))
						obj.draw(b, (int)obj.TileLocation.X, (int)obj.TileLocation.Y, 1f);
				}
				drawingPieces = false;
			}
		}

		public class Object_draw_Patch
		{
			public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
			{
				if (!Config.EnableMod || !__instance.modData.TryGetValue(pieceKey, out string piece))
					return true;
				if (!drawingPieces)
					return false;
				if(Config.WalkThrough)
					__instance.isTemporarilyInvisible = true;
				if (heldPiece is not null && __instance == heldPiece)
					return false;

				Vector2 scaleFactor = __instance.getScale();
				Vector2 tilePosition;
				Point square = new(int.Parse(__instance.modData[squareKey][..1]), int.Parse(__instance.modData[squareKey].Substring(2, 1)));
				Vector2 cornerTile = new(x - square.X + 1, y + square.Y - 1);

				if (Game1.currentLocation.terrainFeatures[cornerTile].modData.ContainsKey(flippedKey))
				{
					tilePosition = GetFlippedTile(cornerTile, square) * 64 - new Vector2(0, 64);
				}
				else
				{
					tilePosition = new Vector2(x * 64, y * 64 - 64);
				}

				Vector2 position = Game1.GlobalToLocal(Game1.viewport, tilePosition);
				Rectangle destination = new((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
				float draw_layer = Math.Max(0f, (float)((tilePosition.Y / 64 + 1) * 64 + 40) / 10000f) + tilePosition.X / 64 * 1E-05f;

				spriteBatch.Draw(piecesSheet, destination, GetSourceRectForPiece(piece), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
				return false;
			}
		}

		public class GameLocation_answerDialogue_Patch
		{
			public static bool Prefix(GameLocation __instance, Response answer)
			{
				if (!Config.EnableMod || __instance.lastQuestionKey != "ChessBoards-mod-promote-question")
					return true;

				PromotePiece(answer.responseKey);
				return false;
			}
		}
	}
}
