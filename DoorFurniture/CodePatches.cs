using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace DoorFurniture
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SliderBar), nameof(SliderBar.click))]
        public static class SliderBar_click_Patch
        {
            public static bool Prefix(SliderBar __instance, int x, int y, ref int __result)
            {
                if (!Config.ModEnabled || !Config.FixSliderBar)
                {
                    return true;
                }
                if (__instance.bounds.Contains(x, y))
                {
                    float fx = x - __instance.bounds.X;
                    __instance.value = (int)Math.Ceiling(fx / __instance.bounds.Width * 100f);
                }
                __result = __instance.value;
                return false;
            }
        }
        [HarmonyPatch(typeof(ColorPicker), nameof(ColorPicker.click))]
        public static class ColorPicker_click_Patch
        {
            public static void Prefix(ColorPicker __instance, Rectangle ___bounds, ref int x)
            {
                if (!Config.ModEnabled || !Config.FixSliderBar)
                {
                    return;
                }
                if(x < ___bounds.X && ___bounds.X - x < 8)
                {
                    x = ___bounds.X;
                }
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Furniture_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Furniture.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Furniture), nameof(Furniture.HasSittingFarmers)))
                    {
                        SMonitor.Log($"adding check for rotated doors");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckRotatedDoor))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Prefix(Furniture __instance, int x, int y, NetInt ___sourceIndexOffset)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(openKey, out var str))
                {
                    return;
                }
                bool flipped = false;
                if (__instance.modData.TryGetValue(flipKey, out var fs))
                {
                    _ = bool.TryParse(fs, out flipped);
                }
                if (str == "closed")
                {
                    switch (__instance.currentRotation.Value)
                    {
                        case 0:
                            ___sourceIndexOffset.Value = 0;
                            __instance.Flipped = flipped;
                            break;
                        case 1:
                            ___sourceIndexOffset.Value = flipped ? 6 : 0;
                            __instance.Flipped = false;
                            break;
                        case 2:
                            ___sourceIndexOffset.Value = 0;
                            __instance.Flipped = flipped;
                            break;
                        case 3:
                            ___sourceIndexOffset.Value = flipped ? 6 : 0;
                            __instance.Flipped = true;
                            break;
                    }
                    return;
                }

                if (str == "open")
                {
                    switch (__instance.currentRotation.Value)
                    {
                        case 0:
                            ___sourceIndexOffset.Value = 1;
                            __instance.Flipped = !flipped;
                            break;
                        case 1:
                            ___sourceIndexOffset.Value = flipped ? 1 : -1;
                            __instance.Flipped = flipped ? false : true;
                            break;
                        case 2:
                            ___sourceIndexOffset.Value = 5;
                            __instance.Flipped = flipped;
                            break;
                        case 3:
                            ___sourceIndexOffset.Value = flipped ? 1 : -1;
                            __instance.Flipped = flipped ? true : false;
                            break;
                    }
                    if (__instance.modData.TryGetValue(closeKey, out var cs))
                    {
                        bool can = true;
                        var box = __instance.GetBoundingBox();
                        box.Inflate(Config.PreventAutoCloseBuffer, Config.PreventAutoCloseBuffer);
                        foreach (var f in __instance.Location.farmers)
                        {
                            if (box.Intersects(f.GetBoundingBox()))
                            {
                                can = false;
                                break;
                            }
                        }
                        foreach (var c in __instance.Location.characters)
                        {
                            if (box.Intersects(c.GetBoundingBox()))
                            {
                                can = false;
                                break;
                            }
                        }
                        if (can)
                        {
                            if (cs == "0")
                            {
                                __instance.modData.Remove(closeKey);
                                CloseDoor(__instance, null);
                                str = "-1";
                            }
                            else
                            {
                                __instance.modData[closeKey] = (int.Parse(cs) - 1).ToString();
                            }
                        }
                    }
                }
                if (str != "open")
                {
                    int open = int.Parse(str);
                    int mult = 2;
                    switch (__instance.currentRotation.Value)
                    {
                        case 0:
                            __instance.Flipped = flipped;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 * mult ? 3 : 4;
                            else
                                ___sourceIndexOffset.Value = open > -3 * mult ? 4 : 3;
                            break;
                        case 1:
                            __instance.Flipped = flipped ? false : true;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 * mult ? (flipped ? 5 : 3) : (flipped ? 4 : 2);
                            else
                                ___sourceIndexOffset.Value = open > -3 * mult ? (flipped ? 4 : 2) : (flipped ? 5 : 3);
                            break;
                        case 2:
                            __instance.Flipped = flipped;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 * mult ? 3 : 4;
                            else
                                ___sourceIndexOffset.Value = open > -3 * mult ? 4 : 3;
                            break;
                        case 3:
                            __instance.Flipped = flipped ? true : false;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 * mult ? (flipped ? 5 : 3) : (flipped ? 4 : 2);
                            else
                                ___sourceIndexOffset.Value = open > -3 * mult ? (flipped ? 4 : 2) : (flipped ? 5 : 3);
                            break;
                    }
                    if (open < 0)
                        open--;
                    else
                        open++;
                    if (open < -4 * mult)
                    {
                        __instance.modData[openKey] = "closed";
                    }
                    else if (open > 4 * mult)
                    {
                        __instance.modData[openKey] = "open";
                    }
                    else
                    {
                        __instance.modData[openKey] = open.ToString();
                    }
                }
            }
            public static void Postfix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, NetInt ___sourceIndexOffset, NetVector2 ___drawPosition)
            {
                if (!Config.ModEnabled || __instance.isTemporarilyInvisible || !__instance.modData.TryGetValue(colorKey, out var str) || !TryGetDoorData(__instance, out var data) || !data.Colorable)
                    return;
                Color color = Utility.StringToColor(str) ?? data.DefaultColor;
                if (data.DefaultUncolored && color == data.DefaultColor)
                {
                    return;
                }
                color *= alpha;
                Rectangle drawn_source_rect = __instance.sourceRect.Value;
                drawn_source_rect.X += drawn_source_rect.Width * ___sourceIndexOffset.Value;
                drawn_source_rect.Offset(data.ColorSpriteOffset);
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                Texture2D texture = itemData.GetTexture();
                string textureName = itemData.TextureName;
                if (itemData.IsErrorItem)
                {
                    return;
                }

                if (Furniture.isDrawingLocationFurniture)
                {

                    Vector2 actualDrawPosition = Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero));
                    SpriteEffects spriteEffects = (__instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                    if (__instance.currentRotation.Value == 2 && __instance.modData.TryGetValue(openKey, out var open) && open == "closed")
                    {
                        spriteBatch.Draw(texture, actualDrawPosition, new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (float)(__instance.boundingBox.Value.Top + 16 + 1) / 10000f);
                    }
                    else
                    {
                        spriteBatch.Draw(texture, actualDrawPosition, new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (__instance.boundingBox.Value.Bottom - 8 + 1) / 10000f);
                    }
                }
                else
                {
                    spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 - (drawn_source_rect.Height * 4 - __instance.boundingBox.Height) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 8 + 1) / 10000f);
                }
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) })]
        public static class Furniture_drawInMenu_Patch
        {
            public static void Postfix(Furniture __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(colorKey, out var str) || !TryGetDoorData(__instance, out var data) || !data.Colorable)
                    return;
                Color color = Utility.StringToColor(str) ?? data.DefaultColor;
                if (data.DefaultUncolored && color == data.DefaultColor)
                {
                    return;
                }
                color *= transparency;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                Rectangle sourceRect = itemData.GetSourceRect(0, null);
                sourceRect.Offset(data.ColorSpriteOffset);
                spriteBatch.Draw(itemData.GetTexture(), location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height / 2)), scaleSize, SpriteEffects.None, layerDepth);
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.checkForAction))]
        public static class Furniture_checkForAction_Patch
        {
            public static bool Prefix(Furniture __instance, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || !TryGetDoorData(__instance, out var data))
                {
                    return true;
                }
                if (!__instance.modData.TryGetValue(openKey, out var open))
                {
                    open = "closed";
                    __instance.modData[openKey] = open;
                }
                if (open != "closed" && open != "open")
                    return false;
                if (justCheckingForActivity)
                {
                    __result = true;
                    return false;
                }
                if (open == "closed")
                {
                    __result = OpenDoor(__instance, data);
                }
                else
                {
                    __result = true;
                    CloseDoor(__instance, data);

                }
                return false;
            }

        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        public static class GameLocation_isCollidingPosition_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GameLocation.isCollidingPosition");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Furniture), nameof(Furniture.IntersectsForCollision)))
                    {
                        SMonitor.Log($"adding override for door furniture");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand =  AccessTools.Method(typeof(ModEntry), nameof(CheckForDoorCollision));
                        codes.Insert(i, new(OpCodes.Ldarg_S, 6));
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.isPassable))]
        public static class Furniture_isPassable_Patch
        {
            public static bool Prefix(Furniture __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !IsDoor(__instance))
                    return true;
                var x = Environment.StackTrace;
                __result = true;
                return false;
            }

        }

        //[HarmonyPatch(typeof(Furniture), nameof(Furniture.IntersectsForCollision))]
        public static class Furniture_IntersectsForCollision_Patch
        {
            public static bool Prefix(Furniture __instance, Rectangle rect, ref bool __result)
            {
                if (!Config.ModEnabled || !TryGetDoorData(__instance, out var data))
                    return true;
                if (!__instance.modData.TryGetValue(openKey, out var open))
                {
                    open = "closed";
                    __instance.modData[openKey] = open;
                }
                else if (open != "closed")
                {
                    return false;
                }

                var rot = __instance.currentRotation.Value;
                var loc = __instance.GetBoundingBox().Location + data.Bounds[rot].Location;
                var bounds = new Rectangle(loc, data.Bounds[rot].Size);
                __result = bounds.Intersects(rect);
                if (__result)
                {
                    if (data.AutoOpen || Config.AutoOpen)
                    {
                        __result = OpenDoor(__instance, data);
                    }
                    else
                    {
                        rect.Inflate(8, 8);
                        foreach (var c in __instance.Location.characters)
                        {
                            if (c is not Monster && c?.GetBoundingBox().Intersects(rect) == true)
                            {

                                __result = OpenDoor(__instance, data, true);
                                return false;
                            }
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.getCategoryName))]
        public static class Object_getCategoryName_Patch
        {
            public static bool Prefix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (__instance.Type == "DoorKey")
                {
                    __result = SHelper.Translation.Get("aedenthorn.DoorFurniture_key");
                    return false;
                }
                if (__instance.Type == "KeyRing")
                {
                    __result = SHelper.Translation.Get("aedenthorn.DoorFurniture_keyring");
                    return false;
                }
                if (__instance is Furniture f && TryGetDoorData(f, out var data))
                {
                    __result = data.Type;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.getCategoryColor))]
        public static class Object_getCategoryColor_Patch
        {
            public static bool Prefix(Object __instance, ref Color __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (__instance.Type == "DoorKey")
                {
                    __result = Config.KeyCategoryColor;
                    return false;
                }
                if (__instance.Type == "KeyRing")
                {
                    __result = Config.KeyRingCategoryColor;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(Furniture), nameof(Furniture.placementAction))]
        public static class Furniture_placementAction_Patch
        {
            public static void Postfix(Furniture __instance)
            {
                if (!Config.ModEnabled || !TryGetDoorData(__instance, out var data))
                    return;
                __instance.modData[openKey] = "closed";
                if(data.Colorable && !__instance.modData.ContainsKey(colorKey))
                    __instance.modData[colorKey] = ColorToHexString(data.DefaultColor);
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.getCategoryColor))]
        public static class Furniture_getCategoryColor_Patch
        {
            public static bool Prefix(Furniture __instance, ref Color __result)
            {
                if (!Config.ModEnabled || !IsDoor(__instance))
                    return true;
                __result = Config.DoorCategoryColor;
                return false;
            }
        }

        [HarmonyPatch(typeof(Furniture), "loadDescription")]
        public static class Furniture_loadDescription_Patch
        {
            public static bool Prefix(Furniture __instance, ref string __result)
            {
                if (!Config.ModEnabled || !TryGetDoorData(__instance, out var data))
                    return true;
                __result = data.Description;
                return false;
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.HasSittingFarmers))]
        public static class Furniture_HasSittingFarmers_Patch
        {
            public static bool Prefix(Furniture __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(lockKey, out var str2) || str2 != "True")
                    return true;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(GameLocation), "removeQueuedFurniture")]
        public static class GameLocation_removeQueuedFurniture_Patch
        {
            public static void Prefix(GameLocation __instance, Guid guid)
            {
                if (!Config.ModEnabled || !__instance.furniture.TryGetValue(guid, out var f) || !IsDoor(f))
                    return;
                f.modData[openKey] = "closed";
                f.modData.Remove(closeKey);
            }
        }

        [HarmonyPatch(typeof(Object), "loadDisplayName")]
        public static class Object_loadDisplayName_Patch
        {
            public static bool Prefix(Object __instance, ref string __result)
            {
                return !Config.ModEnabled || !__instance.modData.TryGetValue(nameKey, out __result);
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.maximumStackSize))]
        public static class Object_maximumStackSize_Patch
        {
            public static bool Prefix(Object __instance, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(guidKey))
                    return true;
                __result = 1;
                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                var mousePos = Game1.getMousePosition(true);
                for (int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if (cc.containsPoint(mousePos.X, mousePos.Y))
                    {
                        if (__instance.actualInventory.Count > i && __instance.actualInventory[i] is Object obj)
                        {
                            string key = null;
                            string ring = null;
                            foreach (var data in DoorData.Values)
                            {
                                if (data.KeyItem == obj.ItemId || data.KeyRingItem == obj.ItemId)
                                {
                                    key = data.KeyItem;
                                    ring = data.KeyRingItem;
                                }
                            }
                            if (key == null || ring == null)
                                return;
                            if (!obj.modData.TryGetValue(guidKey, out var guid))
                            {
                                return;
                            }
                            var split = guid.Split('|');
                            if (Config.CopyButton.JustPressed() && key == obj.ItemId)
                            {
                                var split2 = split[0].Split("=");

                                Object newItem = new Object(key, 1);
                                newItem.modData[guidKey] = guid;
                                newItem.modData[nameKey] = split2[1];
                                Item returned = Utility.addItemToThisInventoryList(newItem, __instance.actualInventory);
                                if (returned != null)
                                {
                                    TryReturnObject(returned, Game1.player);
                                }
                                Game1.playSound("bigSelect");
                                foreach (var k in Config.CopyButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                            else if (Config.RenameButton.JustPressed() && key == obj.ItemId)
                            {
                                var split2 = split[0].Split("=");

                                Game1.activeClickableMenu = new KeyNamingMenu(obj, split2[0], split2[1]);

                                foreach (var k in Config.RenameButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                            else if (Config.CombineButton.JustPressed())
                            {

                                if (Game1.player.CursorSlotItem is null && ring == obj.ItemId)
                                {
                                    foreach (var item in split)
                                    {
                                        var split2 = item.Split("=");
                                        Object newItem = new Object(key, 1);
                                        newItem.modData[guidKey] = $"{split2[0]}={split2[1]}";
                                        newItem.modData[nameKey] = split2[1];
                                        Item returned = Utility.addItemToThisInventoryList(newItem, __instance.actualInventory);
                                        if (returned != null)
                                        {
                                            TryReturnObject(returned, Game1.player);
                                        }
                                    }
                                    __instance.actualInventory[i] = null;
                                }
                                else if (Game1.player.CursorSlotItem is Object obj2 && (obj2.ItemId == key || obj2.ItemId == ring))
                                {
                                    var newObj = CombineKeys(obj, obj2, key, ring);
                                    if (newObj is not null)
                                    {
                                        __instance.actualInventory[i] = newObj;
                                        Game1.player.CursorSlotItem = null;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    return;
                                }
                                Game1.playSound("purchase");
                                foreach (var k in Config.CombineButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                        }
                        return;
                    }
                }

            }

        }

    }
}