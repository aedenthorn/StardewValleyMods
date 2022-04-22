using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace PartialHearts
{
    public partial class ModEntry
    {
		[HarmonyPatch(typeof(SocialPage), "drawNPCSlot")]
        public class SocialPage_drawNPCSlot_Patch
        {
            public static void Postfix(SocialPage __instance, SpriteBatch b, int i, List<ClickableTextureComponent> ___sprites)
            {
                if (!Config.EnableMod)
                    return;

				string name = __instance.names[i] as string;
				int extraFriendshipPixels = Game1.player.getFriendshipLevelForNPC(name) % 250;
				if (extraFriendshipPixels == 0)
					return;
				int heartLevel = Game1.player.getFriendshipHeartLevelForNPC(name);
				if (heartLevel == Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name, true, false)))
					return;

				Texture2D texture;
				Rectangle source;
				float scale;
				if (Config.Granular)
                {
					texture = heartTexture;
					source = new Rectangle(0, 0, (int)Math.Round(28 * (extraFriendshipPixels / 250f)), 24);
					scale = 1;
				}
                else
				{
					texture = Game1.mouseCursors;
					source = new Rectangle(211, 428, (int)Math.Round(7 * (extraFriendshipPixels / 250f)), 6);
					scale = 4;
				}

				if (heartLevel < 10)
				{
					b.Draw(texture, new Vector2(__instance.xPositionOnScreen + 320 - 4 + heartLevel * 32, ___sprites[i].bounds.Y + 64 - 28), source, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.88f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, new Vector2(__instance.xPositionOnScreen + 320 - 4 + (heartLevel - 10) * 32, ___sprites[i].bounds.Y + 64), new Rectangle?(new Rectangle(211, 428, (int)Math.Round(7 * (extraFriendshipPixels / 250f)), 6)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
			}
		}
		private static void CheatsOptionsNpcSlider_draw_Postfix(object __instance, SpriteBatch spriteBatch, int slotX, int slotY, IClickableMenu context, int ___Value)
		{
			string name = (string)AccessTools.Field(__instance.GetType(), "label").GetValue(__instance);
			int extraFriendshipPixels = Game1.player.getFriendshipLevelForNPC(name) % 250;
			if (extraFriendshipPixels == 0)
				return;
			Rectangle bounds = (Rectangle)AccessTools.Field(__instance.GetType(), "bounds").GetValue(__instance);
			Texture2D texture;
			Rectangle source;
			float scale;
			if (Config.Granular)
			{
				texture = heartTexture;
				source = new Rectangle(0, 0, (int)Math.Round(28 * (extraFriendshipPixels / 250f)), 24);
				scale = 1;
			}
			else
			{
				texture = Game1.mouseCursors;
				source = new Rectangle(211, 428, (int)Math.Round(7 * (extraFriendshipPixels / 250f)), 6);
				scale = 4;
			}
			spriteBatch.Draw(texture, new Vector2(slotX + bounds.X + ___Value * 32, slotY + bounds.Y), source, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.88f);
		}
	}

}