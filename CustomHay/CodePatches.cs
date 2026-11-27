using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;

namespace CustomHay
{
	public partial class ModEntry
	{
		public class Grass_TryDropItemsOnCut_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling Grass.performToolAction");
				var codes = new List<CodeInstruction>(instructions);
				var found1 = false;
				var found2 = false;

				for (int i = 0; i < codes.Count; i++)
				{
					if (!found1 && i < codes.Count - 4 && codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.5 && codes[i + 2].opcode == OpCodes.Ldc_R8 && (double)codes[i + 2].operand == 0.75 && codes[i + 4].opcode == OpCodes.Ldc_R8 && (double)codes[i + 4].operand == 1.0)
					{
						SMonitor.Log("modifying hay chances");
						codes[i].opcode = OpCodes.Call;
						codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(GetOrdinaryHayChance));
						codes[i + 2].opcode = OpCodes.Call;
						codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(GetGoldHayChance));
						codes[i + 4].opcode = OpCodes.Call;
						codes[i + 4].operand = AccessTools.Method(typeof(ModEntry), nameof(GetIridiumHayChance));
						found1 = true;
					}
					if (found1 && !found2 && codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo info && info == AccessTools.Method(typeof(Random), nameof(Random.NextDouble)))
					{
						SMonitor.Log("replacing random value");
						codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetRandomValue))));
						found2 = true;
					}
					if (found1 && found2)
						break;
				}
				return codes.AsEnumerable();
			}
		}

		private static double GetOrdinaryHayChance()
		{
			return Config.ModEnabled ? (double)Config.OrdinaryHayChance : 0.5;
		}

		private static double GetGoldHayChance()
		{
			return Config.ModEnabled ? (double)Config.GoldHayChance : 0.75;
		}

		private static double GetIridiumHayChance()
		{
			return Config.ModEnabled ? (double)Config.IridiumHayChance : 1.0;
		}

		private static double GetRandomValue(double value)
		{
			return Config.ModEnabled ? Game1.random.NextDouble() : value;
		}
	}
}
