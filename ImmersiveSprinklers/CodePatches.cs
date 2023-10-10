using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using xTile.Dimensions;
using xTile.Tiles;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklers
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod || who.CurrentItem is null || (who.CurrentItem.ParentSheetIndex != 913 && who.CurrentItem.ParentSheetIndex != 915 && who.CurrentItem.Category != -19))
                    return true;
                Vector2 placementTile = new Vector2((float)tileLocation.X, (float)tileLocation.Y);
                if (!__instance.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt)
                    return true;
                int which = GetMouseCorner();
                if(GetSprinklerTileBool(__instance, ref placementTile, ref which, out string sprinklerString))
                {
                    tf = __instance.terrainFeatures[placementTile];
                    if (who.CurrentItem.ParentSheetIndex == 913 && !tf.modData.ContainsKey(enricherKey + which))
                    {
                        tf.modData[enricherKey + which] = "true";
                        who.reduceActiveItemByOne();
                        __instance.playSound("axe", NetAudio.SoundContext.Default);
                    }
                    else if (who.CurrentItem.ParentSheetIndex == 915 && !tf.modData.ContainsKey(nozzleKey + which))
                    {
                        tf.modData[nozzleKey + which] = "true";
                        who.reduceActiveItemByOne();
                        __instance.playSound("axe", NetAudio.SoundContext.Default);
                    }
                    else if (who.CurrentItem.Category == -19 && tf.modData.ContainsKey(enricherKey + which))
                    {
                        int stack = who.CurrentItem.Stack;
                        int addStack = stack;
                        int index = who.CurrentItem.ParentSheetIndex;
                        if (tf.modData.TryGetValue(fertilizerKey + which, out string fertString))
                        {
                            Object f = GetFertilizer(fertString);
                            if(f.ParentSheetIndex == who.CurrentItem.ParentSheetIndex)
                            {
                                int add = Math.Min(f.maximumStackSize() - f.Stack, stack);
                                addStack = f.Stack + add;
                                stack -= add;
                                who.CurrentItem.Stack = stack;
                                if (stack == 0)
                                {
                                    who.removeItemFromInventory(who.CurrentItem);
                                    who.showNotCarrying();
                                }
                            }
                            else
                            {
                                var slot = who.CurrentToolIndex;
                                who.removeItemFromInventory(who.CurrentItem);
                                who.showNotCarrying();
                                who.Items[slot] = f;
                            }
                        }
                        else
                        {
                            who.removeItemFromInventory(who.CurrentItem);
                            who.showNotCarrying();
                        }
                        tf.modData[fertilizerKey + which] = index + "," + addStack;
                        __instance.playSound("dirtyHit", NetAudio.SoundContext.Default);
                    }
                    else
                        return true;
                }
                else
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (!location.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt)
                    return true;
                int which = GetMouseCorner();
                if (__instance.IsSprinkler())
                {
                    SMonitor.Log($"Placing {__instance.Name} at {x},{y}:{which}");
                    location.playSound("woodyStep");
                    ReturnSprinkler(who, location, placementTile, which);
                    tf.modData[sprinklerKey + which] = GetSprinklerString(__instance);
                    if (__instance.bigCraftable.Value)
                    {
                        tf.modData[bigCraftableKey + which] = "true";
                    }
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
                    __result = true;
                    return false;
                }
                else if (__instance.Category == -74)
                {
                    foreach (var kvp in location.terrainFeatures.Pairs)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (kvp.Value.modData.TryGetValue(sprinklerKey + i, out string sprinklerString) && kvp.Value.modData.ContainsKey(enricherKey + i) && kvp.Value.modData.TryGetValue(fertilizerKey + i, out string fertString))
                            {
                                var obj = GetSprinkler(kvp.Value, i, kvp.Value.modData.ContainsKey(enricherKey + i));
                                if (obj is null)
                                    continue;
                                var radius = obj.GetModifiedRadiusForSprinkler();

                                if (GetSprinklerTiles(kvp.Key, i, radius).Contains(placementTile))
                                {
                                    Object f = GetFertilizer(fertString);
                                    if (((HoeDirt)tf).plant(f.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who, true, location))
                                    {
                                        f.Stack--;
                                        if(f.Stack > 0)
                                        {
                                            kvp.Value.modData[fertilizerKey + i] = f.ParentSheetIndex + "," + f.Stack;
                                        }
                                        else
                                        {
                                            kvp.Value.modData.Remove(fertilizerKey + i);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
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
                    if(__instance.modData.ContainsKey(sprinklerKey + i))
                    {
                        var obj = GetSprinklerCached(__instance, i, __instance.modData.ContainsKey(nozzleKey + i));

                        if (obj is not null)
                        {
                            var globalPosition = tileLocation * 64 + new Vector2(32 - 8 * Config.Scale + Config.DrawOffsetX, (obj.bigCraftable.Value ? -32 : 32) - 8 * Config.Scale + Config.DrawOffsetY) + GetSprinklerCorner(i) * 32;
                            var position = Game1.GlobalToLocal(globalPosition);
                            var layerDepth = (globalPosition.Y + (obj.bigCraftable.Value ? 80 : 16) + Config.DrawOffsetZ) / 10000f;
                            Texture2D texture = null;
                            Rectangle sourceRect = new Rectangle();
                            if (atApi is not null && obj.modData.ContainsKey("AlternativeTextureName"))
                            {
                                texture = GetAltTextureForObject(obj, out sourceRect);
                            }
                            
                            if(texture is null)
                            {
                                texture = obj.bigCraftable.Value ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet;
                                sourceRect = obj.bigCraftable.Value ? Object.getSourceRectForBigCraftable(obj.ParentSheetIndex) : GameLocation.getSourceRectForObject(obj.ParentSheetIndex);
                            }
                            dirt_batch.Draw(texture, position, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

                            if (__instance.modData.ContainsKey(enricherKey + i))
                            {
                                dirt_batch.Draw(Game1.objectSpriteSheet, position + new Vector2(0f, -20f), GameLocation.getSourceRectForObject(914), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 2E-05f);
                            }
                            if (__instance.modData.ContainsKey(nozzleKey + i))
                            {
                                dirt_batch.Draw(Game1.objectSpriteSheet, position, GameLocation.getSourceRectForObject(916), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 1E-05f);
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
                if (!Config.EnableMod || !__result || toPlace is null || !toPlace.IsSprinkler())
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
                if (!Config.EnableMod || item is not Object || !(item as Object).IsSprinkler() || !location.terrainFeatures.TryGetValue(tileLocation, out var tf) || tf is not HoeDirt)
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
                if (!Config.EnableMod || item is not Object || !(item as Object).IsSprinkler() || !location.terrainFeatures.TryGetValue(new Vector2(x / 64, y / 64), out var tf) || tf is not HoeDirt)
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
                if (!Config.EnableMod || !Context.IsPlayerFree || !__instance.IsSprinkler() || Game1.currentLocation?.terrainFeatures?.TryGetValue(Game1.currentCursorTile, out var tf) != true || tf is not HoeDirt)
                    return true;
                var which = GetMouseCorner();
                var sprinklerTile = Game1.currentCursorTile;

                GetSprinklerTileBool(Game1.currentLocation, ref sprinklerTile, ref which, out string str);

                Vector2 pos = Game1.GlobalToLocal(sprinklerTile * 64 + GetSprinklerCorner(which) * 32f);

                spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(Utility.withinRadiusOfPlayer((int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64, 1, Game1.player) ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);

                if (Config.ShowRangeWhenPlacing)
                {
                    foreach(var tile in GetSprinklerTiles(sprinklerTile, which, GetSprinklerRadius(__instance)))
                    {
                        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(tile * 64), new Rectangle(194, 388, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    }
                }
                if(__instance.bigCraftable.Value)
                    pos -= new Vector2(0, 64);
                spriteBatch.Draw(__instance.bigCraftable.Value ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, pos, __instance.bigCraftable.Value ? Object.getSourceRectForBigCraftable(__instance.ParentSheetIndex) : GameLocation.getSourceRectForObject(__instance.ParentSheetIndex), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.02f);

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
                        if (tf.modData.TryGetValue(sprinklerKey + i, out var sprinklerString))
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
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.dayUpdate))]
        public class HoeDirt_dayUpdate_Patch
        {
            public static void Postfix(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
            {
                if (!Config.EnableMod || (environment.IsOutdoors && Game1.IsRainingHere(environment)))
                    return;
                for (int i = 0; i < 4; i++)
                {
                    if (__instance.modData.TryGetValue(sprinklerKey + i, out var sprinklerString))
                    {
                        var obj = GetSprinkler(__instance, i, __instance.modData.ContainsKey(nozzleKey + i));
                        if (obj is not null)
                        {
                            var which = i;
                            environment.postFarmEventOvernightActions.Add(delegate
                            {
                                ActivateSprinkler(environment, tileLocation, obj, which, true);
                            });
                        }
                    }
                }
            }

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
                if (ReturnSprinkler(Game1.player, location, Game1.currentCursorTile, which))
                {
                    location.playSound("hammer");
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
                if (ReturnSprinkler(Game1.player, location, Game1.currentCursorTile, which))
                {
                    location.playSound("hammer");
                    return false;
                }
                return true;
            }
        }
    }
}