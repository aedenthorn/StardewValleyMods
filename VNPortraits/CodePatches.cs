using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace VNPortraits
{
    public partial class ModEntry
    {
		private static bool preventGetCurrentString;
		[HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.drawPortrait))]
        public class DialogueBox_drawPortrait_Patch
        {
            public static bool Prefix(DialogueBox __instance, SpriteBatch b)
            {
                if (!Config.EnableMod)
                    return true;
				int portraitBoxX = __instance.x + __instance.width - 220;
				int nameTextY = __instance.y - 16;
				int mainTextY = __instance.y + 48;
				int portraitBoxY = __instance.y - 232;
				//b.Draw(Game1.mouseCursors, new Rectangle(xPositionOfPortraitArea - 40, this.y, 36, this.height), new Rectangle?(new Rectangle(278, 324, 9, 1)), Color.White);
				//b.Draw(Game1.mouseCursors, new Vector2((float)(xPositionOfPortraitArea - 40), (float)(this.y - 20)), new Rectangle?(new Rectangle(278, 313, 10, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				//b.Draw(Game1.mouseCursors, new Vector2((float)(xPositionOfPortraitArea - 40), (float)(this.y + this.height)), new Rectangle?(new Rectangle(278, 328, 10, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				//b.Draw(Game1.mouseCursors, new Vector2((float)(xPositionOfPortraitArea - 8), (float)__instance.y), new Rectangle?(new Rectangle(583, 411, 115, 97)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				Texture2D portraitTexture = (__instance.characterDialogue.overridePortrait != null) ? __instance.characterDialogue.overridePortrait : __instance.characterDialogue.speaker.Portrait;
				Rectangle portraitSource = Game1.getSourceRectForStandardTileSheet(portraitTexture, __instance.characterDialogue.getPortraitIndex(), 64, 64);
				if (!portraitTexture.Bounds.Contains(portraitSource))
				{
					portraitSource = new Rectangle(0, 0, 64, 64);
				}
				int xOffset = (bool)AccessTools.Method(typeof(DialogueBox), "shouldPortraitShake").Invoke(__instance, new object[] { __instance.characterDialogue }) ? Game1.random.Next(-1, 2) : 0;
				b.Draw(portraitTexture, new Vector2((float)(portraitBoxX + 16 + xOffset), (float)(portraitBoxY - 44)), new Rectangle?(portraitSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				
				//SpriteText.drawString(b, __instance.characterDialogue.speaker.getName(), __instance.x + __instance.width - SpriteText.getWidthOfString(__instance.characterDialogue.speaker.getName()) - 64, nameTextY);
				SpriteText.drawStringWithScrollBackground(b, __instance.characterDialogue.speaker.getName(), __instance.x + __instance.width - SpriteText.getWidthOfString(__instance.characterDialogue.speaker.getName()) - 16, nameTextY);
				if (__instance.shouldDrawFriendshipJewel())
				{
					b.Draw(Game1.mouseCursors, new Vector2(__instance.x + __instance.width + 28, nameTextY + 10), new Rectangle?((Game1.player.getFriendshipHeartLevelForNPC(__instance.characterDialogue.speaker.Name) >= 10) ? new Rectangle(269, 494, 11, 11) : new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Game1.player.getFriendshipHeartLevelForNPC(__instance.characterDialogue.speaker.Name) / 2 * 11), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
				preventGetCurrentString = false;
				SpriteText.drawString(b, __instance.getCurrentString(), __instance.x + 8, mainTextY, __instance.characterIndexInDialogue, __instance.width - 8 - 24, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);

				if(__instance.dialogueIcon != null)
                {
					__instance.dialogueIcon.position = new Vector2((float)(__instance.x + __instance.width - 40), (float)(__instance.y + __instance.height - 44));
				}

				preventGetCurrentString = true;
				return false;
			}

        }
		[HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.draw))]
        public class DialogueBox_draw_Patch
        {
            public static void Postfix(DialogueBox __instance, SpriteBatch b)
            {
                if (!Config.EnableMod)
                    return;

				preventGetCurrentString = false;
			}

        }
        [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.getCurrentString))]
        public class DialogueBox_getCurrentString_Patch
		{
            public static bool Prefix(DialogueBox __instance, ref string __result)
            {
                if (!Config.EnableMod || !preventGetCurrentString)
                    return true;
				__result = "";
				return false;
			}

        }
    }
}