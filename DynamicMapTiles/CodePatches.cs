using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using StardewModdingAPI.Utilities;
using StardewValley;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DynamicMapTiles
{
	public partial class ModEntry
	{
		private static PerScreen<Farmer> explodingFarmer = new();

		public class GameLocation_explode_Patch
		{
			public static void Prefix(Farmer who)
			{
				if (!Config.ModEnabled)
					return;
				explodingFarmer.Value = who;
			}
		}

		public class GameLocation_explosionAt_Patch
		{
			public static void Postfix(GameLocation __instance, float x, float y)
			{
				if (!Config.ModEnabled || (!Config.TriggerDuringEvents && Game1.eventUp) || !__instance.isTileOnMap(new Vector2(x, y)))
					return;
				foreach(var layer in __instance.map.Layers)
				{
					var tile = layer.Tiles[(int)x, (int)y];
					if (tile is null)
						continue;
					if (tile.Properties.TryGetValue(explodeKey, out PropertyValue mail))
					{
						if(explodingFarmer.Value is not null && explodingFarmer.Value.currentLocation.Name == __instance.Name)
						{
							if (!string.IsNullOrEmpty(mail) && !explodingFarmer.Value.mailReceived.Contains(mail))
							{
								explodingFarmer.Value.mailReceived.Add(mail);
							}
							TriggerActions(new List<Layer>() { tile.Layer }, explodingFarmer.Value, new Point((int)x, (int)y), new List<string>() { "Explode" });
						}
						layer.Tiles[(int)x, (int)y] = null;
					}
				}
			}
		}

		public class GameLocation_isCollidingPosition_Patch
		{
			public static bool Prefix(GameLocation __instance, Rectangle position, ref bool __result)
			{
				if (!Config.ModEnabled || !pushingDict.TryGetValue(__instance.Name, out List<PushedTile> tiles))
					return true;
				foreach (var tile in tiles)
				{
					Rectangle tileRect = new(tile.position, new Point(64, 64));
					if (position.Intersects(tileRect))
					{
						__result = true;
						return false;
					}
				}
				return true;
			}
		}

		public class GameLocation_draw_Patch
		{
			public static void Postfix(GameLocation __instance)
			{
				if (!Config.ModEnabled || !pushingDict.TryGetValue(__instance.Name, out List<PushedTile> tiles))
					return;
				foreach(var tile in tiles)
				{
					Game1.mapDisplayDevice.DrawTile(tile.tile, new Location(tile.position.X - Game1.viewport.X, tile.position.Y - Game1.viewport.Y), (float)(tile.position.Y + 64 + (tile.tile.Layer.Id.Contains("Front") ? 16 : 0)) / 10000f);
				}
			}
		}

		public class GameLocation_performToolAction_Patch
		{
			public static bool Prefix(GameLocation __instance, Tool t, int tileX, int tileY, ref bool __result)
			{
				if (!Config.ModEnabled || t is null || t.getLastFarmerToUse() is null || !__instance.isTileOnMap(new Vector2(tileX, tileY)))
					return true;

				if(TriggerActions(__instance.Map.Layers.ToList(), t.getLastFarmerToUse(), new Point(tileX, tileY), new List<string>() { t.GetType().Name, t.Name }))
				{
					__result = true;
					return false;
				}
				return true;
			}
		}

		public class GameLocation_checkAction_Patch
		{
			public static bool Prefix(GameLocation __instance, Location tileLocation, Farmer who, ref bool __result)
			{
				if (!Config.ModEnabled || !__instance.isTileOnMap(new Vector2(tileLocation.X, tileLocation.Y)))
					return true;
				if ((who.ActiveObject is not null && TriggerActions(__instance.Map.Layers.ToList(), who, new Point(tileLocation.X, tileLocation.Y), new List<string>() { "Object" + who.ActiveObject.Name, "Object" + who.ActiveObject.ParentSheetIndex })) || TriggerActions(__instance.Map.Layers.ToList(), who, new Point(tileLocation.X, tileLocation.Y), new List<string>() { "Action" }))
				{
					__result = true;
					return false;
				}
				return true;
			}
		}

		public class Farmer_getMovementSpeed_Patch
		{
			public static void Postfix(Farmer __instance, ref float __result)
			{
				if (!Config.ModEnabled || (!Config.TriggerDuringEvents && Game1.eventUp) || __instance.currentLocation is null)
					return;
				var tileLoc = __instance.Tile;
				if (__instance.currentLocation.isTileOnMap(tileLoc))
				{
					var tile = __instance.currentLocation.Map.GetLayer("Back").Tiles[(int)tileLoc.X, (int)tileLoc.Y];
					if (tile is not null && tile.Properties.TryGetValue(speedKey, out PropertyValue value) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float mult))
					{
						__result *= mult;
					}
				}
			}
		}

		public class Farmer_MovePosition_Patch
		{
			public static void Prefix(Farmer __instance, ref Vector2[] __state)
			{
				if (!Config.ModEnabled || (!Config.TriggerDuringEvents && Game1.eventUp) || __instance.currentLocation is null)
					return;
				var tileLoc = __instance.Tile;
				if (__instance.currentLocation.isTileOnMap(tileLoc))
				{
					var tile = __instance.currentLocation.Map.GetLayer("Back").Tiles[(int)tileLoc.X, (int)tileLoc.Y];
					if(tile is not null && tile.Properties.TryGetValue(moveKey, out PropertyValue value))
					{
						var split = value.ToString().Split(' ');
						__instance.xVelocity = float.Parse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture);
						__instance.yVelocity = float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
				}
				__state = new Vector2[] { __instance.Position, tileLoc };
			}

			public static void Postfix(Farmer __instance, Vector2[] __state)
			{
				if (!Config.ModEnabled || (!Config.TriggerDuringEvents && Game1.eventUp) || __state is null || __instance.currentLocation is null)
					return;
				var tilePos = __instance.TilePoint;
				var oldTile = Utility.Vector2ToPoint(__state[1]);
				if(oldTile != tilePos)
				{
					DoStepOffActions(__instance, oldTile);
					DoStepOnActions(__instance, tilePos);
				}

				if (__instance.currentLocation.isTileOnMap(tilePos.ToVector2()) && __instance.currentLocation.isTileOnMap(tilePos.ToVector2()))
				{
					var backTile = __instance.currentLocation.Map.GetLayer("Back").Tiles[tilePos.X, tilePos.Y];
					var backOldTile = __instance.currentLocation.Map.GetLayer("Back").Tiles[oldTile.X, oldTile.Y];
					if (backTile != null && backTile.Properties.TryGetValue(slipperyKey, out PropertyValue value) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float amount))
					{
						if (__instance.movementDirections.Contains(0))
							__instance.yVelocity += amount;
						if (__instance.movementDirections.Contains(1))
							__instance.xVelocity += amount;
						if (__instance.movementDirections.Contains(2))
							__instance.yVelocity -= amount;
						if (__instance.movementDirections.Contains(3))
							__instance.xVelocity -= amount;
					}
					else if (backOldTile != null && backOldTile.Properties.TryGetValue(slipperyKey, out value) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
					{
						__instance.xVelocity = 0;
						__instance.yVelocity = 0;
					}
				}

				if (__instance.movementDirections.Any() && __state[0] == __instance.Position)
				{
					var startTile = new Point(__instance.GetBoundingBox().Center.X / 64, __instance.GetBoundingBox().Center.Y / 64);
					startTile += GetNextTile(__instance.FacingDirection);
					Point start = new(startTile.X * 64, startTile.Y * 64);
					var startLoc = new Location(start.X, start.Y);

					var build = __instance.currentLocation.Map.GetLayer("Buildings");
					var tile = build.PickTile(startLoc, Game1.viewport.Size);

					if (tile is not null && tile.Properties.TryGetValue(pushKey, out PropertyValue tiles))
					{
						var destTile = startTile + GetNextTile(__instance.FacingDirection);
						foreach (var item in tiles.ToString().Split(','))
						{
							var split = item.Split(' ');
							if (split.Length == 2 && int.TryParse(split[0], out int x) && int.TryParse(split[1], out int y) && destTile.X == x && destTile.Y == y)
							{
								PushTileWithOthers(__instance, tile, startTile);
								break;
							}
						}
					}
				}
			}
		}
	}
}
