using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AprilBugFixSuite
{
	public class Shader
	{
		private readonly int sourceTextureWidth;
		private readonly int sourceTextureHeight;
		private readonly int scaleFactor;
		private readonly int newWidth;
		private readonly int newHeight;
		private readonly Color[] sourceData;
		private readonly Color[] resizedData;
		private readonly int[] sourceX;
		private readonly int[] sourceY;
		private readonly Texture2D resizedTexture;
		private static Shader cache;

		public Shader(Texture2D sourceTexture, int scale)
		{
			sourceTextureWidth = sourceTexture.Width;
			sourceTextureHeight = sourceTexture.Height;
			scaleFactor = scale;
			newWidth = sourceTexture.Width / scaleFactor;
			newHeight = sourceTexture.Height / scaleFactor;
			sourceData = new Color[sourceTexture.Width * sourceTexture.Height];
			resizedData = new Color[newWidth * newHeight];
			sourceX = new int[newWidth];
			sourceY = new int[newHeight];
			resizedTexture = new Texture2D(sourceTexture.GraphicsDevice, newWidth, newHeight);
			for (int x = 0; x < newWidth; x++)
			{
				sourceX[x] = x * scaleFactor + scaleFactor / 2;
			}
			for (int y = 0; y < newHeight; y++)
			{
				sourceY[y] = y * scaleFactor + scaleFactor / 2;
			}
		}

		public static Texture2D DownScale(Texture2D sourceTexture, int scaleFactor)
		{
			if (cache is null || sourceTexture.Width != cache.sourceTextureWidth || sourceTexture.Height != cache.sourceTextureHeight)
			{
				cache = new Shader(sourceTexture, scaleFactor);
			}
			return DownScale(sourceTexture);
		}

		public static Texture2D DownScale(Texture2D sourceTexture)
		{
			if (cache.scaleFactor == 1)
				return sourceTexture;

			sourceTexture.GetData(cache.sourceData);
			for (int y = 0; y < cache.newHeight; y++)
			{
				for (int x = 0; x < cache.newWidth; x++)
				{
					cache.resizedData[y * cache.newWidth + x] = cache.sourceData[cache.sourceY[y] * cache.sourceTextureWidth + cache.sourceX[x]];
				}
			}
			cache.resizedTexture.SetData(cache.resizedData);
			return cache.resizedTexture;
		}
	}
}
