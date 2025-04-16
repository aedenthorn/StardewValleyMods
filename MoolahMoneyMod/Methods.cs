using System.Numerics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MoolahMoneyMod
{
	public partial class ModEntry
	{
		private static bool shouldUpdateTotalMoolahEarned = true;

		public static void SetMoolah(Farmer who, BigInteger value)
		{
			int money = StoreOverflowAndClampMoney(who, value);

			shouldUpdateTotalMoolahEarned = false;
			Game1.player.Money = money;
			shouldUpdateTotalMoolahEarned = true;
		}

		public static int StoreOverflowAndClampMoney(Farmer __instance, BigInteger value)
		{
			UpdateTotalMoolahEarned(__instance, value);
			if (value > int.MaxValue)
			{
				__instance.modData[moolahKey] = (value - int.MaxValue).ToString();
				value = int.MaxValue;
			}
			else
			{
				__instance.modData[moolahKey] = "0";
			}
			return (int)value;
		}

		private static void UpdateTotalMoolahEarned(Farmer __instance, BigInteger value)
		{
			if (Context.IsWorldReady && shouldUpdateTotalMoolahEarned)
			{
				BigInteger moolahEarned = value - GetMoolah(__instance);

				if (moolahEarned > 0)
				{
					__instance.modData[totalMoolahEarnedKey] = (GetTotalMoolahEarned(__instance) + moolahEarned).ToString();
				}
			}
		}

		public static BigInteger GetMoolah(Farmer who)
		{
			BigInteger moolah = who.Money;

			if (who.modData.TryGetValue(moolahKey, out string moolahString))
			{
				moolah += BigInteger.Parse(moolahString);
			}
			return moolah;
		}

		public static BigInteger GetTotalMoolahEarned(Farmer who)
		{
			if (who.modData.TryGetValue(totalMoolahEarnedKey, out string totalMoolahEarnedString))
			{
				return BigInteger.Parse(totalMoolahEarnedString);
			}
			return who.totalMoneyEarned;
		}

		private static void DrawMoneyDial(MoneyDial moneyDial, SpriteBatch b, Vector2 position, BigInteger target, int index = -1)
		{
			MoneyDialData data = (0 <= index && index < moneyDialDataList.Value.Count) ? moneyDialDataList.Value[index] : moneyDialData.Value;
			int numDigits = data.currentValue.ToString().Length;

			if (data.previousTargetValue != target)
			{
				data.speed = (target - data.currentValue) / 100;
				data.previousTargetValue = target;
				data.soundTimer = BigInteger.Max(6, 100 / (BigInteger.Abs(data.speed) + 1));
			}
			if (data.moneyShineTimer > 0 && data.currentValue == target)
			{
				data.moneyShineTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (data.moneyMadeAccumulator > 0)
			{
				data.moneyMadeAccumulator -= (BigInteger.Abs(data.speed / 2) + 1) * ((moneyDial.animations.Count > 0) ? 1 : 100);
				if (data.moneyMadeAccumulator <= 0)
				{
					data.moneyShineTimer = numDigits * 60;
				}
			}
			if (moneyDial.ShouldShakeMainMoneyBox && data.moneyMadeAccumulator > 2000)
			{
				Game1.dayTimeMoneyBox.moneyShakeTimer = 100;
			}
			if (data.currentValue != target)
			{
				data.currentValue += data.speed + ((data.currentValue < target) ? 1 : (-1));
				if (data.currentValue < target)
				{
					data.moneyMadeAccumulator += BigInteger.Abs(data.speed);
				}
				data.soundTimer--;
				if (BigInteger.Abs(target - data.currentValue) <= data.speed + 1 || (data.speed != 0 && (target - data.currentValue).Sign != data.speed.Sign))
				{
					data.currentValue = target;
				}
				if (data.soundTimer <= 0)
				{
					moneyDial.onPlaySound?.Invoke((target - moneyDial.currentValue).Sign);
					data.soundTimer = BigInteger.Max(6, 100 / (BigInteger.Abs(data.speed) + 1));
					if (Game1.random.NextDouble() < 0.4)
					{
						if (target > data.currentValue)
						{
							moneyDial.animations.Add(TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(Game1.random.Next(10, 12), position + new Vector2(Game1.random.Next(30, 190), Game1.random.Next(-32, 48)), Color.Gold));
						}
						else if (target < data.currentValue)
						{
							TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(356, 449, 1, 1), 999999f, 1, 44, position + new Vector2(Game1.random.Next(160), Game1.random.Next(-32, 32)), false, false, 1f, 0.01f, Color.White, Game1.random.Next(1, 3) * 4, -0.001f, 0f, 0f);

							temporaryAnimatedSprite.motion = new Vector2(Game1.random.Next(-30, 40) / 10f, Game1.random.Next(-30, -5) / 10f);
							temporaryAnimatedSprite.acceleration = new Vector2(0f, 0.25f);
							moneyDial.animations.Add(temporaryAnimatedSprite);
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
					moneyDial.animations[i].draw(b, true);
				}
			}

			int xPosition = (numDigits > 8) ? -(numDigits - 8) * 24 : (8 - numDigits) * 24;
			bool significant = false;
			bool showSeparator = !string.IsNullOrEmpty(Config.Separator);

			numDigits = data.currentValue.ToString().Length;
			for (int i = 0; i < numDigits; i++)
			{
				int currentDigit = int.Parse(data.currentValue.ToString()[i].ToString());

				if (currentDigit > 0 || i == numDigits - 1)
				{
					significant = true;
				}
				if (significant)
				{
					if (showSeparator && i < numDigits - 1 && (numDigits - (i + 1)) % Config.SeparatorInterval == 0)
					{
						SpriteText.drawString(b, Config.Separator, (int)position.X + xPosition + Config.SeparatorX, (int)position.Y + Config.SeparatorY);
					}
					b.Draw(Game1.mouseCursors, position + new Vector2(xPosition, 0f), new Rectangle(286, 502 - currentDigit * 8, 5, 8), Color.Maroon, 0f, Vector2.Zero, 4f + ((data.moneyShineTimer / 60 == numDigits - i) ? 0.3f : 0f), SpriteEffects.None, 1f);
				}
				xPosition += 24;
			}
			if (0 <= index && index < moneyDialDataList.Value.Count)
			{
				moneyDialDataList.Value[index] = data;
			}
			else
			{
				moneyDialData.Value = data;
			}
		}

		public static string GetNumberWithSeparator(BigInteger number)
		{
			if (string.IsNullOrEmpty(Config.Separator))
				return number.ToString();

			string numberAsString = new(number.ToString() ?? "");
			int numDigits = numberAsString.Length;
			StringBuilder result = new();

			for (int i = numDigits - 1; i >= 0; i--)
			{
				result.Insert(0, numberAsString[i]);
				if (i > 0 && (numDigits - i) % Config.SeparatorInterval == 0)
				{
					result.Insert(0, Config.Separator);
				}
			}
			return result.ToString();
		}

		private static string CheckIntToString(string input)
		{
			if (!Config.ModEnabled || input.Length <= Config.SeparatorInterval)
				return input;

			string output = "";
			bool showSeparator = !string.IsNullOrEmpty(Config.Separator);

			for (int i = 0; i < input.Length; i++)
			{
				output += input[i];
				if (showSeparator && input[i] != '-' && i < input.Length - 1 && (input.Length - (i + 1)) % Config.SeparatorInterval == 0)
				{
					output += Config.Separator;
				}
			}
			return output;
		}

		private static string GetMoney(Farmer who)
		{
			if (!Config.ModEnabled)
				return Utility.getNumberWithCommas(who.Money);

			return FormatMoolah(GetMoolah(who));
		}

		private static string GetTotalMoneyEarned(Farmer who)
		{
			if (!Config.ModEnabled)
				return Utility.getNumberWithCommas((int)who.totalMoneyEarned);

			return FormatMoolah(GetTotalMoolahEarned(who));
		}

		private static string FormatMoolah(BigInteger value)
		{
			if (value.ToString().Length < 10)
			{
				return Utility.getNumberWithCommas((int)value);
			}
			return string.Format("{0:#.##E+0}", value);
		}
	}
}
