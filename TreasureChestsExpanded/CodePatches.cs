using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace TreasureChestsExpanded
{
	public partial class ModEntry
	{
		public class Chest_draw_Patch
		{
			public static bool Prefix(Chest __instance)
			{
				if (!__instance.modData.ContainsKey(modKey))
					return true;
				if (!__instance.Location.objects.ContainsKey(__instance.TileLocation) || (__instance.Items.Count > 0 && __instance.Items[0] != null))
					return true;

				SMonitor.Log($"removing chest at {__instance.TileLocation}");
				__instance.Location.objects.Remove(__instance.TileLocation);
				return false;
			}
		}

		public class MineShaft_addLevelChests_Patch
		{
			public static void Postfix(MineShaft __instance)
			{
				if (!Config.EnableMod || __instance.mineLevel < 121)
					return;

				Vector2 chestSpot = new(9f, 9f);
				NetBool treasureRoom = SHelper.Reflection.GetField<NetBool>(__instance, "netIsTreasureRoom").GetValue();

				if (treasureRoom.Value && __instance.overlayObjects.ContainsKey(chestSpot))
				{
					int level = Math.Min((int)Math.Floor((double)(__instance.mineLevel - 120) / 30), tintColors.Length - 1);
					Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, __instance.mineLevel, Config.IncreaseRate, Config.ItemsBaseMaxValue, chestSpot);

					chest.playerChoiceColor.Value = tintColors[level];
					chest.modData.Add(modKey, "T");
					chest.modData.Add(modCoinKey, advancedLootFrameworkApi.GetChestCoins(level + 1, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax).ToString());
					__instance.overlayObjects[chestSpot] = chest;
				}
			}
		}

		public class MineShaft_loadLevel_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (CodeInstruction instruction in instructions)
				{
					if (instruction.opcode.Equals(OpCodes.Ldc_R8) && instruction.operand.Equals(0.01))
					{
						instruction.opcode = OpCodes.Call;
						instruction.operand = typeof(MineShaft_loadLevel_Patch).GetMethod(nameof(GetChanceForTreasureRoom), BindingFlags.NonPublic | BindingFlags.Static);
						break;
					}
				}
				return instructions;
			}

			private static double GetChanceForTreasureRoom()
			{
				if (!Config.EnableMod)
					return 0.01;

				return (double)Config.ChanceForTreasureRoom / 100;
			}
		}

		public class Chest_showMenu_Patch
		{
			public static void Postfix(Chest __instance)
			{
				if (!__instance.modData.ContainsKey(modKey))
					return;
				if (!__instance.modData.ContainsKey(modCoinKey))
					return;
				Game1.player.Money += int.Parse(__instance.modData[modCoinKey]);
				__instance.modData.Remove(modCoinKey);
				return;
			}
		}
	}
}
