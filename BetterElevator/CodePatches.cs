using xTile.Dimensions;
using xTile.Tiles;
using StardewValley;
using StardewValley.Locations;

namespace BetterElevator
{
	public partial class ModEntry
	{
		public class GameLocation_performAction_Patch
		{
			public static bool Prefix(string fullActionString, Farmer who, ref bool __result)
			{
				if (!Config.ModEnabled || fullActionString == null || !who.IsLocalPlayer || !SHelper.Input.IsDown(Config.ModKey))
					return true;
				if (!Config.Unrestricted && MineShaft.lowestLevelReached < (Game1.player.currentLocation.Name == "SkullCave" ? 121 : 1))
					return true;

				string[] actionParams = fullActionString.Split(' ');
				string action = actionParams[0];

				if (action == "SkullDoor")
				{
					if (!who.hasSkullKey || !who.hasUnlockedSkullDoor)
					{
						return true;
					}
				}
				else if (action == "Mine" && actionParams.Length > 1 && actionParams[1] == "77377")
				{
					return true;
				}
				else if (action != "Mine")
				{
					return true;
				}
				Game1.activeClickableMenu = new BetterElevatorMenu();
				__result = true;
				return false;
			}
		}

		public class MineShaft_checkAction_Patch
		{
			public static bool Prefix(MineShaft __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
			{
				if (!Config.ModEnabled || !who.IsLocalPlayer)
					return true;

				Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);

				if (tile == null)
				{
					return true;
				}
				if (tile.TileIndex == 115)
				{
					if (!SHelper.Input.IsDown(Config.ModKey))
					{
						return true;
					}
					if (__instance.mineLevel == 77377)
					{
						return true;
					}
					Game1.activeClickableMenu = new BetterElevatorMenu();
					__result = true;
					return false;
				}
				if (tile.TileIndex == 173)
				{
					if (__instance.mineLevel == 77376)
					{
						Game1.enterMine(__instance.mineLevel + 2);
						__instance.playSound("stairsdown");
						__result = true;
						return false;
					}
					if (__instance.mineLevel == int.MaxValue)
					{
						Game1.enterMine(__instance.mineLevel);
						__instance.playSound("stairsdown");
						__result = true;
						return false;
					}
				}
				return true;
			}
		}

		public class MineShaft_shouldCreateLadderOnThisLevel_Patch
		{
			public static void Postfix(MineShaft __instance, ref bool __result)
			{
				if (!Config.ModEnabled)
					return;

				if (__instance.mineLevel == int.MaxValue)
				{
					__result = false;
				}
			}
		}
	}
}
