using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;

namespace YetAnotherJumpMod
{
	public partial class ModEntry
	{
		private static readonly PerScreen<bool> gettingLocalPositionForShadow = new(() => false);

		internal static bool GettingLocalPositionForShadow
		{
			get => gettingLocalPositionForShadow.Value;
			set => gettingLocalPositionForShadow.Value = value;
		}

		public class Horse_draw_Patch
		{
			public static void Prefix(ref Horse __instance, SpriteBatch b)
			{
				if (!Config.ModEnabled)
					return;

				GettingLocalPositionForShadow = true;
				b.Draw(horseShadow, __instance.getLocalPosition(Game1.viewport) + Config.HorseShadowOffset + new Vector2(__instance.Sprite.SpriteWidth * 4 / 2, __instance.GetBoundingBox().Height / 2), new Microsoft.Xna.Framework.Rectangle?(__instance.Sprite.SourceRect), Color.White, __instance.rotation, new Vector2(__instance.Sprite.SpriteWidth / 2, __instance.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, __instance.Scale) * 4f, (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
				GettingLocalPositionForShadow = false;
				if (__instance.rider is not null)
				{
					if (!PlayerJumpingWithHorse)
					{
						__instance.yOffset = 0;
						__instance.drawOnTop = false;
					}
					else
					{
						__instance.yOffset = __instance.rider.yJumpOffset * 2;
						__instance.drawOnTop = true;
					}
				}
			}
		}

		public class Character_getLocalPosition_Patch
		{
			public static void Postfix(Character __instance, ref Vector2 __result)
			{
				if (!Config.ModEnabled || GettingLocalPositionForShadow)
					return;

				if (__instance is Horse horse && PlayerJumpingWithHorse)
				{
					__result.Y += horse.yOffset;
				}
			}
		}

		public class Farmer_getDrawLayer_Patch
		{
			public static bool Prefix(ref Farmer __instance, ref float __result)
			{
				if (!Config.ModEnabled)
					return true;

				if(__instance.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID && PlayerJumpingWithHorse)
				{
					__result = 0.992f;
					return false;
				}
				return true;
			}
		}
	}
}
