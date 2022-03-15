using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace StardewRPG
{
    public partial class ModEntry
	{
		private static int statsX = 79;
		private static int ccStatY = 85;
		private static int ccStatsY = 237;
		private static void CharacterCustomization_setUpPositions_Postfix(CharacterCustomization __instance)
		{
			if (!Config.EnableMod || __instance.source != CharacterCustomization.Source.NewGame)
				return;

			int x = __instance.xPositionOnScreen + __instance.width + 4 + 8 + 88 + 4 + 8;
			int y = __instance.yPositionOnScreen;

			for (int i = 0; i < skillNames.Length; i++)
            {
				string name = skillNames[i];
				__instance.leftSelectionButtons.Add(new ClickableTextureComponent("SDRPG_" + name, new Rectangle(new Point(x + statsX + 60, y + ccStatsY + i * ccStatY + 17), new Point(64, 64)), null, "", Game1.mouseCursors, OptionsPlusMinus.minusButtonSource, 4f, false)
				{
					myID = 106 - i,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				__instance.rightSelectionButtons.Add(new ClickableTextureComponent("SDRPG_" + name, new Rectangle(new Point(x + statsX + 134, y + ccStatsY + i * ccStatY + 17), new Point(64, 64)), null, "", Game1.mouseCursors, OptionsPlusMinus.plusButtonSource, 4f, false)
				{
					myID = 106 - i,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
		}
		private static Vector2 coords;
		private static void CharacterCustomization_performHoverAction_Postfix(CharacterCustomization __instance, int x, int y, ref string ___hoverTitle, ref string ___hoverText)
		{
			if (!Config.EnableMod)
				return;
			coords = new Vector2(x, y);
			for (int i = 0; i < skillNames.Length; i++)
			{
				var rect = new Rectangle(Utility.Vector2ToPoint(new Vector2(__instance.xPositionOnScreen + __instance.width + 4 + 8 + 88 + 4 + 8 + statsX, __instance.yPositionOnScreen + ccStatsY + 17 + i * ccStatY)), Utility.Vector2ToPoint(Game1.smallFont.MeasureString(SHelper.Translation.Get(skillNames[i]))));
				if (rect.Contains(x, y))
				{
					___hoverTitle = SHelper.Translation.Get(skillNames[i] + "-full");
					___hoverText = SHelper.Translation.Get(skillNames[i] + "-desc");
					break;
				}
			}
		}
		private static void CharacterCustomization_selectionClick_Prefix(CharacterCustomization __instance, string name, int change)
		{
			if (!Config.EnableMod || __instance.source != CharacterCustomization.Source.NewGame || !name.StartsWith("SDRPG_") || (change == 1 && GetStatValue(Game1.player, "points") <= 0))
				return;
			string stat = name.Substring("SDRPG_".Length);
			int oldStat = GetStatValue(Game1.player, stat);
			if ((change == 1 && oldStat >= Config.MaxStatValue) || (change == -1 && oldStat <= Config.MinStatValue))
				return;
			SetModData(Game1.player, stat, oldStat + change);
			SetModData(Game1.player, "points", GetStatValue(Game1.player, "points") - change);
		}
		private static void CharacterCustomization_draw_Prefix(CharacterCustomization __instance, SpriteBatch b)
		{
			if (!Config.EnableMod || __instance.source != CharacterCustomization.Source.NewGame)
				return;

			Utility.drawTextWithShadow(b, coords.ToString(), Game1.smallFont, new Vector2(20,20), Color.Black, 1f, -1f, -1, -1, 1f, 3);


			int x = __instance.xPositionOnScreen + __instance.width + 4 + 8 + 88 + 4 + 8;
			int y = __instance.yPositionOnScreen;

			Game1.drawDialogueBox(x, __instance.yPositionOnScreen, 400, __instance.height, false, true, null, false, true, -1, -1, -1);
			Utility.drawTextWithShadow(b, SHelper.Translation.Get("stardew-rpg"), Game1.dialogueFont, new Vector2(x + 78, y + 112), Color.SaddleBrown, 1f, -1f, -1, -1, 1f, 3);

			int points = GetStatValue(Game1.player, "points");
			if (points < 0)
            {
				points = Config.StartStatExtraPoints;
				SetModData(Game1.player, "points", points);
            }

			Utility.drawTextWithShadow(b, string.Format(SHelper.Translation.Get("x-points-left"), points), Game1.smallFont, new Vector2(x + statsX, y + 128 + 50), Color.Black, 1f, -1f, -1, -1, 1f, 3);
			for (int i = 0; i < skillNames.Length; i++)
			{
				Utility.drawTextWithShadow(b, SHelper.Translation.Get(skillNames[i]), Game1.smallFont, new Vector2(x + statsX, y + ccStatsY + 17 + i * ccStatY), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				int val = GetStatValue(Game1.player, skillNames[i]);

				Utility.drawTextWithShadow(b, val.ToString(), Game1.smallFont, new Vector2(x + statsX + 80 + 17, y + ccStatsY + 17 + i * ccStatY), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				int mod = GetStatMod(val);
				if(mod != 0)
                {
					string modString = "(" + (mod > 0 ? "+" : "") + mod + ")";
					Utility.drawTextWithShadow(b, modString, Game1.smallFont, new Vector2(x + statsX + 154 + 17, y + ccStatsY + 17 + i * ccStatY), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				}
			}
		}


		// Skills Page

		private static int skillStatY = 70;
		private static int skillStatsY = 198;

		private static List<ClickableTextureComponent> increaseButtons = new List<ClickableTextureComponent>();
		private static void SkillsPage_Postfix(SkillsPage __instance)
		{
			if (!Config.EnableMod)
				return;
			int x = __instance.xPositionOnScreen + __instance.width - 24;
			int y = __instance.yPositionOnScreen;

			increaseButtons.Clear();
			for(int i = 0; i < skillNames.Length; i++)
            {
				var name = skillNames[i];
				increaseButtons.Add(new ClickableTextureComponent("SDRPG_" + name, new Rectangle(new Point(x + statsX + 134, y + skillStatsY + i * skillStatY + 17), new Point(64, 64)), null, "", Game1.mouseCursors, OptionsPlusMinus.plusButtonSource, 4f, false)
				{
					myID = 106 - i,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
		}
		private static void SkillsPage_receiveLeftClick_Postfix(SkillsPage __instance, int x, int y, bool playSound = true)
		{
			if (!Config.EnableMod || GetStatValue(Game1.player, "points") <= 0)
				return;
			foreach (ClickableTextureComponent c in increaseButtons)
			{
				if (c != null && c.containsPoint(x, y))
				{

					string stat = c.name.Substring("SDRPG_".Length);
					int oldStat = GetStatValue(Game1.player, stat);
					if (oldStat >= Config.MaxStatValue)
						return;
					SetModData(Game1.player, stat, oldStat + 1);
					SetModData(Game1.player, "points", GetStatValue(Game1.player, "points") - 1);
					break;
				}
			}
		}
        private static void SkillsPage_draw_Postfix(SkillsPage __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;

			int x = __instance.xPositionOnScreen + __instance.width - 24;
			int y = __instance.yPositionOnScreen;

			// draw points box

			Game1.drawDialogueBox(x, __instance.yPositionOnScreen, 400, __instance.height, false, true, null, false, true, -1, -1, -1);
			Utility.drawTextWithShadow(b, SHelper.Translation.Get("stardew-rpg"), Game1.dialogueFont, new Vector2(x + 78, y + 112), Color.SaddleBrown, 1f, -1f, -1, -1, 1f, 3);

			int points = GetStatValue(Game1.player, "points");
			if (points < 0)
			{
				points = Config.StartStatExtraPoints;
				SetModData(Game1.player, "points", points);
			}

			if (points > 0)
			{
				Utility.drawTextWithShadow(b, string.Format(SHelper.Translation.Get("x-points-left"), points), Game1.smallFont, new Vector2(x + statsX, y + 128 + 39), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				for (int i = 0; i < increaseButtons.Count; i++)
				{
					increaseButtons[i].bounds = new Rectangle(new Point(x + statsX + 134, y + skillStatsY + i * skillStatY + 17), new Point(64, 64));
					increaseButtons[i].draw(b);
				}
			}

			for (int i = 0; i < skillNames.Length; i++)
			{
				Utility.drawTextWithShadow(b, SHelper.Translation.Get(skillNames[i]), Game1.smallFont, new Vector2(x + statsX, y + skillStatsY + 17 + i * skillStatY), Color.Black, 1f, -1f, -1, -1, 1f, 3);

				int val = GetStatValue(Game1.player, skillNames[i]);
				Utility.drawTextWithShadow(b, val.ToString(), Game1.smallFont, new Vector2(x + statsX + 80 + 17, y + skillStatsY + 17 + i * skillStatY), Color.Black, 1f, -1f, -1, -1, 1f, 3);

				int mod = GetStatMod(val);
				if (mod != 0)
				{
					string modString = "(" + (mod > 0 ? "+" : "") + mod + ")";
					Utility.drawTextWithShadow(b, modString, Game1.smallFont, new Vector2(x + statsX + 154 + 17, y + skillStatsY + 17 + i * skillStatY), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				}
			}

			// hover text

			var mx = Game1.getMouseX();
			var my = Game1.getMouseY();
			for (int i = 0; i < skillNames.Length; i++)
			{
				var rect = new Rectangle(Utility.Vector2ToPoint(new Vector2(__instance.xPositionOnScreen + __instance.width - 24 + statsX, __instance.yPositionOnScreen + skillStatsY + 17 + i * skillStatY)), Utility.Vector2ToPoint(Game1.smallFont.MeasureString(SHelper.Translation.Get(skillNames[i]))));
				if (rect.Contains(mx, my))
				{
					IClickableMenu.drawHoverText(b, Game1.parseText(SHelper.Translation.Get(skillNames[i] + "-desc"), Game1.smallFont, 256), Game1.smallFont, 0, 0, -1, SHelper.Translation.Get(skillNames[i] + "-full"), -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
					break;
				}
			}


			int playerLevel = GetExperienceLevel(Game1.player);
			string levelString = string.Format(SHelper.Translation.Get("level-x"), playerLevel);
			b.DrawString(Game1.smallFont, levelString, new Vector2((float)(__instance.xPositionOnScreen + 64 - 12 + 64) - Game1.smallFont.MeasureString(levelString).X / 2f, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 252), Game1.textColor);
			var levels = GetExperienceLevels();
			if (playerLevel < levels.Length + 1)
            {
				string expString = string.Format(SHelper.Translation.Get("x/y-exp-to-next"), GetStatValue(Game1.player, "exp", 0), levels[playerLevel - 1]);
				b.DrawString(Game1.smallFont, expString, new Vector2((float)(__instance.xPositionOnScreen + 64 - 12 + 64) - Game1.smallFont.MeasureString(levelString).X / 2f, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 280), Game1.textColor);
			}


			int totalLevels = GetTotalSkillLevels(Game1.player);
			int newLevels = Math.Max(0, (int)(playerLevel * (1 + Config.IntSkillLevelsBonus)) - totalLevels - 1);

			for(int i = 0; i < 6; i++)
            {
				int sl = Game1.player.GetSkillLevel(i);
				int sp = Game1.player.professions.Count(p => p / 6 == i && p % 6 > 1);
				if (sl >= 10 && !Game1.player.newLevels.Contains(new Point(i, 10)))
					sp--;
				if(sp > 0) // prestige
                {
					newLevels -= 10 * sp;
                }
			}

			if (newLevels <= 0 || !Config.ManualSkillUpgrades)
                return;

			// draw skill changes

			string pointString = string.Format(SHelper.Translation.Get("x-points"), newLevels);
			b.DrawString(Game1.smallFont, pointString, new Vector2((float)(__instance.xPositionOnScreen + 64 - 12 + 64) - Game1.smallFont.MeasureString(pointString).X / 2f, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 28), Game1.textColor);

			x = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? (__instance.xPositionOnScreen + __instance.width - 448 - 48) : (__instance.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8));
			y = __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 8;

			int addedXSource = 0;
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					Rectangle boundary = (i + 1) % 5 == 0 ? new Rectangle(new Point(addedXSource + x - 4 + i * 36, y + j * 56), new Point(14 * 4, 9 * 4)) : new Rectangle(new Point(addedXSource + x - 4 + i * 36, y + j * 56), new Point(8 * 4, 9 * 4));
					if(!boundary.Contains(Game1.getMousePosition(true)))
						continue;
					int which = j;
					if (which == 1)
						which = 3;
					else if (which == 3)
						which = 1;
					bool clickable = Game1.player.GetSkillLevel(which) == i;
					if (!clickable)
						return;
					if (SHelper.Input.IsDown(StardewModdingAPI.SButton.MouseLeft))
					{
						int newLevel = 1;
						foreach (int level in skillLevels)
						{
							if (Game1.player.experiencePoints[which] < level)
							{
								Game1.player.experiencePoints[which] = level;
								Game1.player.newLevels.Add(new Point(which, newLevel));

								switch (which)
								{
									case 0:
										Game1.player.FarmingLevel++;
										break;
									case 1:
										Game1.player.FishingLevel++;
										break;
									case 2:
										Game1.player.ForagingLevel++;
										break;
									case 3:
										Game1.player.MiningLevel++;
										break;
									case 4:
										Game1.player.CombatLevel++;
										break;
								}
								if((i + 1) % 5 == 0)
								{
									int drawX = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? (__instance.xPositionOnScreen + __instance.width - 448 - 48 + 4) : (__instance.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 4);
									int drawY = __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 12;
									__instance.skillBars.Add(new ClickableTextureComponent("", new Rectangle(drawX - 4 + (i == 9 ? 348 : 144), drawY + j * 56, 56, 36), null, "", Game1.mouseCursors, new Rectangle(159, 338, 14, 9), 4f, true)
									{
										myID = ((i + 1 == 5) ? (100 + j) : (200 + j)),
										leftNeighborID = ((i + 1 == 5) ? j : (100 + j)),
										rightNeighborID = ((i + 1 == 5) ? (200 + j) : -1),
										downNeighborID = -99998
									});
								}
								return;
							}
							newLevel++;
						}
					}
					if ((i + 1) % 5 == 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2(addedXSource + x + i * 36, y - 4 + j * 56), new Rectangle?(new Rectangle(145 + 14, 338, 14, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.871f);
					}
					else if ((i + 1) % 5 != 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2(addedXSource + x + i * 36, y - 4 + j * 56), new Rectangle?(new Rectangle(129 + 8 , 338, 8, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.871f);
					}
				}
				if ((i + 1) % 5 == 0)
				{
					addedXSource += 24;
				}
			}
		}

		public static bool ChatBox_runCommand_Prefix(string command)
		{
			if (!Config.EnableMod)
				return true;

			if (command.StartsWith("levelup"))
			{
				int num = 1;
				if (command.Contains(" "))
					int.TryParse(command.Split(' ')[1], out num);
				LevelUp(num);
				return false;
			}
			if (command.Equals("respec"))
			{
				Respec();
				return false;
			}
			return true;
		}
    }
}