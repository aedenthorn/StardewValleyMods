using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using xTile.Dimensions;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveScarecrows
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.IsScarecrow())
                    return true;
                Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (!location.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt)
                    return true;
                int which = GetMouseCorner();
                ReturnScarecrow(who, location, tf, placementTile, which);
                tf.modData[scarecrowKey + which] = GetScarecrowString(__instance);
                location.playSound("woodyStep", NetAudio.SoundContext.Default);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.DrawOptimized))]
        public class HoeDirt_DrawOptimized_Patch
        {
            public static void Postfix(HoeDirt __instance, SpriteBatch dirt_batch, Vector2 tileLocation)
            {
                if (!Config.EnableMod)
                    return;
                for (int i = 0; i < 4; i++)
                {
                    if(__instance.modData.TryGetValue(scarecrowKey + i, out var scarecrowString))
                    {
                        if(!scarecrowDict.TryGetValue(scarecrowString, out var obj))
                        {
                            obj = GetScarecrow(scarecrowString);
                        }
                        if(obj is not null)
                        {
                            Vector2 scaleFactor = obj.getScale();
                            var globalPosition = tileLocation * 64 + new Vector2(32 - 8 * Config.Scale - scaleFactor.X / 2f, 32 - 8 * Config.Scale - 80 - scaleFactor.Y / 2f) + GetScarecrowCorner(i) * 32;
                            var position = Game1.GlobalToLocal(globalPosition);
                            dirt_batch.Draw(Game1.bigCraftableSpriteSheet, position, Object.getSourceRectForBigCraftable(obj.ParentSheetIndex), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, SpriteEffects.None, (globalPosition.Y + 81 + 16) / 10000f);
                        }
                    }
                }
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOccupiedForPlacement))]
        public class GameLocation_isTileOccupiedForPlacement_Patch
        {
            public static void Postfix(GameLocation __instance, Vector2 tileLocation, Object toPlace, ref bool __result)
            {
                if (!Config.EnableMod || !__result || toPlace is null || !toPlace.IsScarecrow())
                    return;
                if (__instance.terrainFeatures.ContainsKey(tileLocation) && __instance.terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)__instance.terrainFeatures[tileLocation]).crop is not null)
                {
                    __result = false;
                }
            }

        }
        [HarmonyPatch(typeof(GameLocation), "initNetFields")]
        public class GameLocation_initNetFields_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.EnableMod)
                    return;
                __instance.terrainFeatures.OnValueRemoved += delegate (Vector2 tileLocation, TerrainFeature tf)
                {
                    if (tf is not HoeDirt)
                        return;
                    for (int i = 0; i < 4; i++)
                    {
                        if (tf.modData.TryGetValue(scarecrowKey + i, out var scarecrowString))
                        {
                            var obj = GetScarecrow(scarecrowString);
                            if (obj is not null)
                            {
                                __instance.debris.Add(new Debris(obj, tileLocation * 64));
                            }
                        }
                    }
                };
            }
        }
        [HarmonyPatch(typeof(Farm), nameof(Farm.addCrows))]
        public class Farm_addCrows_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Object");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 7 && codes[i].opcode == OpCodes.Call && codes[i].operand  is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(KeyValuePair<Vector2, TerrainFeature>), nameof(KeyValuePair<Vector2, TerrainFeature>.Key)) && codes[i + 1].opcode == OpCodes.Stloc_S && codes[i + 7].opcode == OpCodes.Brfalse)
                    {
                        SMonitor.Log("Adding check for scarecrow at vector");
                        codes.Insert(i + 2, codes[i + 7].Clone());
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckForScarecrowInRange))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldloc_S, codes[i + 1].operand));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

    }
}