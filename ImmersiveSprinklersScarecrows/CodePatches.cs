using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Machines;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklersScarecrows
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.isPassable))]
        public class Object_isPassable_Patch
        {
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.EnableMod || !IsImmersive(__instance))
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.GetSprinklerTiles))]
        public class Object_GetSprinklerTiles_Patch
        {
            public static bool Prefix(Object __instance, ref List<Vector2> __result)
            {
                if (!Config.EnableMod || !__instance.modData.ContainsKey(sprinklerKey))
                    return true;
                __result = GetSprinklerTiles(__instance.TileLocation, GetSprinklerRadius(__instance));
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.ApplySprinklerAnimation))]
        public class Object_ApplySprinklerAnimation_Patch
        {
            public static bool Prefix(Object __instance)
            {
                if (!Config.EnableMod || !__instance.modData.ContainsKey(sprinklerKey))
                    return true;
                ApplySprinklerAnimation(__instance.TileLocation, GetSprinklerRadius(__instance), __instance.Location, Game1.random.Next(1000));
                return false;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod || (!__instance.IsSprinkler() && !__instance.IsScarecrow()))
                    return true;
                __instance.modData.Remove(sprinklerKey);
                __instance.modData.Remove(scarecrowKey);

                Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (!location.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt || location.Objects.ContainsKey(placementTile))
                    return true;
                SMonitor.Log($"Placing {__instance.Name} at {x},{y}");
                location.playSound("woodyStep");
                if (__instance.IsSprinkler())
                {
                    __instance.modData[sprinklerKey] = "true";
                }
                else
                {
                    __instance.modData[scarecrowKey] = "true";
                }
                location.objects.Add(placementTile, __instance);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha, int ____machineAnimationFrame, MachineEffects ____machineAnimation)
            {
                if (!Config.EnableMod || (!__instance.IsSprinkler() && !__instance.IsScarecrow()) || (!__instance.modData.ContainsKey(sprinklerKey) && !__instance.modData.ContainsKey(scarecrowKey)))
                    return true;
                GameLocation location = __instance.Location;
                Vector2 drawOffset = new(32, 16);
                if (__instance.bigCraftable.Value)
                {
                    Vector2 scaleFactor = __instance.getScale();
                    scaleFactor *= 4f;
                    Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)) + drawOffset);
                    Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                    float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 16) / 10000f) + (float)x * 1E-05f;
                    int offset = 0;
                    if (__instance.showNextIndex.Value)
                    {
                        offset = 1;
                    }
                    ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                    if (__instance.heldObject.Value != null)
                    {
                        MachineData machineData = __instance.GetMachineData();
                        if (machineData != null && machineData.IsIncubator)
                        {
                            FarmAnimalData animalDataFromEgg = FarmAnimal.GetAnimalDataFromEgg(__instance.heldObject.Value, location);
                            offset = ((animalDataFromEgg != null) ? animalDataFromEgg.IncubatorParentSheetOffset : 1);
                        }
                    }
                    if (____machineAnimationFrame >= 0 && ____machineAnimation != null)
                    {
                        offset = ____machineAnimationFrame;
                    }
                    Mannequin mannequin = __instance as Mannequin;
                    if (mannequin != null)
                    {
                        offset = mannequin.facing.Value;
                    }
                    if (__instance.QualifiedItemId == "(BC)272")
                    {
                        Texture2D texture = itemData.GetTexture();
                        spriteBatch.Draw(texture, destination, new Rectangle?(itemData.GetSourceRect(1, new int?(__instance.ParentSheetIndex))), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                        spriteBatch.Draw(texture, position + new Vector2(8.5f, 12f) * 4f, new Rectangle?(itemData.GetSourceRect(2, new int?(__instance.ParentSheetIndex))), Color.White * alpha, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * -1.5f, new Vector2(7.5f, 15.5f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
                        return false;
                    }
                    spriteBatch.Draw(itemData.GetTexture(), destination, new Rectangle?(itemData.GetSourceRect(offset, new int?(__instance.ParentSheetIndex))), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                    if (__instance.QualifiedItemId == "(BC)17" && __instance.MinutesUntilReady > 0)
                    {
                        spriteBatch.Draw(Game1.objectSpriteSheet, __instance.getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16)), Color.White * alpha, __instance.scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64) / 10000f + 0.0001f + (float)x * 1E-05f));
                    }
                    if (__instance.isLamp.Value && Game1.isDarkOut(__instance.Location))
                    {
                        spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Rectangle?(new Rectangle(88, 1779, 32, 32)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 16) / 10000f) + (float)x / 1000000f);
                    }
                    if (__instance.QualifiedItemId == "(BC)126")
                    {
                        string hatId = ((__instance.Quality != 0) ? (__instance.Quality - 1).ToString() : __instance.preservedParentSheetIndex.Value);
                        if (hatId != null)
                        {
                            ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(H)" + hatId);
                            Texture2D texture2 = dataOrErrorItem.GetTexture();
                            int spriteIndex = dataOrErrorItem.SpriteIndex;
                            bool isPrismatic = ItemContextTagManager.HasBaseTag("(H)" + hatId, "Prismatic");
                            spriteBatch.Draw(texture2, position + new Vector2(-3f, -6f) * 4f, new Rectangle?(new Rectangle(spriteIndex * 20 % texture2.Width, spriteIndex * 20 / texture2.Width * 20 * 4, 20, 20)), (isPrismatic ? Utility.GetPrismaticColor(0, 1f) : Color.White) * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 16) / 10000f) + (float)x * 1E-05f);
                        }
                    }
                }
                else if (!Game1.eventUp || (Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y)))
                {
                    Rectangle bounds = new Rectangle(x*64 , y * 64, 64, 64);
                    string qualifiedItemId = __instance.QualifiedItemId;
                    int layerOffset = 48;
                    if (qualifiedItemId == "(O)590")
                    {
                        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))) + drawOffset, new Rectangle?(new Rectangle(368 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0 <= 400.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 32, 16, 16)), Color.White * alpha, 0f, new Vector2(8f, 8f), (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(__instance.isPassable() ? bounds.Top + layerOffset : bounds.Bottom + layerOffset) / 10000f);
                        return false;
                    }
                    if (qualifiedItemId == "(O)SeedSpot")
                    {
                        spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))) + drawOffset, new Rectangle?(new Rectangle(160 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1600.0 <= 800.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 0, 17, 16)), Color.White * alpha, 0f, new Vector2(8f, 8f), (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1600.0 <= 400.0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(__instance.isPassable() ? bounds.Top + layerOffset : bounds.Bottom + layerOffset) / 10000f);
                        return false;
                    }
                    if (__instance.Fragility != 2)
                    {
                        spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 + 51 + 4))) + drawOffset, new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(bounds.Bottom + layerOffset) / 15000f);
                    }
                    ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                    spriteBatch.Draw(itemData2.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))) + drawOffset, new Rectangle?(itemData2.GetSourceRect(0, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(__instance.isPassable() ? bounds.Top + layerOffset : bounds.Center.Y + layerOffset) / 10000f);
                    if (__instance.IsSprinkler())
                    {
                        if (__instance.heldObject.Value != null)
                        {
                            Vector2 offset2 = Vector2.Zero;
                            if (__instance.heldObject.Value.QualifiedItemId == "(O)913")
                            {
                                offset2 = new Vector2(0f, -20f);
                            }
                            ParsedItemData heldItemData = ItemRegistry.GetDataOrErrorItem(__instance.heldObject.Value.QualifiedItemId);
                            spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))) + offset2) + drawOffset, new Rectangle?(heldItemData.GetSourceRect(1, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(__instance.isPassable() ? bounds.Top + layerOffset : bounds.Bottom + layerOffset) / 10000f + 1E-05f);
                        }
                        if (__instance.SpecialVariable == 999999)
                        {
                            if (__instance.heldObject.Value != null && __instance.heldObject.Value.QualifiedItemId == "(O)913")
                            {
                                Torch.drawBasicTorch(spriteBatch, (float)(x * 64) - 2f + 32, (float)(y * 64 - 32) + 16, (float)(bounds.Bottom + layerOffset) / 10000f + 1E-06f, 1f);
                            }
                            else
                            {
                                Torch.drawBasicTorch(spriteBatch, (float)(x * 64) - 2f + 32, (float)(y * 64 - 32 + 12) + 16, (float)(bounds.Bottom + 2 + layerOffset) / 10000f, 1f);
                            }
                        }
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public class Object_canBePlacedHere_Patch
        {
            public static bool Prefix(Object __instance, GameLocation l, Vector2 tile, ref bool __result)
            {
                if (!Config.EnableMod || (!__instance.IsSprinkler() && !__instance.IsScarecrow()) || !l.terrainFeatures.TryGetValue(tile, out var tf) || tf is not HoeDirt || l.objects.ContainsKey(tile))
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
                if (!Config.EnableMod || item is not Object || (!(item as Object).IsSprinkler() && !(item as Object).IsScarecrow()) || !location.terrainFeatures.TryGetValue(new Vector2(x / 64, y / 64), out var tf) || tf is not HoeDirt || location.objects.ContainsKey(new Vector2(x / 64, y / 64)))
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
                if (!Config.EnableMod || !Context.IsPlayerFree || Game1.currentLocation?.terrainFeatures?.TryGetValue(Game1.currentCursorTile, out var tf) != true || tf is not HoeDirt)
                    return true;
                bool sprinkler = false;
                bool scarecrow = false;
                if (__instance.IsSprinkler())
                {
                    sprinkler = true;
                }
                if (__instance.IsScarecrow())
                {
                    scarecrow = true;
                }
                if (!sprinkler && !scarecrow)
                    return true;
                var cursorTile = Game1.currentCursorTile;

                Vector2 pos = Game1.GlobalToLocal(cursorTile * 64 + new Vector2(32,32) - new Vector2(0, 16));

                spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(Utility.withinRadiusOfPlayer((int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64, 1, Game1.player) ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);

                if (Config.ShowRangeWhenPlacing)
                {
                    HashSet<Vector2> tiles = new();
                    if (sprinkler)
                    {
                        foreach(var t in GetSprinklerTiles(cursorTile, GetSprinklerRadius(__instance)))
                            tiles.Add(t);
                    }
                    if (scarecrow)
                    {
                        foreach (var t in GetScarecrowTiles(cursorTile, __instance.GetRadiusForScarecrow()))
                            tiles.Add(t);
                    }
                    foreach (var tile in tiles)
                    {
                        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(tile * 64), new Rectangle(194, 388, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    }
                }
                if(__instance.bigCraftable.Value)
                    pos -= new Vector2(0, 64);
                Texture2D texture = null;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                Rectangle sourceRect = itemData.GetSourceRect(__instance is Mannequin ? 2 : 0, new int?(__instance.ParentSheetIndex));
                if (atApi is not null && __instance.modData.ContainsKey("AlternativeTextureName"))
                {
                    texture = GetAltTextureForObject(__instance, out sourceRect);
                }
                else
                {
                    texture = itemData.GetTexture();
                }
                if (texture is null)
                {
                    return false;
                }
                spriteBatch.Draw(texture, pos, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.02f);

                return false;
            }

        }
        [HarmonyPatch(typeof(Pickaxe), nameof(Pickaxe.DoFunction))]
        public class Pickaxe_DoFunction_Patch
        {
            public static bool Prefix(Pickaxe __instance, GameLocation location, int x, int y, Farmer who, ref (Vector2 tile, Object obj) __state)
            {
                return true;

                if (!Config.EnableMod)
                    return true;
                int tileX = x / 64;
                int tileY = y / 64;
                Vector2 tile = new Vector2(tileX, tileY);

                var xx = x - 32;
                var yy = y - 32;
                int tileXX = xx / 64;
                int tileYY = yy / 64;
                Vector2 tile2 = new Vector2(tileXX, tileYY);
                if (location.Objects.TryGetValue(tile2, out var obj) && IsImmersive(obj) && obj.performToolAction(__instance))
                {
                    obj.modData.Remove(sprinklerKey);
                    obj.modData.Remove(scarecrowKey);
                    if (obj.Type == "Crafting" && obj.Fragility != 2)
                    {
                        who.currentLocation.debris.Add(new Debris(obj.QualifiedItemId, who.GetToolLocation(false), Utility.PointToVector2(who.StandingPixel)));
                    }
                    obj.performRemoveAction();
                    who.currentLocation.Objects.Remove(tile2);
                    return false;
                }
                else if (location.Objects.TryGetValue(tile, out obj) && IsImmersive(obj))
                {
                    __state = new(tile, obj);
                    location.Objects.Remove(tile);
                    return true;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Axe), nameof(Axe.DoFunction))]
        public class Axe_DoFunction_Patch
            {
            public static bool Prefix(Pickaxe __instance, GameLocation location, int x, int y, Farmer who, ref (Vector2 tile, Object obj) __state)
            {
                return true;

                if (!Config.EnableMod)
                    return true;
                int tileX = x / 64;
                int tileY = y / 64;
                Vector2 tile = new Vector2(tileX, tileY);

                var xx = x - 32;
                var yy = y - 32;
                int tileXX = xx / 64;
                int tileYY = yy / 64;
                Vector2 tile2 = new Vector2(tileXX, tileYY);
                if(location.Objects.TryGetValue(tile2, out var obj) && IsImmersive(obj) && obj.performToolAction(__instance))
                {
                    obj.modData.Remove(sprinklerKey);
                    obj.modData.Remove(scarecrowKey);
                    if (obj.Type == "Crafting" && obj.Fragility != 2)
                    {
                        who.currentLocation.debris.Add(new Debris(obj.QualifiedItemId, who.GetToolLocation(false), Utility.PointToVector2(who.StandingPixel)));
                    }
                    obj.performRemoveAction();
                    who.currentLocation.Objects.Remove(tile2);
                    return false;
                }
                else if (location.Objects.TryGetValue(tile, out obj) && IsImmersive(obj))
                {
                    __state = new(tile, obj);
                    location.Objects.Remove(tile);
                    return true;
                }
                return true;
            }
            public static void Postfix(GameLocation location, (Vector2 tile, Object obj) __state)
            {
                if (__state.obj != null) 
                    location.Objects[__state.tile] = __state.obj;
            }
        }
    }
}