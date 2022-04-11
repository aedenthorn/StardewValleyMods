using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;

namespace AdvancedDialogueDisplay
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
				string name = __instance.characterDialogue.speaker.getName();
				//name = "asdfkhdsafhfdsk";
				if (!dataDict.TryGetValue(name, out DialogueDisplayData data))
					data = dataDict[defaultKey];

				// Images

				foreach (var image in data.images)
                {
					b.Draw(imageDict[image.texturePath], GetDataVector(__instance, image), new Rectangle(image.x, image.y, image.w, image.h), Color.White * image.alpha, 0, Vector2.Zero, image.scale, SpriteEffects.None, image.layerDepth);
				}


				// NPC Portrait

				var portrait = data.portrait is null ? dataDict[defaultKey].portrait : data.portrait;

				Texture2D portraitTexture;
				Rectangle portraitSource;

				if (portrait.texturePath != null)
				{
					portraitTexture = imageDict[portrait.texturePath];
				}
				else
				{
					if (__instance.characterDialogue.overridePortrait != null)
						portraitTexture = __instance.characterDialogue.overridePortrait;
					else
						portraitTexture = __instance.characterDialogue.speaker.Portrait;
				}
                if (!portrait.tileSheet)
                {
					portraitSource = new Rectangle(0, 0, portrait.w, portrait.h);
				}
                else
                {
					portraitSource = Game1.getSourceRectForStandardTileSheet(portraitTexture, __instance.characterDialogue.getPortraitIndex(), portrait.w, portrait.h);
				}
				if (!portraitTexture.Bounds.Contains(portraitSource))
				{
					portraitSource = new Rectangle(0, 0, portrait.w, portrait.h);
				}


				int xOffset = (bool)AccessTools.Method(typeof(DialogueBox), "shouldPortraitShake").Invoke(__instance, new object[] { __instance.characterDialogue }) ? Game1.random.Next(-1, 2) : 0;
				b.Draw(portraitTexture, GetDataVector(__instance, portrait) + new Vector2(xOffset, 0), new Rectangle?(portraitSource), Color.White * portrait.alpha, 0f, Vector2.Zero, portrait.scale, SpriteEffects.None, portrait.layerDepth);

				// NPC Name

				var npcName = data.name != null ? data.name : dataDict[defaultKey].name;
				var namePos = GetDataVector(__instance, npcName);

				if (npcName.centered)
				{
					if (npcName.scroll)
					{
						SpriteText.drawStringWithScrollCenteredAt(b, name, (int)namePos.X, (int)namePos.Y, npcName.placeholderText is null ? name : npcName.placeholderText, npcName.alpha, npcName.color, npcName.scrollType, npcName.layerDepth, npcName.junimo);
					}
                    else
                    {
						SpriteText.drawStringHorizontallyCenteredAt(b, name, (int)namePos.X, (int)namePos.Y, 999999, npcName.width, 999999, npcName.alpha, npcName.layerDepth, npcName.junimo, npcName.color);
					}

				}
				else
				{
					if (npcName.right)
						namePos.X -= SpriteText.getWidthOfString(name);

					if (npcName.scroll)
					{
						SpriteText.drawStringWithScrollBackground(b, name, (int)namePos.X, (int)namePos.Y, npcName.placeholderText is null ? name : npcName.placeholderText, npcName.alpha, npcName.color, npcName.alignment);
					}
					else
					{
						SpriteText.drawString(b, name, (int)namePos.X, (int)namePos.Y, 999999, npcName.width, 999999, npcName.alpha, npcName.layerDepth, npcName.junimo, npcName.color);
					}
				}


				// Texts

				foreach(var text in data.texts)
                {
					var pos = GetDataVector(__instance, text);
					if (text.centered)
					{
						if (text.variable && text.right)
							pos.X -= SpriteText.getWidthOfString(text.text) / 2;

						if (text.scroll)
						{
							SpriteText.drawStringWithScrollCenteredAt(b, name, (int)pos.X, (int)pos.Y, text.placeholderText, text.alpha, text.color, text.scrollType, text.layerDepth, text.junimo);
						}
						else
						{
							SpriteText.drawStringHorizontallyCenteredAt(b, name, (int)pos.X, (int)pos.Y, 999999, text.width, 999999, text.alpha, text.layerDepth, text.junimo, text.color);
						}

					}
					else
					{
						if (text.variable && text.right)
							pos.X -= SpriteText.getWidthOfString(text.text);

						if (text.scroll)
						{
							SpriteText.drawStringWithScrollBackground(b, name, (int)pos.X, (int)pos.Y, text.placeholderText, text.alpha, text.color, text.alignment);
						}
						else
						{
							SpriteText.drawString(b, name, (int)pos.X, (int)pos.Y, 999999, text.width, 999999, text.alpha, text.layerDepth, text.junimo, text.color);
						}
					}
				}

				var hearts = data.hearts is null ? dataDict[defaultKey].hearts : data.hearts;
				if (hearts is not null)
                {
					var pos = GetDataVector(__instance, hearts);
					int heartLevel = Game1.player.getFriendshipHeartLevelForNPC(name);
					int extraFriendshipPixels = Game1.player.getFriendshipLevelForNPC(name) % 250;

					bool datable = SocialPage.isDatable(name);
					bool spouse = false;
					if (Game1.player.friendshipData.TryGetValue(name, out Friendship friendship))
					{
						spouse = friendship.IsMarried();
					}
					for (int h = 0; h < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name, true, false)), 10); h++)
					{
						if (h > heartLevel && !hearts.showEmptyHearts)
							break;
						if (h == heartLevel && extraFriendshipPixels == 0)
							break;
						int xSource = (h < heartLevel) ? 211 : 218;
						if (datable && !friendship.IsDating() && !spouse && h >= 8)
						{
							xSource = 211;
						}
						int x = h % hearts.heartsPerRow;
						int y = h / hearts.heartsPerRow;
						b.Draw(Game1.mouseCursors, pos + new Vector2(x * 32, y * 32), new Rectangle?(new Rectangle(xSource, 428, 7, 6)), (datable && !friendship.IsDating() && !spouse && h >= 8) ? (Color.Black * 0.35f) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
						if(h == heartLevel && extraFriendshipPixels > 0)
                        {
							b.Draw(Game1.mouseCursors, pos + new Vector2(x * 32, y * 32), new Rectangle?(new Rectangle(211, 428, (int)Math.Round(7 * (extraFriendshipPixels / 250f)), 6)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
						}
					}
				}

				// Jewel

				if (__instance.shouldDrawFriendshipJewel())
				{
					var jewel = data.jewel != null ? data.jewel : dataDict[defaultKey].jewel;
					if(jewel != null)
                    {
						var pos = GetDataVector(__instance, jewel);
						b.Draw(Game1.mouseCursors, pos, new Rectangle?((Game1.player.getFriendshipHeartLevelForNPC(__instance.characterDialogue.speaker.Name) >= 10) ? new Rectangle(269, 494, 11, 11) : new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Game1.player.getFriendshipHeartLevelForNPC(__instance.characterDialogue.speaker.Name) / 2 * 11), 11, 11)), Color.White * jewel.alpha, 0f, Vector2.Zero, jewel.scale, SpriteEffects.None, jewel.layerDepth);
					}
				}


				// Dialogue String

				var dialogue = data.dialogue != null ? data.dialogue : dataDict[defaultKey].dialogue;
				var dialoguePos = GetDataVector(__instance, dialogue);
				preventGetCurrentString = false;
				SpriteText.drawString(b, __instance.getCurrentString(), (int)dialoguePos.X, (int)dialoguePos.Y, __instance.characterIndexInDialogue, dialogue.width >= 0 ? dialogue.width : __instance.width - 8, 999999, dialogue.alpha, dialogue.layerDepth, false, -1, "", dialogue.color, dialogue.alignment);


				// Close Icon

				if(__instance.dialogueIcon != null)
                {
					var button = data.button != null ? data.button : dataDict[defaultKey].button;

					__instance.dialogueIcon.position = GetDataVector(__instance, button);
				}

				preventGetCurrentString = true;
				return false;
			}

            private static Vector2 GetDataVector(DialogueBox box, BaseData data)
            {
				return new Vector2(box.x + (data.right ? box.width : 0) + data.xOffset, box.y + (data.bottom ? box.height : 0) + data.yOffset);
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