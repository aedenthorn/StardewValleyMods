using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StardewValley.Monsters;
using xTile.Dimensions;
using StardewValley.Objects;

namespace GhostSpeed
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Monster), nameof(Monster.MovePosition))]
        public class Character_MovePosition_Patch
        {
            public static void Prefix(Monster __instance)
            {
                if (__instance is not Ghost)
                    return;
                var speed = __instance.Speed;
                var add = __instance.addedSpeed;
                var xv = __instance.xVelocity;
                var yv = __instance.yVelocity;
                var s = __instance.Slipperiness;
                var g = __instance.isGlider;
                var xy = 1;
            }
        }
        //[HarmonyPatch(typeof(Ghost), "updateAnimation")]
        public class Ghost_updateAnimation_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Ghost.updateAnimation");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 2 && 
                        codes[i].opcode == OpCodes.Call && 
                        codes[i].operand is MethodInfo && 
                        (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Math), nameof(Math.Max), new Type[] { typeof(float), typeof(float) }) && 
                        codes[i + 1].opcode == OpCodes.Call && 
                        codes[i + 1].operand is MethodInfo && 
                        (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Math), nameof(Math.Min), new Type[] { typeof(float), typeof(float) }) &&
                        codes[i + 2].opcode == OpCodes.Stloc_S
                        )
                    {
                        SMonitor.Log("Inserting maxAccel multiplier");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSpeedMult))));
                        i += 2;
                    }
                }
                return codes.AsEnumerable();
            }
        }
        
        [HarmonyPatch(typeof(Ghost), nameof(Ghost.behaviorAtGameTick))]
        public class Ghost_behaviorAtGameTick_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Ghost.behaviorAtGameTick");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 2 && codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == -12 && codes[i+1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i + 1].operand == 12 && codes[i + 2].opcode == OpCodes.Callvirt)
                    {
                        SMonitor.Log("Replacing knockback");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetKnockback))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetKnockback))));
                        i += 4;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        private static float GetSpeedMult(float mult)
        {
            if(Config.ModEnabled)
            {
                mult *= Config.SpeedMultiplier;
            }
            return mult;
        }
        private static int GetKnockback(int tiles)
        {
            if(Config.ModEnabled)
            {
                tiles = Math.Sign(tiles) * Config.TilesKnockedBack;
            }
            return tiles;
        }
    }
}