using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
                if (!Config.EnableMod || !__instance.IsScarecrow() || (__instance.IsSprinkler() && SHelper.ModRegistry.IsLoaded("aedenthorn.ImmersiveSprinklers")))
                    return true;
                Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (!location.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt)
                    return true;
                int which = GetMouseCorner();
                SMonitor.Log($"Placing {__instance.Name} at {x},{y}:{which}");
                ReturnScarecrow(who, location, placementTile, which);
                tf.modData[scarecrowKey + which] = __instance.QualifiedItemId;
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
                location.playSound("woodyStep");
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
                if (!GetScarecrowTileBool(__instance, ref tile, ref which))
                    return true;
                tf = __instance.terrainFeatures[tile];
                var scareCrow = GetScarecrow(tf, which);
                if(scareCrow is null)
                    return true;
                if(scareCrow.ParentSheetIndex == 126 && who.CurrentItem is not null && who.CurrentItem is Hat)
                {
                    if (tf.modData.TryGetValue(hatKey + which, out var hatString))
                    {
                        Game1.createItemDebris(new Hat(hatString), tf.Tile * 64f, (who.FacingDirection + 2) % 4, null, -1);
                        tf.modData.Remove(hatKey + which);

                    }
                    tf.modData[hatKey + which] = (who.CurrentItem as Hat).ItemId;
                    who.Items[who.CurrentToolIndex] = null;
                    who.currentLocation.playSound("dirtyHit");
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
            public static void Postfix(HoeDirt __instance, SpriteBatch dirt_batch)
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


                            ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                            Rectangle sourceRect = itemData.GetSourceRect(obj is Mannequin ? 2 : 0, new int?(obj.ParentSheetIndex));
                            Texture2D texture = itemData.GetTexture();
                            if (atApi is not null && obj.modData.ContainsKey("AlternativeTextureName"))
                            {
                                texture = GetAltTextureForObject(obj, out sourceRect);
                            }
                            if (texture is null)
                            {
                                texture = Game1.bigCraftableSpriteSheet;
                                sourceRect = Object.getSourceRectForBigCraftable(obj.ParentSheetIndex);
                            }
                            Vector2 globalPosition = __instance.Tile * 64 + GetScarecrowCorner(i) * 32f + new Vector2(0, -16);
                            if (obj.bigCraftable.Value)
                                globalPosition -= new Vector2(0, 64);
                            var layerDepth = (globalPosition.Y + (obj.bigCraftable.Value ? 81 : 33 ) + Config.DrawOffsetZ) / 10000f;
                            var position = Game1.GlobalToLocal(globalPosition);
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
        //[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOccupiedForPlacement))]
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
        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public class Object_canBePlacedHere_Patch
        {
            public static bool Prefix(Object __instance,GameLocation l, Vector2 tile, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.IsScarecrow() || !l.terrainFeatures.TryGetValue(tile, out var tf) || tf is not HoeDirt)
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
                if (!Config.EnableMod || !Context.IsPlayerFree || !__instance.IsScarecrow() || (__instance.IsSprinkler() && SHelper.ModRegistry.IsLoaded("aedenthorn.ImmersiveSprinklers")) || Game1.currentLocation?.terrainFeatures?.TryGetValue(Game1.currentCursorTile, out var tf) != true || tf is not HoeDirt)
                    return true;
                var which = GetMouseCorner();
                var scarecrowTile = Game1.currentCursorTile;

                GetScarecrowTileBool(Game1.currentLocation, ref scarecrowTile, ref which);

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
                Texture2D texture = null;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                Rectangle sourceRect = itemData.GetSourceRect(__instance is Mannequin ? 2 : 0, new int?(__instance.ParentSheetIndex));
                texture = itemData.GetTexture();
                if (atApi is not null && __instance.modData.ContainsKey("AlternativeTextureName"))
                {
                    texture = GetAltTextureForObject(__instance, out sourceRect);
                }
                if (texture is null)
                {
                    texture = Game1.bigCraftableSpriteSheet;
                    sourceRect = Object.getSourceRectForBigCraftable(__instance.ParentSheetIndex);
                }
                spriteBatch.Draw(texture, pos + new Vector2(0, -16), __instance.bigCraftable.Value ? Object.getSourceRectForBigCraftable(__instance.ParentSheetIndex) : GameLocation.getSourceRectForObject(__instance.ParentSheetIndex), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.02f);

                return false;
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
                    if (i < codes.Count - 4 && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Brtrue_S && codes[i + 4].opcode == OpCodes.Callvirt && codes[i + 4].operand  is MethodInfo && (MethodInfo)codes[i + 4].operand == AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.destroyCrop)))
                    {
                        SMonitor.Log("Adding check for scarecrow at vector");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IsScarecrowInRange))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_S, 11));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
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

                Rectangle boundingBox = new Rectangle(x * 64, y * 64, 64, 64);
                using (List<ResourceClump>.Enumerator enumerator2 = location.resourceClumps.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        if (enumerator2.Current.getBoundingBox().Intersects(boundingBox))
                        {
                            return true;
                        }
                    }
                }

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
                int tileX = x / 64;
                int tileY = y / 64;
                Vector2 tile = new Vector2(tileX, tileY);
                for (int i = 0; i < 4; i++)
                {
                    if (location.terrainFeatures.TryGetValue(tile, out var tf) && tf is HoeDirt && TryReturnScarecrow(who, location, tf, i))
                    {
                        location.playSound("woodyHit");
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.GetDirtDecayChance))]
        public class GameLocation_GetDirtDecayChance_Patch
        {
            public static bool Prefix(GameLocation __instance, Vector2 tile, ref double __result)
            {
                if (!Config.EnableMod || !__instance.terrainFeatures.TryGetValue(tile, out var tf) || tf is not HoeDirt dirt)
                    return true;
                for (int i = 0; i < 4; i++)
                {
                    if (dirt.modData.TryGetValue(scarecrowKey + i, out var scarecrowString))
                    {
                        SMonitor.Log($"Preventing hoedirt removal");
                        __result = -1;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.HandleGrassGrowth))]
        public class GameLocation_HandleGrassGrowth_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GameLocation.HandleGrassGrowth");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldftn && codes[i].operand is MethodInfo info && Array.Exists(info.GetParameters(), p => p.ParameterType == typeof(KeyValuePair<Vector2, TerrainFeature>)))
                    {
                        SMonitor.Log($"overriding hoedirt removal");
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(RemoveWhere));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Utility), nameof(Utility.doesItemExistAnywhere))]
        public class Utility_doesItemExistAnywhere_Patch
        {
            public static void Postfix(string itemId, ref bool __result)
            {
                if (!Config.EnableMod || __result || (!new string[] { "(BC)136", "(BC)137", "(BC)138", "(BC)139", "(BC)140", "(BC)126", "(BC)110", "(BC)113" }.Contains(itemId)) || Game1.getFarm() is null)
                    return;
                foreach(var kvp in Game1.getFarm().terrainFeatures.Pairs)
                {
                    if (kvp.Value is not HoeDirt)
                        continue;
                    for (int i = 0; i < 4; i++)
                    {
                        var id = GetScarecrow(kvp.Value, i)?.QualifiedItemId;
                        if (id is null)
                            continue;
                        if (id == itemId)
                        {
                            __result = true;
                            return;
                        }
                    }
                }
            }
        }

    }
}