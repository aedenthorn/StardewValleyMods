using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Menus;

namespace InfallibleFishing
{
	public partial class ModEntry
	{
		public class BobberBar_update_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> list = instructions.ToList();

				SMonitor.Log($"Transpiling BobberBar.update");
				for(int i = 0; i < list.Count; i++)
				{
					if (i < list.Count - 2 && list[i].opcode == OpCodes.Ldfld && (FieldInfo)list[i].operand == AccessTools.Field(typeof(BobberBar), "distanceFromCatching") && list[i + 1].opcode == OpCodes.Ldc_R4 && (float)list[i + 1].operand == 0 && list[i + 2].opcode == OpCodes.Bgt_Un_S)
					{
						SMonitor.Log($"Lowering min distance from catching to -1");
						list[i + 1].opcode = OpCodes.Call;
						list[i + 1].operand = typeof(BobberBar_update_Patch).GetMethod(nameof(GetMinDistanceFromCatching));
					}
				}
				return list;
			}

			public static float GetMinDistanceFromCatching()
			{
				return Config.ModEnabled ? -1f : 0f;
			}
		}
	}
}
