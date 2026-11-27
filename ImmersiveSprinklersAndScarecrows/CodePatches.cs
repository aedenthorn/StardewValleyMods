using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklersAndScarecrows
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                Vector2 placementTile = new Vector2(tileLocation.X, tileLocation.Y);
                var mouseTile = GetMouseCornerTile();
                if (Vector2.Distance(placementTile, mouseTile.ToVector2()) >= 3)
                    return true; 
                var obj = GetSprinklerAtMouse();

                if (obj is not null)
                {
                    var tile = obj.TileLocation;
                    int x = (int)tile.X;
                    int y = (int)tile.Y;
                    if (obj.heldObject.Value is not Object held || held.ItemId == "913" || held.ItemId == "915") // 913 enricher 915 nozzle
                    {
                        if (who.CurrentItem?.ItemId == "913" && obj.heldObject.Value?.ItemId == "915")
                        {
                            obj.modData[nozzleKey] = "true";
                            obj.heldObject.Value = new Object("913", 1);
                            obj.heldObject.Value.heldObject.Value = new Chest
                            {
                                SpecialChestType = Chest.SpecialChestTypes.Enricher
                            };
                            who.reduceActiveItemByOne();
                            __instance.playSound("axe");

                        }
                        else if (who.CurrentItem?.ItemId == "915" && obj.heldObject.Value?.ItemId == "913")
                        {
                            if (obj.modData.ContainsKey(nozzleKey))
                            {
                                return true;
                            }
                            obj.modData[nozzleKey] = "true";
                            who.reduceActiveItemByOne();
                            __instance.playSound("axe");
                        }
                        else if (who.CurrentItem?.ItemId == "915" && obj.heldObject.Value is null)
                        {
                            obj.heldObject.Value = new Object("915", 1);
                            obj.modData.Remove(nozzleKey);
                            who.reduceActiveItemByOne();
                            __instance.playSound("axe");
                        }
                        else if (who.CurrentItem?.ItemId == "913" && obj.heldObject.Value is null)
                        {
                            obj.modData.Remove(nozzleKey);
                            obj.heldObject.Value = new Object("913", 1);
                            obj.heldObject.Value.heldObject.Value = new Chest
                            {
                                SpecialChestType = Chest.SpecialChestTypes.Enricher
                            };
                            who.reduceActiveItemByOne();
                            __instance.playSound("axe");
                        }
                        else if ((who.CurrentItem is null || who.CurrentItem?.Category == -19) && obj.heldObject.Value?.ItemId == "913")
                        {

                            Object value = obj.heldObject.Value.heldObject.Value;
                            Chest chest = value as Chest;
                            if (chest != null)
                            {
                                IInventory items = chest.Items;
                                if(who.CurrentItem is null)
                                {
                                    if (items.Count > 0 && items[0] is Item f)
                                    {
                                        TryReturnObject(f, who);
                                        chest.Items.Remove(f);
                                        __instance.playSound("dirtyHit");
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    if (items.Count > 0 && items[0] is Item f)
                                    {
                                        if (f.canStackWith(who.CurrentItem))
                                        {
                                            int back = f.addToStack(who.CurrentItem);
                                            if (back == 0)
                                            {
                                                who.removeItemFromInventory(who.CurrentItem);
                                            }
                                        }
                                        else
                                        {
                                            TryReturnObject(f, who);
                                            items[0] = who.CurrentItem;
                                        }
                                    }
                                    else
                                    {
                                        items.Clear();
                                        items.Add(who.CurrentItem);
                                        who.removeItemFromInventory(who.CurrentItem);
                                        who.showNotCarrying();
                                    }
                                    __instance.playSound("dirtyHit");
                                }
                            }
                        }
                        else
                        {
                            return true;
                        }
                        PlaceObject(obj, __instance, x, y, true);
                        __result = true;
                        return false;
                    }

                    return true;
                }
                obj = GetScarecrowAtMouse();
                if (obj != null)
                {
                    var tile = obj.TileLocation;
                    int x = (int)tile.X;
                    int y = (int)tile.Y;
                    if (obj.ParentSheetIndex == 126)
                    {
                        if (who.CurrentItem is Hat hat)
                        {
                            if (obj.preservedParentSheetIndex.Value != null)
                            {
                                Game1.createItemDebris(new Hat(obj.preservedParentSheetIndex.Value), Game1.currentCursorTile * 64f, (who.FacingDirection + 2) % 4, null, -1);
                                obj.preservedParentSheetIndex.Value = null;
                            }
                            obj.preservedParentSheetIndex.Value = hat.ItemId;
                            who.Items[who.CurrentToolIndex] = null;
                            who.currentLocation.playSound("dirtyHit");

                        }
                        else if (who.CurrentItem is null && obj.preservedParentSheetIndex.Value != null)
                        {
                            who.Items[who.CurrentToolIndex] = new Hat(obj.preservedParentSheetIndex.Value);
                            obj.preservedParentSheetIndex.Value = null;
                            who.currentLocation.playSound("dirtyHit");
                        }
                        else
                        {
                            return true;
                        }
                        PlaceObject(obj, __instance, x, y, true);
                        __result = true;
                        return false;
                    }
                    if (Game1.didPlayerJustRightClick(true))
                    {
                        var scared = obj.SpecialVariable;
                        if (scared == 0)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
                        }
                        else
                        {
                            Game1.drawObjectDialogue((scared == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", scared));
                        }
                        __result = true;
                        return false;
                    }

                }
                    
                return true;


            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                Point placementTile = GetMouseCornerTile();
                Vector2 tileVector = placementTile.ToVector2();

                int tileX = placementTile.X;
                int tileY = placementTile.Y;

                var one = (Object)__instance.getOne();
                one.TileLocation = tileVector;
                if (one.IsSprinkler())
                {
                    if (!CheckForHoeDirt(location, tileVector))
                        return true;

                    SMonitor.Log($"Placing {one.Name} at {tileX},{tileY}");

                    location.playSound("woodyStep");

                    ReturnOrDropSprinkler(location, tileX, tileY, who, false);
                    ReturnOrDropScarecrow(location, tileX, tileY, who, false);

                    PlaceObject(one, location, tileX, tileY, true);

                    __result = true;
                    return false;
                }
                else if (one.IsScarecrow())
                {
                    if (!CheckForHoeDirt(location, tileVector))
                        return true;
                    SMonitor.Log($"Placing {one.Name} at {tileX},{tileY}");

                    location.playSound("woodyStep");

                    ReturnOrDropSprinkler(location, tileX, tileY, who, false);
                    ReturnOrDropScarecrow(location, tileX, tileY, who, false);

                    PlaceObject(one, location, tileX, tileY, false);

                    __result = true;
                    return false;
                }
                else if (one.Category == -74 && location.terrainFeatures.TryGetValue(new Vector2(x/64, y/64), out var tf) && tf is HoeDirt dirt)
                {
                    bool update = false;
                    foreach (var s in GetSprinklers(location))
                    {

                        if (s.heldObject.Value is Object obj && obj.ItemId == "913" && obj.heldObject.Value is Chest chest && chest.Items.Count > 0 && chest.Items[0] is Item f)
                        {
                            var radius = s.GetModifiedRadiusForSprinkler();
                            if (GetSprinklerTiles(s.TileLocation, radius).Contains(tileVector) && dirt.plant(f.ItemId, who, true))
                            {
                                f.Stack--;
                                if (f.Stack <= 0)
                                {
                                    chest.Items.RemoveAt(0);
                                }
                                SetData(location, (int)s.TileLocation.X, (int)s.TileLocation.Y, GetImmersiveData(s, true));
                                update = true;
                                break;
                            }
                        }
                    }
                    if (update)
                    {
                        SendMessage(location.NameOrUniqueName, "sprinklers");
                        ReloadSprinklers(location);
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.DayUpdate))]
        public class GameLocation_DayUpdate_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.EnableMod || (__instance.IsOutdoors && Game1.IsRainingHere(__instance)))
                    return;
                foreach(var obj in GetSprinklers(__instance))
                {
                    if (obj is not null)
                    {
                        obj.Location = __instance;
                        __instance.postFarmEventOvernightActions.Add(delegate
                        {
                            ActivateSprinkler(__instance, obj.TileLocation, obj, true);
                        });
                    }
                }
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw))]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.EnableMod)
                    return;
                if(sprinklerDict.TryGetValue(__instance, out var dict))
                {
                    foreach (var obj in dict.Values)
                    {
                        if (obj is null)
                            continue;
                        Vector2 globalPosition = obj.TileLocation * 64 + new Vector2(32, 16);
                        if (obj.bigCraftable.Value)
                            globalPosition -= new Vector2(0, 64);
                        var layerDepth = (globalPosition.Y + (obj.bigCraftable.Value ? 81 : 33) + Config.DrawOffsetZ) / 10000f;
                        var position = Game1.GlobalToLocal(globalPosition) + new Vector2(Config.DrawOffsetX, Config.DrawOffsetY);
                        b.Draw(Game1.shadowTexture, position + new Vector2(32, 55), Game1.shadowTexture.Bounds, Color.White * Config.Alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Config.Scale, SpriteEffects.None, obj.GetBoundingBoxAt((int)obj.TileLocation.X, (int)obj.TileLocation.Y).Bottom / 15000f);
                        Texture2D texture = null;
                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                        Rectangle sourceRect = itemData.GetSourceRect(obj is Mannequin ? 2 : 0, new int?(obj.ParentSheetIndex));
                        
                        if (atApi is not null && obj.modData.ContainsKey("AlternativeTextureName"))
                        {
                            texture = GetAltTextureForObject(obj, out sourceRect);
                        }
                        else
                        {
                            texture = itemData.GetTexture();
                        }
                        if (texture is null)
                        {
                            texture = obj.bigCraftable.Value ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet;
                            sourceRect = obj.bigCraftable.Value ? Object.getSourceRectForBigCraftable(obj.ParentSheetIndex) : GameLocation.getSourceRectForObject(obj.ParentSheetIndex);
                        }
                        b.Draw(texture, position, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

                        if (obj.heldObject.Value is Object held)
                        {
                            if (held.ItemId == "913")
                            {
                                var epos = position + new Vector2(0f, -20f);
                                b.Draw(Game1.objectSpriteSheet, epos, GameLocation.getSourceRectForObject(914), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 2E-05f);
                                if(held.heldObject.Value is Chest c && c.Items.Count > 0 && c.Items[0] is Item f)
                                {
                                    var mousePos = Game1.getMousePosition();
                                    if (epos.X <= mousePos.X && epos.X + 64 > mousePos.X && epos.Y <= mousePos.Y && epos.Y + 64 > mousePos.Y)
                                    {
                                        f.drawInMenu(b, epos + new Vector2(0, -48), 1);
                                    }
                                }
                                if (obj.modData.ContainsKey(nozzleKey))
                                {
                                    b.Draw(Game1.objectSpriteSheet, position, GameLocation.getSourceRectForObject(916), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 1E-05f);
                                }
                            }
                            else if (held.ItemId == "915")
                            {
                                b.Draw(Game1.objectSpriteSheet, position, GameLocation.getSourceRectForObject(916), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 1E-05f);
                            }
                        }
                    }
                }

                if (scarecrowDict.TryGetValue(__instance, out var dict2))
                {
                    foreach (var obj in dict2.Values)
                    {
                        if (obj is null)
                            continue;
                        Vector2 scaleFactor = obj.getScale();

                        Vector2 globalPosition = obj.TileLocation * 64 + new Vector2(32f, 16);
                        if (obj.bigCraftable.Value)
                            globalPosition -= new Vector2(0, 64);
                        var layerDepth = (globalPosition.Y + (obj.bigCraftable.Value ? 81 : 33) + 24 + Config.DrawOffsetZ) / 10000f;
                        var position = Game1.GlobalToLocal(globalPosition) + new Vector2(Config.DrawOffsetX, Config.DrawOffsetY);
                        if (!obj.bigCraftable.Value)
                            b.Draw(Game1.shadowTexture, position + new Vector2(32, 55), Game1.shadowTexture.Bounds, Color.White * Config.Alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Config.Scale, SpriteEffects.None, obj.GetBoundingBoxAt((int)obj.TileLocation.X, (int)obj.TileLocation.Y).Bottom / 15000f);


                        Texture2D texture = null;
                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                        Rectangle sourceRect;

                        if (atApi is not null && obj.modData.ContainsKey("AlternativeTextureName"))
                        {
                            texture = GetAltTextureForObject(obj, out sourceRect);
                        }
                        else
                        {
                            texture = itemData.GetTexture();
                            sourceRect = itemData.GetSourceRect(obj is Mannequin ? 2 : 0, new int?(obj.ParentSheetIndex));
                        }
                        if (texture is null)
                        {
                            texture = obj.bigCraftable.Value ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet;
                            sourceRect = obj.bigCraftable.Value ? Object.getSourceRectForBigCraftable(obj.ParentSheetIndex) : GameLocation.getSourceRectForObject(obj.ParentSheetIndex);
                        }

                        b.Draw(texture, position, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, SpriteEffects.None, layerDepth);

                        string hatId = obj.preservedParentSheetIndex.Value;
                        if(hatId != null)
                        {
                            ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(H)" + hatId);
                            Texture2D texture2 = dataOrErrorItem.GetTexture();
                            int spriteIndex = dataOrErrorItem.SpriteIndex;
                            bool isPrismatic = ItemContextTagManager.HasBaseTag("(H)" + hatId, "Prismatic");

                            b.Draw(texture2, position + new Vector2(-3f, -6f) * 4f, new Rectangle(spriteIndex * 20 % texture2.Width, spriteIndex * 20 / texture2.Width * 20 * 4, 20, 20), (isPrismatic ? Utility.GetPrismaticColor(0, 1f) : Color.White) * Config.Alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 1E-05f);
                        }
                    }
                }
            }

        }
        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public class Object_canBePlacedHere_Patch
        {
            public static bool Prefix(Object __instance, GameLocation l, Vector2 tile, ref bool __result)
            {
                if (!Config.EnableMod || (!__instance.IsSprinkler() && !__instance.IsScarecrow()))
                    return true;

                if (!CheckForHoeDirt(l, GetMouseCornerTile().ToVector2()))
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
                if (!Config.EnableMod || item is not Object obj || (!obj.IsSprinkler() && !obj.IsScarecrow()))
                    return true;
                if (!CheckForHoeDirt(location, new Vector2(x / 64, y / 64)))
                {
                    return true;
                }
                __result = Utility.withinRadiusOfPlayer(x, y, 1, Game1.player);
                return false;
            }

        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawPlacementBounds))]
        public class Object_drawPlacementBounds_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, GameLocation location)
            {
                if (!Config.EnableMod || !Context.IsPlayerFree || (!__instance.IsSprinkler() && !__instance.IsScarecrow()))
                    return true;
                var mouseTile = GetMouseCornerTile().ToVector2();
                if (!CheckForHoeDirt(location, mouseTile))
                {
                    return true;
                }
                Vector2 pos = Game1.GlobalToLocal(mouseTile * 64 + new Vector2(32, 16));

                spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(Utility.withinRadiusOfPlayer((int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64, 1, Game1.player) ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);

                if (Config.ShowRangeWhenPlacing)
                {
                    foreach(var tile in __instance.IsSprinkler() ? GetSprinklerTiles(mouseTile, GetSprinklerRadius(__instance)) : GetScarecrowTiles(mouseTile, __instance.GetRadiusForScarecrow()))
                    {
                        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(tile * 64), new Rectangle(194, 388, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    }
                }
                if(__instance.bigCraftable.Value)
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
                spriteBatch.Draw(texture, pos, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.02f);

                return false;
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

                var tile = GetMouseCornerTile();
                if (Vector2.Distance(tile.ToVector2(), new Vector2(x, y) / 64) < 3 && (ReturnOrDropSprinkler(location, tile.X, tile.Y, who, true) || ReturnOrDropScarecrow(location, tile.X, tile.Y, who, true)))
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
                var tile = GetMouseCornerTile();
                if (Vector2.Distance(tile.ToVector2(), new Vector2(x, y) / 64) < 3 && (ReturnOrDropSprinkler(location, tile.X, tile.Y, who, true) || ReturnOrDropScarecrow(location, tile.X, tile.Y, who, true)))
                {
                    location.playSound("hammer");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix()
            {
                if (!Config.EnableMod || !Context.IsPlayerFree || Game1.player.ActiveItem is not Tool tool || tool.ItemId != "PeacefulEnd.AlternativeTextures_PaintBucket" || atApi is null)
                    return true;
                var tile = GetMouseCornerTile();
                if (Vector2.Distance(tile.ToVector2(), Game1.player.Position / 64) >= 3)
                    return true;

                var sc = GetScarecrowAtMouse();
                if(sc != null)
                {
                    OpenPaintMenu(tool, sc);
                    return false;
                }
                var sp = GetSprinklerAtMouse();
                if(sp != null)
                {
                    OpenPaintMenu(tool, sp);
                    return false;
                }
                return true;
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
                    if (i < codes.Count - 4 && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Brtrue_S && codes[i + 4].opcode == OpCodes.Callvirt && codes[i + 4].operand is MethodInfo && (MethodInfo)codes[i + 4].operand == AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.destroyCrop)))
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

        [HarmonyPatch(typeof(Utility), nameof(Utility.doesItemExistAnywhere))]
        public class Utility_doesItemExistAnywhere_Patch
        {
            public static void Postfix(string itemId, ref bool __result)
            {
                if (!Config.EnableMod || !Context.IsWorldReady || __result || (!new string[] { "(BC)136", "(BC)137", "(BC)138", "(BC)139", "(BC)140", "(BC)126", "(BC)110", "(BC)113" }.Contains(itemId)))
                    return;
                foreach (var kvp in scarecrowDict)
                {
                    foreach(var kvp2 in kvp.Value)
                    {
                        if (kvp2.Value?.ItemId == itemId)
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