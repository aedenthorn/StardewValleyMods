using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Moolah
{
    public partial class ModEntry
    {
        private static void DrawMoneyDial(MoneyDial moneyDial, SpriteBatch b, Vector2 position, BigInteger target)
        {
            int numDigits = currentValue.ToString().Length;
            if (previousTargetValue != target)
            {
                BigInteger diff = target - currentValue;

                if (diff < -int.MaxValue)
                    speed = -int.MaxValue / 100;
                else if (diff > int.MaxValue)
                    speed = int.MaxValue / 100;
                else
                    speed = (int)diff / 100;
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
            //currentValue = target;

            if (currentValue != target)
            {
                currentValue += speed + ((currentValue < target) ? 1 : -1);
                if (currentValue < target)
                {
                    AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyMadeAccumulator") += (int)Math.Abs(speed);
                }
                soundTimer--;
                BigInteger diff = target - currentValue;
                int sign = diff > 0 ? 1 : -1;
                BigInteger abs = diff > 0 ? diff : currentValue - target;

                if (abs <= speed + 1 || (speed != 0 && sign != Math.Sign(speed)))
                {
                    currentValue = target;
                }
                if (soundTimer <= 0)
                {
                    if (moneyDial.onPlaySound != null)
                    {
                        moneyDial.onPlaySound(sign);
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
            var moneyShineTimer = AccessTools.FieldRefAccess<MoneyDial, int>(moneyDial, "moneyShineTimer");
            var showSeparator = !string.IsNullOrEmpty(Config.Separator);
            for (int j = 0; j < numDigits; j++)
            {
                int currentDigit = int.Parse(currentValue.ToString()[j].ToString());
                if (currentDigit > 0 || j == numDigits - 1)
                {
                    significant = true;
                }
                if (significant)
                {
                    if (showSeparator && j < numDigits - 1 && (numDigits - j) % Config.SeparatorInterval == 1)
                    {
                        SpriteText.drawString(b, Config.Separator, (int)position.X + xPosition + Config.SeparatorX, (int)position.Y + Config.SeparatorY);
                    }
                    b.Draw(Game1.mouseCursors, position + new Vector2(xPosition, 0f), new Rectangle?(new Rectangle(286, 502 - (currentDigit) * 8, 5, 8)), Color.Maroon, 0f, Vector2.Zero, 4f + ((moneyShineTimer / 60 == numDigits - j) ? 0.3f : 0f), SpriteEffects.None, 1f);
                }
                xPosition += 24;
            }
        }
        private static string CheckIntToString(string input)
        {
            if (!Config.EnableMod || input.Length <= Config.SeparatorInterval)
                return input;
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                output += input[i];
                if (i < input.Length - 1 && (input.Length - i) % Config.SeparatorInterval == 1)
                    output += Config.Separator;
            }
            return output;
        }
    }
}