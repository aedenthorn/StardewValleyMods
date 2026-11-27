using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Dreamscape
{
	public partial class ModEntry
	{
		public const int fairyPositionX = 3;
		public const int fairyPositionY = 18;
		private static TemporaryAnimatedSprite fairySprite = null;
		private static bool fairyFlyingUp = true;
		private static int xLocationAfterWarpingToFarmhouse = -1;
		private static int yLocationAfterWarpingToFarmhouse = -1;

		public class EmilysParrot_doAction_Patch
		{
			public static bool Prefix(EmilysParrot __instance)
			{
				if (!Config.ModEnabled || __instance.GetType() != typeof(EmilysParrot))
					return true;

				if (Game1.player.CanMove)
				{
					Game1.playSound("parrot");
					Game1.player.CanMove = false;
					DelayedAction.functionAfterDelay(() => {
						if (Game1.currentLocation.Name.Equals($"{SModManifest.UniqueID}_Dreamscape"))
						{
							if (!Game1.player.friendshipData.TryGetValue("Emily", out Friendship friendship) || !friendship.IsMarried())
							{
								Game1.warpFarmer("HaleyHouse", 14, 5, 2);
								return;
							}

							FarmHouse farmHouse = Utility.getHomeOfFarmer(Game1.player);
							Layer layer = farmHouse.map.GetLayer("Buildings");

							SMonitor.Log($"Warping to FarmHouse");
							for (int x = 0; x < layer.LayerWidth; x++)
							{
								for (int y = 0; y < layer.LayerHeight; y++)
								{
									if (layer.Tiles[new Location(x, y)]?.TileIndex == 2173)
									{
										xLocationAfterWarpingToFarmhouse = x;
										yLocationAfterWarpingToFarmhouse = y + 1;
										Game1.warpFarmer("FarmHouse", x, y + 1, 2);
										return;
									}
								}
							}
							Game1.warpFarmer("FarmHouse", farmHouse.getEntryLocation().X, farmHouse.getEntryLocation().Y, 2);
							return;
						}
						else
						{
							SMonitor.Log($"Warping to Dreamscape");
							Game1.warpFarmer($"{SModManifest.UniqueID}_Dreamscape", 21, 15, 2);
							return;
						}
					}, 600);
				}
				return false;
			}
		}

		public class GameLocation_resetLocalState_Patch
		{
			public static void Postfix(GameLocation __instance)
			{
				if (!Config.ModEnabled || !__instance.Name.Equals($"{SModManifest.UniqueID}_Dreamscape"))
					return;

				SMonitor.Log($"Building Emily's parrot");
				fairySprite = new TemporaryAnimatedSprite(Game1.mouseCursors.Name, new Rectangle(16, 592, 16, 16), 128, 4, 1, new Vector2(fairyPositionX, fairyPositionY) * Game1.tileSize + new Vector2(32, 0), false, false) {
					scale = Game1.pixelZoom,
					drawAboveAlwaysFront = true,
					destroyable = false
				};
				__instance.temporarySprites.Add(new EmilysParrot(new Vector2(21 * Game1.tileSize, 9 * Game1.tileSize)));
				__instance.temporarySprites.Add(fairySprite);
			}
		}

		public class GameLocation_checkAction_Patch
		{
			public static bool Prefix(GameLocation __instance, Location tileLocation, Farmer who)
			{
				if (!Config.ModEnabled || !Game1.currentLocation.Name.Equals($"{SModManifest.UniqueID}_Dreamscape"))
					return true;

				if (tileLocation.X == 21 && tileLocation.Y == 9)
				{
					TemporaryAnimatedSprite temporaryAnimatedSprite = __instance.getTemporarySpriteByID(5858585);

					if (temporaryAnimatedSprite is EmilysParrot emilysParrot)
					{
						emilysParrot.doAction();
						return false;
					}
				}
				else if (fairySprite is not null && fairySprite.shakeIntensity <= 0f && new Rectangle(fairyPositionX, fairyPositionY + 2, 2, 2).Contains(new Point(tileLocation.X, tileLocation.Y)))
				{
					fairySprite.shakeIntensity = 16;
					fairySprite.shakeIntensityChange = -0.05f;
					__instance.localSound("yoba");
					who.health = who.maxHealth;
					who.stamina = who.MaxStamina;
				}
				return true;
			}
		}

		public class GameLocation_UpdateWhenCurrentLocation_Patch
		{
			public static void Postfix()
			{
				if (!Config.ModEnabled)
					return;

				if (fairySprite is not null)
				{
					if (fairyFlyingUp)
					{
						if (fairySprite.Position.Y > (fairyPositionY - 0.5f) * Game1.tileSize)
						{
							fairySprite.Position += new Vector2(0, -0.5f);
						}
						else
						{
							fairyFlyingUp = false;
						}
					}
					else
					{
						if (fairySprite.Position.Y < (fairyPositionY + 0.5f) * Game1.tileSize)
						{
							fairySprite.Position += new Vector2(0, 0.5f);
						}
						else
						{
							fairyFlyingUp = true;
						}
					}
				}
			}
		}

		public class GameLocation_doesTileSinkDebris_Patch
		{
			public static bool Prefix(GameLocation __instance, int xTile, int yTile, ref bool __result)
			{
				if (!Config.ModEnabled || !__instance.Name.Equals($"{SModManifest.UniqueID}_Dreamscape"))
					return true;

				Tile tileBack = __instance.map.GetLayer("Back").PickTile(new Location(xTile * Game1.tileSize, yTile * Game1.tileSize), Game1.viewport.Size);
				Tile tileBack3 = __instance.map.GetLayer("Back3").PickTile(new Location(xTile * Game1.tileSize, yTile * Game1.tileSize), Game1.viewport.Size);

				if ((tileBack is null || (tileBack is not null && tileBack.TileIndex != 15 && tileBack.TileIndex != 26)) && tileBack3 is null)
				{
					__result = true;
				}
				return false;
			}
		}

		public class GameLocation_sinkDebris_Patch
		{
			public static bool Prefix(GameLocation __instance, Debris debris, ref bool __result)
			{
				if (!Config.ModEnabled || !__instance.Name.Equals($"{SModManifest.UniqueID}_Dreamscape"))
					return true;

				if (debris.isEssentialItem())
				{
					__result = false;
					return false;
				}
				if (debris.item is not null && debris.item.HasContextTag("book_item"))
				{
					__result = false;
					return false;
				}
				if (debris.debrisType.Value == Debris.DebrisType.OBJECT && debris.chunkType.Value == 74)
				{
					__result = false;
					return false;
				}
				if (debris.debrisType.Value == Debris.DebrisType.CHUNKS)
				{
					__result = false;
					return false;
				}
				Game1.sounds.PlayLocal("throw", __instance, null, null, StardewValley.Audio.SoundContext.Default, out ICue cue);
				cue.Volume = 0.15f;
				__result = true;
				return false;
			}
		}

		public class HoeDirt_draw_Patch
		{
			public static void Prefix(ref Texture2D ___texture)
			{
				if (!Config.ModEnabled || !Game1.currentLocation.Name.Equals($"{SModManifest.UniqueID}_Dreamscape"))
					return;

				___texture = HoeDirt.snowTexture;
			}
		}

		public class FarmHouse_resetLocalState_Patch
		{
			public static void Postfix()
			{
				if (!Config.ModEnabled)
					return;

				if (xLocationAfterWarpingToFarmhouse > 0 && yLocationAfterWarpingToFarmhouse > 0)
				{
					Game1.player.Position = new Vector2(xLocationAfterWarpingToFarmhouse, yLocationAfterWarpingToFarmhouse) * Game1.tileSize;
					Game1.xLocationAfterWarp = Game1.player.TilePoint.X;
					Game1.yLocationAfterWarp = Game1.player.TilePoint.Y;
				}
				xLocationAfterWarpingToFarmhouse = -1;
				yLocationAfterWarpingToFarmhouse = -1;
			}
		}
	}
}
