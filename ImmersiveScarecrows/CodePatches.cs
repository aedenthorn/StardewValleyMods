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
using StardewValley.Objects;

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
                SMonitor.Log($"Placing {__instance.Name} at {x},{y}:{which}");
                ReturnScarecrow(who, location, tf, placementTile, which);
                tf.modData[scarecrowKey + which] = GetScarecrowString(__instance);
                tf.modData[guidKey + which] = Guid.NewGuid().ToString();
                if (atApi is not null)
                {
                    Object obj = (Object)__instance.getOne();
                    SetAltTextureForObject(obj);
                    foreach (var kvp in obj.modData.Pairs)
                    {
                        if (kvp.Key.StartsWith(altTextureKey))
                        {
                            tf.modData[prefixKey + kvp.Key + which] = kvp.Value;
                        }
                    }
                }
                location.playSound("woodyStep", NetAudio.SoundContext.Default);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                var tile = new Vector2(tileLocation.X, tileLocation.Y);
                if (!Config.EnableMod || !Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var tf) && tf is HoeDirt)
                    return true;
                int which = GetMouseCorner();
                if (!GetScarecrowTileBool(__instance, ref tile, ref which, out string scarecrowString))
                    return true;
                var scareCrow = GetScarecrow(tf, which);
                if(scareCrow is null || scareCrow.ParentSheetIndex != 126)
                    return true;

                if(tf.modData.TryGetValue(hatKey + which, out var hatString))
                {
                    Game1.createItemDebris(new Hat(int.Parse(hatString)), tf.currentTileLocation * 64f, (who.FacingDirection + 2) % 4, null, -1);
                    tf.modData.Remove(hatKey + which);
                }
                if (who.CurrentItem is not null && who.CurrentItem is Hat)
                {
                    tf.modData[hatKey + which] = (who.CurrentItem as Hat).which.Value + "";
                    who.Items[who.CurrentToolIndex] = null;
                    who.currentLocation.playSound("dirtyHit", NetAudio.SoundContext.Default);
                }
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
                    if(__instance.modData.ContainsKey(scarecrowKey + i))
                    {
                        if (!__instance.modData.TryGetValue(guidKey + i, out var guid))
                        {
                            guid = Guid.NewGuid().ToString();
                            __instance.modData[guidKey + i] = guid;
                        }
                        if (!scarecrowDict.TryGetValue(guid, out var obj))
                        {
                            obj = GetScarecrow(__instance, i);
                        }
                        if (obj is not null)
                        {
                            Vector2 scaleFactor = obj.getScale();
                            var globalPosition = tileLocation * 64 + new Vector2(32 - 8 * Config.Scale - scaleFactor.X / 2f + Config.DrawOffsetX, 32 - 8 * Config.Scale - 80 - scaleFactor.Y / 2f + Config.DrawOffsetY) + GetScarecrowCorner(i) * 32;
                            var position = Game1.GlobalToLocal(globalPosition);
                            Texture2D texture = null;
                            Rectangle sourceRect = new Rectangle();
                            if (atApi is not null && obj.modData.ContainsKey("AlternativeTextureName"))
                            {
                                texture = GetAltTextureForObject(obj, out sourceRect);
                            }
                            if (texture is null)
                            {
                                texture = Game1.bigCraftableSpriteSheet;
                                sourceRect = Object.getSourceRectForBigCraftable(obj.ParentSheetIndex);
                            }
                            var layerDepth = (globalPosition.Y + 81 + 16 + Config.DrawOffsetZ) / 10000f;
                            dirt_batch.Draw(texture, position, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, SpriteEffects.None, layerDepth);
                            if (__instance.modData.TryGetValue(hatKey + i, out string hatString) && int.TryParse(hatString, out var hat))
                            {
                                dirt_batch.Draw(FarmerRenderer.hatsTexture, position + new Vector2(-3f, -6f) * 4f, new Rectangle(hat * 20 % FarmerRenderer.hatsTexture.Width, hat * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), Color.White * Config.Alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 1E-05f);
                            }
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
                            try
                            {
                                __instance.terrainFeatures.Add(tileLocation, tf);
                            }
                            catch { }
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
                SMonitor.Log($"Transpiling Farm.addCrows");

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