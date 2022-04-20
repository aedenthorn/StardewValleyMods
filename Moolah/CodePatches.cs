using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace Moolah
{
    public partial class ModEntry
    {
        private static int maxValue = 1000000000;
        private static long previousTargetValue;
        private static long currentValue;
        private static long speed;
        private static long soundTimer;

        [HarmonyPatch(typeof(Farmer), nameof(Farmer._money))]
        [HarmonyPatch(MethodType.Setter)]
        public class Farmer__money_Setter_Patch
        {
            public static void Prefix(Farmer __instance, ref int value)
            {
                if (!Config.EnableMod)
                    return;
                long moocha = 0;
                if(__instance.modData.TryGetValue("aedenthorn.Moolah/moocha", out string moochaString))
                    moocha = Convert.ToInt64(moochaString);
                if(value > maxValue)
                {
                    //SMonitor.Log($"Storing excess money: {value - maxValue} with {moocha}");
                    moocha += value - maxValue;
                    value = maxValue;
                    __instance.modData["aedenthorn.Moolah/moocha"] = moocha + "";
                }
                else if(moocha > 0 && value < maxValue)
                {
                    //SMonitor.Log($"Retrieving excess money: {maxValue - value} from {moocha}");
                    if (maxValue - value > moocha)
                    {
                        value += (int)moocha;
                        moocha = 0;
                    }
                    else
                    {
                        moocha -= maxValue - value;
                        value = maxValue;
                    }
                    __instance.modData["aedenthorn.Moolah/moocha"] = moocha + "";
                }
                //SMonitor.Log($"Total money: {value + moocha}");
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox))]
        public class DayTimeMoneyBox_drawMoneyBox_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, SpriteBatch b, int overrideX, int overrideY)
            {
                if (!Config.EnableMod || Game1.player is null)
                    return true;

				long moocha = GetTotalMoolah();

                AccessTools.Method(typeof(DayTimeMoneyBox), "updatePosition").Invoke(__instance, new object[] { });
                b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(340, 472, 65, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

				int extraDigits = moocha.ToString().Length - 8;
				if(extraDigits > 0)
                {
					b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - (extraDigits) * 24, (float)(172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(340, 472, 11, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					for (int i = 0; i < extraDigits; i++)
                    {
						b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - (i + 1) * 24, (float)(180 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(356, 474, 6, 15)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					}
				}

				DrawMoneyDial(__instance.moneyDial, b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(196 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), moocha);
                if (__instance.moneyShakeTimer > 0)
                {
                    __instance.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                }
                return false;
            }

			private static void DrawMoneyDial(MoneyDial moneyDial, SpriteBatch b, Vector2 position, long target)
			{
				int numDigits = currentValue.ToString().Length;
				if (previousTargetValue != target)
				{
					speed = (int)(target - currentValue) / 100;
					previousTargetValue = target;
					soundTimer = Math.Max(6, 100 / (Math.Abs(speed) + 1));
				}
				if (AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyShineTimer") > 0 && currentValue == target)
				{
					AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyShineTimer") -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				}
				if (AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyMadeAccumulator") > 0)
				{
					AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyMadeAccumulator") -= (int)(Math.Abs(speed / 2) + 1) * ((moneyDial.animations.Count <= 0) ? 100 : 1);
					if (AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyMadeAccumulator") <= 0)
					{
						AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyShineTimer") = numDigits * 60;
					}
				}
				if (AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyMadeAccumulator") > 2000)
				{
					Game1.dayTimeMoneyBox.moneyShakeTimer = 100;
				}
				if (currentValue != target)
				{
					currentValue += speed + ((currentValue < target) ? 1 : -1);
					if (currentValue < target)
					{
						AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyMadeAccumulator") += (int)Math.Abs(speed);
					}
					soundTimer--;
					if (Math.Abs(target - currentValue) <= speed + 1 || (speed != 0 && Math.Sign(target - currentValue) != Math.Sign(speed)))
					{
						currentValue = target;
					}
					if (soundTimer <= 0)
					{
						if (moneyDial.onPlaySound != null)
						{
							moneyDial.onPlaySound(Math.Sign(target - currentValue));
						}
						soundTimer = Math.Max(6, 100 / (Math.Abs(speed) + 1));
						if (Game1.random.NextDouble() < 0.4)
						{
							if (target > currentValue)
							{
								moneyDial.animations.Add(new TemporaryAnimatedSprite(Game1.random.Next(10, 12), position + new Vector2((float)Game1.random.Next(30, 190), (float)Game1.random.Next(-32, 48)), Color.Gold, 8, false, 100f, 0, -1, -1f, -1, 0));
							}
							else if (target < currentValue)
							{
								moneyDial.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(356, 449, 1, 1), 999999f, 1, 44, position + new Vector2((float)Game1.random.Next(160), (float)Game1.random.Next(-32, 32)), false, false, 1f, 0.01f, Color.White, (float)(Game1.random.Next(1, 3) * 4), -0.001f, 0f, 0f, false)
								{
									motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(-30, -5) / 10f),
									acceleration = new Vector2(0f, 0.25f)
								});
							}
						}
					}
				}
				for (int i = moneyDial.animations.Count - 1; i >= 0; i--)
				{
					if (moneyDial.animations[i].update(Game1.currentGameTime))
					{
						moneyDial.animations.RemoveAt(i);
					}
					else
					{
						moneyDial.animations[i].draw(b, true, 0, 0, 1f);
					}
				}
				int xPosition = 0;
				if (numDigits > 8)
                {
					xPosition -= (numDigits - 8) * 24;
                }
                else
                {
					xPosition += (8 - numDigits) * 24;
				}
				bool significant = false;
				numDigits = currentValue.ToString().Length; 
				for (int j = 0; j < numDigits; j++)
				{
					int currentDigit = int.Parse(currentValue.ToString()[j].ToString());
					if (currentDigit > 0 || j == numDigits - 1)
					{
						significant = true;
					}
					if (significant)
					{
						b.Draw(Game1.mouseCursors, position + new Vector2(xPosition, 0f), new Rectangle?(new Rectangle(286, 502 - (currentDigit) * 8, 5, 8)), Color.Maroon, 0f, Vector2.Zero, 4f + ((AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyShineTimer") / 60 == numDigits - j) ? 0.3f : 0f), SpriteEffects.None, 1f);
					}
					xPosition += 24;
				}
			}
        }

    }
}