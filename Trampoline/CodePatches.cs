﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;

namespace Trampoline
{
	public partial class ModEntry
	{
		public class Furniture_draw_Patch
		{
			public static bool Prefix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, NetVector2 ___drawPosition)
			{
				if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey))
					return true;

				Rectangle drawn_source_rect = new(0, 0, 32, 48);
				bool touching = IsOnTrampoline() && __instance == Game1.player.sittingFurniture && Game1.player.yOffset < 12;

				if (touching)
				{
					drawn_source_rect.X += 32;
				}

				Rectangle cutoffRect1 = new(32, 0, 32, 30);
				Rectangle cutoffRect2 = new(32, 30, 32, 18);

				if (Furniture.isDrawingLocationFurniture)
				{
					if (touching)
					{
						spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(cutoffRect1), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 2E-09f + __instance.TileLocation.Y / 100000f);
						spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, 120), new Rectangle?(cutoffRect2), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 48) / 10000f);
					}
					else
					{
						spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 2E-09f + __instance.TileLocation.Y / 100000f);
					}
				}
				else
				{
					spriteBatch.Draw(trampolineTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (__instance.sourceRect.Height * 4 - __instance.boundingBox.Height) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 2E-09f + __instance.TileLocation.Y / 100000f);
				}
				return false;
			}
		}

		public class Furniture_GetSeatPositions_Patch
		{
			public static bool Prefix(Furniture __instance, ref List<Vector2> __result)
			{
				if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey))
					return true;

				__result = new List<Vector2>()
				{
					__instance.TileLocation + new Vector2(0.5f, 0.5f)
				};
				return false;
			}
		}

		public class Furniture_GetSeatCapacity_Patch
		{
			public static bool Prefix(Furniture __instance, ref int __result)
			{
				if (!Config.EnableMod || !__instance.modData.ContainsKey(trampolineKey))
					return true;

				__result = 1;
				return false;
			}
		}

		public class Farmer_ShowSitting_Patch
		{
			public static bool Prefix(Farmer __instance)
			{
				return !Config.EnableMod || !IsOnTrampoline(__instance);
			}
		}
	}
}
