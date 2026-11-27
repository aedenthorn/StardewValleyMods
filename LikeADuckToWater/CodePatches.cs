using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LikeADuckToWater
{
	public partial class ModEntry
	{
		public class FarmAnimal_updatePerTenMinutes_Patch
		{
			public static void Postfix(FarmAnimal __instance)
			{
				if (NotReadyToSwim(__instance))
					return;

				TryAddToQueue(__instance, __instance.currentLocation);
			}
		}

		public class FarmAnimal_MovePosition_Patch
		{
			public static void Prefix(FarmAnimal __instance, ref Vector2 __state)
			{
				if (!Config.ModEnabled || !__instance.CanSwim())
					return;

				__state = __instance.Position;
			}

			public static void Postfix(FarmAnimal __instance, GameLocation currentLocation, Vector2 __state)
			{
				if (!Config.ModEnabled || !__instance.CanSwim())
					return;

				if(__instance.hopOffset == Vector2.Zero && __state != __instance.Position)
				{
					__instance.isSwimming.Value = currentLocation.isWaterTile(__instance.TilePoint.X, __instance.TilePoint.Y);
				}
			}
		}

		public class FarmAnimal_Eat_Patch
		{
			public static void Postfix(FarmAnimal __instance)
			{
				if (NotReadyToSwim(__instance))
					return;

				TryAddToQueue(__instance, __instance.currentLocation);
			}
		}

		public class FarmAnimal_pet_Patch
		{
			public static void Postfix(FarmAnimal __instance, bool is_auto_pet)
			{
				if (is_auto_pet || NotReadyToSwim(__instance))
					return;

				TryAddToQueue(__instance, __instance.currentLocation);
			}
		}

		public class FarmAnimal_HandleCollision_Patch
		{
			public static void Prefix(FarmAnimal __instance, out bool __state)
			{
				__state = __instance.isSwimming.Value;
			}

			public static void Postfix(FarmAnimal __instance, bool __state, ref bool __result)
			{
				if (__instance.controller is not null && !__result)
				{
					__instance.Halt();
					__instance.Sprite.StopAnimation();
					__result = true;
				}
				if (__state && !__instance.isSwimming.Value)
				{
					__instance.controller = null;
					__instance.behaviors(Game1.currentGameTime, __instance.currentLocation);
				}
			}

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> instructionsList = instructions.ToList();

				for (int i = 0; i < instructionsList.Count; i++)
				{
					if (instructionsList[i].opcode == OpCodes.Ldfld && instructionsList[i].operand.Equals(typeof(FarmAnimal).GetField(nameof(FarmAnimal.wasPet))))
					{
						instructionsList[i].opcode = OpCodes.Call;
						instructionsList[i].operand = typeof(FarmAnimal_HandleCollision_Patch).GetMethod(nameof(WasPet));
						if (i + 1 < instructionsList.Count)
						{
							instructionsList.RemoveAt(i + 1);
						}
						break;
					}
				}
				return instructionsList;
			}

			public static bool WasPet(FarmAnimal __instance)
			{
				return __instance.wasPet.Value || __instance.wasAutoPet.Value;
			}
		}

		public class GameLocation_isCollidingPosition_Patch
		{
			public static bool Prefix(GameLocation __instance, Rectangle position, Character character, ref bool __result)
			{
				if (IsCollidingWater(__instance, character, position.X / 64, position.Y / 64))
				{
					__result = false;
					return false;
				}
				return true;
			}
		}

		public class FarmAnimal_dayUpdate_Patch
		{
			public static void Postfix(FarmAnimal __instance)
			{
				__instance.modData.Remove(swamTodayKey);
			}
		}

		public class FarmAnimal_HandleHop_Patch
		{
			public static void Postfix(FarmAnimal __instance)
			{
				if (__instance.hopOffset == Vector2.Zero)
				{
					Point p = __instance.TilePoint;

					__instance.isSwimming.Value = __instance.currentLocation.doesTileHaveProperty(p.X, p.Y, "Water", "Back") != null;
				}
				if (!__instance.modData.ContainsKey(swamTodayKey))
				{
					SwamToday(__instance);
				}
			}
		}

		public class GameLocation_isOpenWater_Patch
		{
			public static bool Prefix(GameLocation __instance, int xTile, int yTile, ref bool __result)
			{
				if (!Config.ModEnabled)
					return true;

				if (!__instance.isWaterTile(xTile, yTile))
				{
					__result = false;
					return false;
				}

				int tileIndexAt = __instance.getTileIndexAt(xTile, yTile, "Buildings");

				if (tileIndexAt != -1)
				{
					bool flag = true;

					if (waterBuildingTiles.Contains(tileIndexAt))
					{
						flag = false;
					}
					if (flag)
					{
						__result = false;
						return false;
					}
				}
				__result = !__instance.objects.ContainsKey(new Vector2(xTile, yTile));
				return false;
			}
		}
	}
}
