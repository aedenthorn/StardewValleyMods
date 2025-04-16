using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace RainbowTrail
{
	public partial class ModEntry
	{
		public class Farmer_draw_Patch
		{
			public static void Prefix(Farmer __instance, SpriteBatch b)
			{
				if (!Config.ModEnabled)
					return;

				trailDictionary.TryGetValue(__instance.UniqueMultiplayerID, out List<RainbowTrailElement> rainbowTrailElements);
				if (IsRainbowTrailActive(__instance))
				{
					if (rainbowTrailElements is null)
					{
						rainbowTrailElements = new();
						trailDictionary.Add(__instance.UniqueMultiplayerID, rainbowTrailElements);
					}
					rainbowTrailElements.Add(new RainbowTrailElement(__instance.Position, __instance.FacingDirection));
				}
				if (rainbowTrailElements is not null)
				{
					for (int i = 0; i < rainbowTrailElements.Count; i++)
					{
						Vector2 position = rainbowTrailElements[i].Position;
						int direction = rainbowTrailElements[i].Direction;
						int duration = rainbowTrailElements[i].Duration;

						duration -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
						if (duration > 0)
						{
							rainbowTrailElements[i].Duration = duration;
							if (position != __instance.Position && (i == 0 || rainbowTrailElements[i - 1].Position != position))
							{
								if (direction == 0 && i > Config.MaxDuration - 1)
									continue;

								Point destinationSize = new(128, 128);
								Rectangle? sourceRectangle = null;
								float offsetX = 0;
								float offsetY = 0;
								float rotation = 0;
								int maxRange = 48;
								int rangeX = (int)Math.Abs(position.X - __instance.Position.X);
								int rangeY = (int)Math.Abs(position.Y - __instance.Position.Y);

								if (direction % 2 == 1)
								{
									if (__instance.FacingDirection == 1 && rangeY < 32 && rangeX < maxRange)
									{
										sourceRectangle = new(0, 0, 64 - maxRange + rangeX, 64);
										destinationSize = new(sourceRectangle.Value.Size.X * 2, sourceRectangle.Value.Size.Y * 2);
									}
									else if (__instance.FacingDirection == 3 && rangeY < 32 && rangeX < maxRange)
									{
										sourceRectangle = new(maxRange - rangeX, 0, maxRange + rangeX, 64);
										offsetX = 2 * (rangeX - maxRange);
										destinationSize = new(sourceRectangle.Value.Size.X * 2, sourceRectangle.Value.Size.Y * 2);
									}
								}
								else
								{
									destinationSize = new(128, 64);
								}
								if (direction == 0)
								{
									rotation = (float)Math.PI / 2f;
									offsetX -= 96;
									offsetY += 8;
								}
								else if (direction == 2)
								{
									rotation = (float)Math.PI * 3 / 2f;
									offsetX += 96;
									offsetY -= 118;
								}
								b.Draw(rainbowTexture, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(position) - new Vector2(32f + offsetX - (direction == 2 ? 128 : 0), 88 + offsetY)), destinationSize), sourceRectangle, Color.White * (0.5f - (Config.MaxDuration - i) / Config.MaxDuration / 2), rotation, Vector2.Zero, SpriteEffects.None, position.Y / 10000f - 0.005f + (__instance.FacingDirection == 0 ? 0.0067f : 0) - (Config.MaxDuration + 1 - i) / 10000f);
							}
						}
						else
						{
							rainbowTrailElements.RemoveAt(i--);
						}
					}
					if (rainbowTrailElements.Count == 0)
					{
						trailDictionary.Remove(__instance.UniqueMultiplayerID);
					}
				}
			}
		}
	}
}
