using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;

namespace CropsSurviveSeasonChange
{
	public partial class ModEntry
	{
		public class Crop_newDay_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling Crop.newDay");
				var codes = new List<CodeInstruction>(instructions);
				for (int i = 0; i < codes.Count; i++)
				{
					if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo info && info == AccessTools.Field(typeof(GameLocation), "isOutdoors"))
					{
						SMonitor.Log($"adding method to prevent killing");
						codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ShouldKillCrop))));
						codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldloc_0));
						codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
						break;
					}
				}
				return codes.AsEnumerable();
			}
		}

		public class HoeDirt_dayUpdate_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling Crop.dayUpdate");
				var codes = new List<CodeInstruction>(instructions);
				for (int i = 0; i < codes.Count; i++)
				{
					if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo info && info == AccessTools.Field(typeof(GameLocation), "isOutdoors"))
					{
						SMonitor.Log($"adding method to prevent killing");
						codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ShouldDestroyCrop))));
						codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldloc_0));
						codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
						break;
					}
				}
				return codes.AsEnumerable();
			}
		}
	}
}
