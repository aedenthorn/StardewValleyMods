using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using Object = StardewValley.Object;

namespace CraftableButterflyHutches
{
	public partial class ModEntry
	{
		private static void CreateTextureFile()
		{
			Texture2D texture = new(Game1.graphics.GraphicsDevice, 32, 48);
			Texture2D sourceTexture = Game1.content.Load<Texture2D>("TileSheets\\furniture");
			Color[] data = new Color[texture.Width * texture.Height];
			Color[] sourceData = new Color[sourceTexture.Width * sourceTexture.Height];

			texture.GetData(data);
			sourceTexture.GetData(sourceData);
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					data[i + j * texture.Width] = sourceData[304 + i + (976 + j) * sourceTexture.Width];
				}
			}
			texture.SetData(data);

			Directory.CreateDirectory(assetDirectory);
			Stream stream = File.Create(textureFile);

			texture.SaveAsPng(stream, texture.Width, texture.Height);
			stream.Dispose();
		}

		private static (int, int) GetRecipeTilesheetSize(CraftingRecipe recipe)
		{
			if (!Game1.content.Load<Dictionary<string, string>>("Data\\Furniture").TryGetValue(recipe.name, out string text))
				return (0, 0);

			string[] array = text.Split('/');

			if (!ArgUtility.TryGet(array, 2, out string tilesheetSize, out string error))
			{
				typeof(CraftingRecipe).GetMethod("LogParseError", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(recipe, new object[] {text, error});
				return (0, 0);
			}

			string[] size = ArgUtility.SplitBySpace(tilesheetSize);

			if (!ArgUtility.TryGetInt(size, 0, out int width, out error) || !ArgUtility.TryGetInt(size, 1, out int height, out error))
			{
				typeof(CraftingRecipe).GetMethod("LogParseError", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(recipe, new object[] {text, error});
				return (0, 0);
			}
			return (width, height);
		}

		private static bool IsCraftableButterflyHutches(Object obj)
		{
			return obj.QualifiedItemId.Equals("(F)aedenthorn.CraftableButterflyHutches_ButterflyHutch");
		}

		private static void DelayedShowCraftableButterflyHutches(GameLocation environment)
		{
			DelayedAction.functionAfterDelay(() => AddButterflies(environment), 100);
		}

		private static void AddButterflies(GameLocation location)
		{
			context.Monitor.Log($"Showing butterflies for {location.Name}");
			Random random = new();

			if (location.critters is not null)
			{
				for (int i = location.critters.Count - 1; i >= 0; i--)
				{
					if (location.critters[i]?.sprite is not null)
					{
						int x = location.critters[i].sprite.SourceRect.X;
						int y = location.critters[i].sprite.SourceRect.Y;

						if (((y == 96 || y == 112) && (x == 128 || x == 144 || x == 160 || x == 176 || x == 192 || x == 208 || x == 224 || x == 240 || x == 256 || x == 272 || x == 288 || x == 304)) || ((y == 128 || y == 144) && (x == 0 || x == 16 || x == 32 || x == 48 || x == 64 || x == 80 || x == 96 || x == 112 || x == 128)) || (y == 288 && (x == 64 || x == 64 || x == 80 || x == 96 || x == 112 || x == 128 || x == 144 || x == 160 || x == 176 || x == 192 || x == 208 || x == 224 || x == 240 || x == 256 || x == 272 || x == 288 || x == 304)) || (y == 304 && (x == 224 || x == 240 || x == 256 || x == 272 || x == 288 || x == 304)) || (y == 336 && (x == 128 || x == 144 || x == 160 || x == 176 || x == 192 || x == 208 || x == 224 || x == 240)) || (y == 384 && (x == 0 || x == 16 || x == 32 || x == 48 || x == 64 || x == 80 || x == 96 || x == 112)))
						{
							location.critters.RemoveAt(i);
						}
					}
				}
			}
			location.instantiateCrittersList();
			if (location.furniture.Any(f => f.QualifiedItemId.Equals("(F)aedenthorn.CraftableButterflyHutches_ButterflyHutch")))
			{
				int area = location.map.Layers[0].TileSize.Area;
				int minButterflies = (int)(Config.MinDensity * area);
				int maxButterflies = (int)(Math.Max(Config.MinDensity, Config.MaxDensity) * area) + 1;
				int numberOfButterflies = random.Next(minButterflies, Math.Max(minButterflies + 1, maxButterflies));

				SMonitor.Log($"Number of butterflies: {numberOfButterflies} ({minButterflies}, {maxButterflies})");
				for (int i = 0; i < numberOfButterflies; i++)
				{
					location.addCritter(new Butterfly(location, location.getRandomTile()).setStayInbounds(true));
				}
			}
		}
	}
}
