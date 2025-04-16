using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using xTile.Dimensions;
using StardewValley;
using Object = StardewValley.Object;

namespace FishSpotBait
{
	public partial class ModEntry
	{
		public class GameLocation_checkAction_Patch
		{
			public static bool Prefix(GameLocation __instance, Location tileLocation, Farmer who, ref bool __result)
			{
				if (!Config.ModEnabled || who.ActiveObject is null || who.ActiveObject.Category != Object.baitCategory)
					return true;

				Point fishSpotTile = Utility.Vector2ToPoint(Game1.currentCursorTile);

				if (!__instance.isWaterTile((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y) || __instance.Objects.ContainsKey(Game1.currentCursorTile) || !Utility.tileWithinRadiusOfPlayer((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y, Config.MaxRange, who))
				{
					if (!__instance.isWaterTile(tileLocation.X, tileLocation.Y) || __instance.Objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)))
					{
						return true;
					}
					else
					{
						fishSpotTile = new Point(tileLocation.X, tileLocation.Y);
					}
				}
				if (Config.RandomRadius <= 0 && __instance.fishSplashPoint.Value == fishSpotTile)
				{
					return true;
				}
				Player = who;
				Direction = GetPlayerDirectionTowardTile(who, fishSpotTile);
				if (Config.RandomRadius > 0)
				{
					List<Point> potentialTiles = new();

					for (int dx = -Config.RandomRadius; dx <= Config.RandomRadius; dx++)
					{
						for (int dy = -Config.RandomRadius; dy <= Config.RandomRadius; dy++)
						{
							Point potentialTile = new(fishSpotTile.X + dx, fishSpotTile.Y + dy);

							if (IsValidTile(__instance, potentialTile))
							{
								potentialTiles.Add(potentialTile);
							}
						}
					}
					if (potentialTiles.Count == 0)
					{
						Player = null;
						Direction = -1;
						return true;
					}
					fishSpotTile = potentialTiles[new Random().Next(potentialTiles.Count)];
				}
				if (!IsValidTile(__instance, fishSpotTile))
				{
					Point potentialTile = Direction switch
					{
						0 => new Point(fishSpotTile.X, fishSpotTile.Y - 1),
						1 => new Point(fishSpotTile.X + 1, fishSpotTile.Y),
						2 => new Point(fishSpotTile.X, fishSpotTile.Y + 1),
						3 => new Point(fishSpotTile.X - 1, fishSpotTile.Y),
						_ => fishSpotTile
					};

					if (potentialTile == fishSpotTile || !IsValidTile(__instance, potentialTile))
					{
						Player = null;
						Direction = -1;
						return true;
					}
					fishSpotTile = potentialTile;
				}
				SMonitor.Log($"Set fish spot point to {fishSpotTile}");
				who.reduceActiveItemByOne();
				__instance.playSound("dropItemInWater");
				__instance.fishSplashPoint.Value = fishSpotTile;
				__result = true;
				SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
				return false;
			}

			private static bool IsValidTile(GameLocation location, Point tile)
			{
				return location.isOpenWater(tile.X, tile.Y) && location.doesTileHaveProperty(tile.X, tile.Y, "NoFishing", "Back") is null && !location.Objects.ContainsKey(Utility.PointToVector2(tile));
			}
		}
	}
}
