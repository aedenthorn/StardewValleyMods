
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace CropVariation
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Crop), nameof(Crop.draw), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(Color), typeof(float) })]
        public class Crop_draw_Patch
        {
            public static void Prefix(Crop __instance, Vector2 tileLocation, ref Color toTint, ref float ___layerDepth, ref float __state)
            {
                if (!Config.EnableMod || __instance.forageCrop.Value || Config.ColorVariation  == 0 || __instance.currentPhase.Value == 0 || !Game1.currentLocation.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature f))
                    return;
                if(!f.modData.ContainsKey(redVarKey))
                    GetRandomColorVars(f as HoeDirt);
                if (f.modData.TryGetValue(redVarKey, out string redVarString)
                    && float.TryParse(redVarString, NumberStyles.Float, CultureInfo.InvariantCulture, out float redVarFloat)
                    && f.modData.TryGetValue(greenVarKey, out string greenVarString)
                    && float.TryParse(greenVarString, NumberStyles.Float, CultureInfo.InvariantCulture, out float greenVarFloat)
                    && f.modData.TryGetValue(blueVarKey, out string blueVarString)
                    && float.TryParse(blueVarString, NumberStyles.Float, CultureInfo.InvariantCulture, out float blueVarFloat)
                )
                {
                    int redVar = (int)Math.Round(Config.ColorVariation * redVarFloat);
                    int greenVar = (int)Math.Round(Config.ColorVariation * greenVarFloat);
                    int blueVar = (int)Math.Round(Config.ColorVariation * blueVarFloat);
                    toTint = new Color(Math.Clamp(toTint.R + redVar, 0, 255), Math.Clamp(toTint.G + greenVar, 0, 255), Math.Clamp(toTint.B + blueVar, 0, 255), toTint.A);
                }
                __state = ___layerDepth;
                ___layerDepth += tileLocation.X / 1000000f;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Crop), "origin") && codes[i + 1].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 4)
                    {
                        SMonitor.Log("replacing scale with method");
                        codes[i + 1].opcode = OpCodes.Call;
                        codes[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ChangeScale));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(ref float ___layerDepth, ref float __state)
            {
                if (!Config.EnableMod || __state == 0)
                    return;
                ___layerDepth = __state;
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public class Crop_harvest_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.harvest");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i > 1 && codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Crop), nameof(Crop.programColored)) && codes[i - 2].opcode == OpCodes.Stloc_S && ((LocalBuilder)codes[i - 2].operand).LocalIndex == 7)
                    {
                        SMonitor.Log("adding quality adjust method");
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(i, new CodeInstruction(codes[i - 2].opcode, codes[i - 2].operand));
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ChangeQuality))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldloc_S, codes[i - 2].operand));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_3));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.plant))]
        public class HoeDirt_plant_Patch
        {
            public static void Prefix(HoeDirt __instance, bool isFertilizer)
            {
                if (!Config.EnableMod || isFertilizer)
                    return;
                if(Config.SizeVariationPercent  > 0)
                {
                    GetRandomSizeVar(__instance);
                }
                if(Config.ColorVariation  > 0)
                {
                    GetRandomColorVars(__instance);
                }
            }
        }
    }
}