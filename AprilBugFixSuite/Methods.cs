using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace AprilBugFixSuite
{
	public partial class ModEntry
	{
		private static bool IsModEnabled()
		{
			return Config.EnableMod && (!Config.RestrictToAprilFirst || (DateTime.Now.Month == 4 && DateTime.Now.Day == 1));
		}

		private readonly string[] _AsciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };

		public static void ShuffleList<T>(List<T> list)
		{
			int n = list.Count;

			while (n > 1)
			{
				n--;

				int k = Game1.random.Next(n + 1);

				(list[n], list[k]) = (list[k], list[n]);
			}
		}

		private List<string> ConvertToAscii(Color[] data, int width)
		{
			List<string> output = new();

			string line = "";
			for (int i = 0; i < data.Length; i++)
			{
				Color pixelColor = data[i];
				//Average out the RGB components to find the Gray Color
				int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
				int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
				int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
				Color grayColor = new(red, green, blue);
				int index = grayColor.R * 10 / 255;

				line += " ";
				line += _AsciiChars[index];
				if (line.Length == width * 2)
				{
					output.Add(line);
					line = "";
				}
			}
			return output;
		}
	}
}
