using Microsoft.Xna.Framework;
using xTile.Dimensions;
using xTile.Tiles;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using Rectangle = xTile.Dimensions.Rectangle;

namespace TrashCansOnHorse
{
	public partial class ModEntry
	{
		public class GameLocation_checkAction_Patch
		{
			public static bool Prefix(GameLocation __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
			{
				if (!Config.ModEnabled || !who.isRidingHorse())
					return true;

				Tile tile = __instance.map.RequireLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);

				if (tile == null || !tile.Properties.TryGetValue("Action", out string value))
				{
					value = __instance.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
				}
				if (value is not null)
				{
					string[] action = ArgUtility.SplitBySpace(value);

					if (ArgUtility.TryGet(action, 0, out string actionType, out _) && actionType == "Garbage")
					{
						Vector2 vector = new(tileLocation.X, tileLocation.Y);
						NPC npc = __instance.isCharacterAtTile(vector + new Vector2(0f, 1f));

						if (__instance.currentEvent is null && npc is not null && !npc.IsInvisible && !npc.IsMonster && npc is not Horse)
						{
							Point standingPixel = npc.StandingPixel;

							if (Utility.withinRadiusOfPlayer(standingPixel.X, standingPixel.Y, 1, who) && npc.checkAction(who, __instance))
							{
								if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, who.IsCarrying()))
								{
									who.faceGeneralDirection(Utility.PointToVector2(standingPixel), 0, opposite: false, useTileCalculations: false);
								}
								__result = true;
								return false;
							}
						}
						__result = __instance.performAction(value, who, tileLocation);
						return false;
					}
				}
				return true;
			}
		}
	}
}
