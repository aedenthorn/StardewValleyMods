using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using xTile.Dimensions;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Object = StardewValley.Object;
using StardewValley.Characters;
using static StardewValley.Minigames.CraneGame;
using StardewValley.Locations;

namespace BuffFramework
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static bool Prefix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !__instance.IsLocalPlayer)
                    return true;

                foreach(var fb in farmerBuffs.Value)
                {
                    Buff? buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == fb.which);
                    if (buff == null)
                    {
                        Game1.buffsDisplay.addOtherBuff(
                            buff = fb
                        );
                    }
                    buff.millisecondsDuration = 50;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Buff), nameof(Buff.addBuff))]
        public class Buff_addBuff_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Buff.addBuff");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 0.05f && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Character), nameof(Character.startGlowing)))
                    {
                        SMonitor.Log($"adding check for custom glow rate");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckGlowRate))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(Buff), nameof(Buff.removeBuff))]
        public class Buff_removeBuff_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Buff.removeBuff");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 0.05f && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Character), nameof(Character.startGlowing)))
                    {
                        SMonitor.Log($"adding check for custom glow rate");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckGlowRate))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}