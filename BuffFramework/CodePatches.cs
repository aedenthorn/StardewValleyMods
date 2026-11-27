using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewValley;
using StardewValley.Objects;

namespace BuffFramework
{
	public partial class ModEntry
	{
		public class Farmer_ActiveItemSetter_Patch
		{
			public static void Postfix()
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}
		}

		public class Farmer_shiftToolbar_Patch
		{
			public static void Postfix()
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}
		}

		public class Tool_attach_Patch
		{
			public static void Postfix()
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}
		}

		public class Farmer_farmerInit_Patch
		{
			public static void Postfix(Farmer __instance)
			{
				__instance.hat.fieldChangeEvent += Hat_fieldChangeEvent;
				__instance.shirtItem.fieldChangeEvent += ShirtItem_fieldChangeEvent;
				__instance.pantsItem.fieldChangeEvent += PantsItem_fieldChangeEvent;
				__instance.boots.fieldChangeEvent += Boots_fieldChangeEvent;
				__instance.leftRing.fieldChangeEvent += LeftRing_fieldChangeEvent;
				__instance.rightRing.fieldChangeEvent += RightRing_fieldChangeEvent;
			}

			public static void Hat_fieldChangeEvent(Netcode.NetRef<Hat> field, Hat oldValue, Hat newValue)
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}

			public static void ShirtItem_fieldChangeEvent(Netcode.NetRef<Clothing> field, Clothing oldValue, Clothing newValue)
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}

			public static void PantsItem_fieldChangeEvent(Netcode.NetRef<Clothing> field, Clothing oldValue, Clothing newValue)
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}

			public static void Boots_fieldChangeEvent(Netcode.NetRef<Boots> field, Boots oldValue, Boots newValue)
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}

			public static void LeftRing_fieldChangeEvent(Netcode.NetRef<Ring> field,Ring oldValue, Ring newValue)
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}

			public static void RightRing_fieldChangeEvent(Netcode.NetRef<Ring> field, Ring oldValue, Ring newValue)
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}
		}

		public class Farmer_doneEating_Patch
		{
			public static void Prefix(Farmer __instance)
			{
				ApplyBuffsOnEat(__instance);
			}
		}

		public class Buff_OnAdded_Patch
		{
			public static void Postfix(Buff __instance)
			{
				if (!Config.ModEnabled)
					return;

				if (soundBuffs.ContainsKey(__instance.id))
				{
					ICue cue = Game1.soundBank.GetCue(soundBuffs[__instance.id].Item1);

					cue.Play();
					soundBuffs[__instance.id] = (soundBuffs[__instance.id].Item1, cue);
				}
			}
		}

		public class Buff_OnRemoved_Patch
		{
			public static void Postfix(Buff __instance)
			{
				if (!Config.ModEnabled)
					return;

				if (soundBuffs.ContainsKey(__instance.id))
				{
					ICue cue = soundBuffs[__instance.id].Item2;

					if (cue is not null && cue.IsPlaying)
					{
						cue.Stop(AudioStopOptions.Immediate);
					}
				}
				HealthRegenerationBuffs.Remove(__instance.id);
				StaminaRegenerationBuffs.Remove(__instance.id);
				GlowRateBuffs.Remove(__instance.id);
				soundBuffs.Remove(__instance.id);
			}
		}

		public class BuffManager_GetValues_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode.Equals(OpCodes.Ldc_R4) && list[i].operand is not null && list[i].operand.Equals(0.05f))
					{
						CodeInstruction[] replacementInstructions = new CodeInstruction[]
						{
							new(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(GetGlowRate), BindingFlags.Public | BindingFlags.Static))
						};

						list.InsertRange(i, replacementInstructions);
						i += replacementInstructions.Length;
						list.RemoveAt(i);
						break;
					}
				}
				return list;
			}
		}

		public class GameLocation_startEvent_Patch
		{
			public static void Postfix(Event evt)
			{
				if (!Config.ModEnabled)
					return;

				evt.onEventFinished += HandleEventAndFestivalFinished;
				HandleEventAndFestivalStart();
			}
		}
	}
}
