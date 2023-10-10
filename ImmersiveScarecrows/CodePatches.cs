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
using StardewModdingAPI;
using StardewValley.Tools;

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
                ReturnScarecrow(who, location, placementTile, which);
                tf.modData[scarecrowKey + which] = GetScarecrowString(__instance);
                tf.modData[guidKey + which] = Guid.NewGuid().ToString();
                tf.modData[scaredKey + which] = "0";
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
                if (!Config.EnableMod || !Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var tf) || tf is not HoeDirt)
                    return true;
                int which = GetMouseCorner();
                if (!GetScarecrowTileBool(__instance, ref tile, ref which, out string scarecrowString))
                    return true;
                tf = __instance.terrainFeatures[tile];
                var scareCrow = GetScarecrow(tf, which);
                if(scareCrow is null)
                    return true;
                if(scareCrow.ParentSheetIndex == 126 && who.CurrentItem is not null && who.CurrentItem is Hat)
                {
                    if (tf.modData.TryGetValue(hatKey + which, out var hatString))
                    {
                        Game1.createItemDebris(new Hat(int.Parse(hatString)), tf.currentTileLocation * 64f, (who.FacingDirection + 2) % 4, null, -1);
                        tf.modData.Remove(hatKey + which);

                    }
                    tf.modData[hatKey + which] = (who.CurrentItem as Hat).which.Value + "";
                    who.Items[who.CurrentToolIndex] = null;
                    who.currentLocation.playSound("dirtyHit", NetAudio.SoundContext.Default);
                    __result = true;
                    return false;
                }
                if (Game1.didPlayerJustRightClick(true))
                {
                    if (!tf.modData.TryGetValue(scaredKey + which, out var scaredString) || !int.TryParse(scaredString, out int scared))
                    {
                        tf.modData[scaredKey + which] = "0";
                        scared = 0;
                    }
                    if (scared == 0)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
                    }
                    else
                    {
                        Game1.drawObjectDialogue((scared == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", scared));
                    }
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
        [HarmonyPatch(typeof(Utility), "itemCanBePlaced")]
        public class Utility_itemCanBePlaced_Patch
        {
            public static bool Prefix(GameLocation location, Vector2 tileLocation, Item item, ref bool __result)
            {
                if (!Config.EnableMod || item is not Object || !(item as Object).IsScarecrow() || !location.terrainFeatures.TryGetValue(tileLocation, out var tf) || tf is not HoeDirt)
                    return true;
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.playerCanPlaceItemHere))]
        public class Utility_playerCanPlaceItemHere_Patch
        {
            public static bool Prefix(GameLocation location, Item item, int x, int y, Farmer f, ref bool __result)
            {
                if (!Config.EnableMod || item is not Object || !(item as Object).IsScarecrow() || !location.terrainFeatures.TryGetValue(new Vector2(x / 64, y / 64), out var tf) || tf is not HoeDirt)
                    return true;
                __result = Utility.withinRadiusOfPlayer(x, y, 1, Game1.player);
                return false;
            }

        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawPlacementBounds))]
        public class Object_drawPlacementBounds_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, GameLocation location)
            {
                if (!Config.EnableMod || !Context.IsPlayerFree || !__instance.IsScarecrow() || Game1.currentLocation?.terrainFeatures?.TryGetValue(Game1.currentCursorTile, out var tf) != true || tf is not HoeDirt)
                    return true;
                var which = GetMouseCorner();
                var scarecrowTile = Game1.currentCursorTile;

                GetScarecrowTileBool(Game1.currentLocation, ref scarecrowTile, ref which, out string str);

                Vector2 pos = Game1.GlobalToLocal(scarecrowTile * 64 + GetScarecrowCorner(which) * 32f);

                spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(Utility.withinRadiusOfPlayer((int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64, 1, Game1.player) ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);

                if (Config.ShowRangeWhenPlacing)
                {
                    foreach (var tile in GetScarecrowTiles(scarecrowTile, which, __instance.GetRadiusForScarecrow()))
                    {
                        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(tile * 64), new Rectangle(194, 388, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    }
                }
                if (__instance.bigCraftable.Value)
                    pos -= new Vector2(0, 64);
                spriteBatch.Draw(__instance.bigCraftable.Value ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, pos + new Vector2(0, -16), __instance.bigCraftable.Value ? Object.getSourceRectForBigCraftable(__instance.ParentSheetIndex) : GameLocation.getSourceRectForObject(__instance.ParentSheetIndex), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.02f);

                return false;
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
                bool found1 = false;
                bool found2 = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 7 && codes[i].opcode == OpCodes.Call && codes[i].operand  is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(KeyValuePair<Vector2, TerrainFeature>), nameof(KeyValuePair<Vector2, TerrainFeature>.Key)) && codes[i + 1].opcode == OpCodes.Stloc_S && codes[i + 7].opcode == OpCodes.Brfalse)
                    {
                        SMonitor.Log("Adding check for scarecrow at vector");
                        codes.Insert(i + 2, codes[i + 7].Clone());
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IsNoScarecrowInRange))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldloc_S, codes[i + 1].operand));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 11;
                        found1 = true;
                    }
                    if (!found2 && i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldloca_S  && codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.PropertyGetter(typeof(KeyValuePair<Vector2, Object>), nameof(KeyValuePair<Vector2, Object>.Value)) && codes[i + 2].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 2].operand == AccessTools.Field(typeof(Object), nameof(Object.bigCraftable)) && codes[i + 4].opcode == OpCodes.Brfalse_S)
                    {
                        SMonitor.Log("Removing big craftable check");
                        codes[i].opcode = OpCodes.Nop;
                        codes[i + 1].opcode = OpCodes.Nop;
                        codes[i + 2].opcode = OpCodes.Nop;
                        codes[i + 3].opcode = OpCodes.Nop;
                        codes[i + 4].opcode = OpCodes.Nop;
                        codes[i].operand = null;
                        codes[i + 1].operand = null;
                        codes[i + 2].operand = null;
                        codes[i + 3].operand = null;
                        codes[i + 4].operand = null;
                        i += 4;
                        found2 = true;
                    }
                    if (found1 && found2)
                        break;
                }

                return codes.AsEnumerable();
            }
        }
        public static bool Modded_Farm_AddCrows_Prefix(ref bool __result)
        {
            SMonitor.Log("Disabling addCrows prefix for Prismatic Tools and Radioactive tools");
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(Axe), nameof(Axe.DoFunction))]
        public class Axe_DoFunction_Patch
        {
            public static bool Prefix(GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.EnableMod || power > 1)
                    return true;
                Vector2 placementTile = new Vector2(x, y);
                int which = GetMouseCorner();
                if (ReturnScarecrow(Game1.player, location, Game1.currentCursorTile, which))
                {
                    location.playSound("axechop");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Pickaxe), nameof(Pickaxe.DoFunction))]
        public class Pickaxe_DoFunction_Patch
        {
            public static bool Prefix(GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.EnableMod)
                    return true;
                Vector2 placementTile = new Vector2(x, y);
                int which = GetMouseCorner();
                if (ReturnScarecrow(Game1.player, location, Game1.currentCursorTile, which))
                {
                    location.playSound("axechop");
                    return false;
                }
                return true;
            }
        }
    }
}