using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace CraftableTerrarium
{
	public partial class ModEntry
	{
		private static void CreateTextureFile()
		{
			Texture2D texture = new(Game1.graphics.GraphicsDevice, 48, 48);
			Texture2D sourceTexture = Game1.mouseCursors;
			Color[] data = new Color[texture.Width * texture.Height];
			Color[] sourceData = new Color[sourceTexture.Width * sourceTexture.Height];

			texture.GetData(data);
			sourceTexture.GetData(sourceData);
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					if (j < 11)
					{
						data[i + j * texture.Width] = Color.Transparent;
					}
					else
					{
						data[i + j * texture.Width] = sourceData[641 + i + (j + 1534 - 11) * sourceTexture.Width];
					}
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

		private static bool IsCraftableTerrarium(Object obj)
		{
			return obj.QualifiedItemId.Equals("(F)aedenthorn.CraftableTerrarium_Terrarium");
		}

		private static void DelayedShowCraftableTerrariums(GameLocation environment)
		{
			DelayedAction.functionAfterDelay(() => ShowFrogs(environment), 100);
		}

		private static void ShowFrogs(GameLocation location)
		{
			context.Monitor.Log($"Showing frogs for {location.Name}");
			for (int i = location.TemporarySprites.Count - 1; i >= 0; i--)
			{
				if (location.TemporarySprites[i] is TerrariumFrogs)
				{
					location.TemporarySprites.RemoveAt(i);
				}
			}
			foreach (Furniture furniture in location.furniture)
			{
				if (IsCraftableTerrarium(furniture))
				{
					context.Monitor.Log($"Showing {Config.Frogs} terrarium frogs for tile {furniture.TileLocation}");

					int i = 0;

					while (i++ < Config.Frogs)
					{
						bool which = Game1.random.NextDouble() > 0.5;
						Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");

						location.TemporarySprites.Add(new TerrariumFrogs(furniture.TileLocation)
						{
							texture = crittersText2,
							sourceRect = which ? new Rectangle(64, 224, 16, 16) : new Rectangle(64, 240, 16, 16),
							animationLength = 1,
							sourceRectStartingPos = which ? new Vector2(64f, 224f) : new Vector2(64f, 240f),
							interval = Game1.random.Next(100, 200),
							totalNumberOfLoops = 9999,
							position = (furniture.TileLocation - new Vector2(0 , 1)) * 64f + new Vector2((Game1.random.NextDouble() < 0.5) ? 22 : 25, (Game1.random.NextDouble() < 0.5) ? 2 : 1) * 4f + new Vector2(0, 42 + 42 / Config.Frogs * i),
							scale = 4f,
							flipped = Game1.random.NextDouble() < 0.5,
							layerDepth = (furniture.TileLocation.Y + 1f + 0.11f + 0.01f * i) * 64f / 10000f + 0.005f,
							Parent = location
						});
					}
					if (!string.IsNullOrEmpty(Config.Sound) && Game1.random.NextDouble() < 0.05 && Game1.timeOfDay > 610)
					{
						DelayedAction.playSoundAfterDelay(Config.Sound, Game1.random.Next(1000, 3000));
					}
				}
			}
		}
	}
}
