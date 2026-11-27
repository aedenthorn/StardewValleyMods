using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace AdvancedFluteBlocks
{
	public partial class ModEntry
	{
		public static IEnumerable<CodeInstruction> Object_FluteBlock_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
		{
			try
			{
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode.Equals(OpCodes.Ldstr) && list[i].operand.Equals("flute"))
					{
						CodeInstruction[] replacementInstructions = new CodeInstruction[]
						{
							new(OpCodes.Ldarg_0),
							new(list[i].opcode, list[i].operand),
							new(OpCodes.Call, typeof(ModEntry).GetMethod("FluteCueNameOverride", BindingFlags.NonPublic | BindingFlags.Static))
						};
						list.InsertRange(i, replacementInstructions);
						i += replacementInstructions.Length;
						list.RemoveAt(i);
					}
				}
				return list;
			}
			catch (Exception e)
			{
				SMonitor.Log($"There was an issue modifying the instructions for {typeof(Object)}.{original.Name}: {e}", LogLevel.Error);
				return instructions;
			}
		}

		public static bool Game1_pressSwitchToolButton_Prefix()
		{
			return !Config.EnableMod || !SHelper.Input.IsDown(Config.PitchModKey) && !SHelper.Input.IsDown(Config.ToneModKey) || !Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) || !obj.Name.Equals("Flute Block");
		}

		private static string FluteCueNameOverride(Object __instance, string flute)
		{
			if (Config.EnableMod && __instance.modData.TryGetValue(advancedFluteBlocksKey, out string tone))
			{
				return tone;
			}
			return flute;
		}
	}
}
