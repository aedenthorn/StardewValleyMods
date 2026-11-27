using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static StardewValley.Menus.CharacterCustomization;
using Object = StardewValley.Object;

namespace FlowerColors
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Crop), nameof(Crop.draw))]
        public static class Crop_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)) && codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand is MethodInfo mi2 && mi2.Name == "Equals")
                    {
                        SMonitor.Log($"preventing skip of white colored crops");
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(PreventSkipWhite));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

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

        [HarmonyPatch(typeof(Object), nameof(Object.DisplayName))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Object_DisplayName_Patch
        {
            public static void Postfix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled || !Config.AppendCombinedNumberToName || __instance is not ColoredObject co || co.Category != Object.flowersCategory || !co.modData.TryGetValue(colorsKey, out var str))
                {
                    return;
                }
                int count = 1;
                foreach (char c in str)
                    if (c == '|') count++;
                if(count > 1)
                    __result += $" ({count})";
            }
        }

        [HarmonyPatch(typeof(ColoredObject), nameof(ColoredObject.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) })]
        public static class ColoredObject_drawInMenu_Patch
        {
            public static void Prefix(ColoredObject __instance, Vector2 location, float scaleSize)
            {
                if (!Config.ModEnabled || __instance.Category != Object.flowersCategory || (!__instance.modData.TryGetValue(colorsKey, out var cs)))
                {
                    return;
                }
                if (Config.HoverToShowCombined && !new Rectangle((int)location.X, (int)location.Y, (int)(scaleSize * 64), (int)(scaleSize * 64)).Contains(Game1.getMousePosition(true)))
                    return;
                var strs = cs.Split('|');

                float interval = 1500f;
                int i = ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / interval)) % strs.Length;
                var split = strs[i].Split('=');
                var color = Utility.StringToColor(split[0]) ?? Color.Transparent;
                if(color != Color.Transparent)
                {
                    __instance.color.Value = color;
                }
            }
        }

        [HarmonyPatch(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList))]
        public static class ItemGrabMenu_organizeItemsInList_Patch
        {
            public static void Prefix(IList<Item> items)
            {
                if (!Config.ModEnabled || !Config.CombineOnOrganize)
                {
                    return;
                }
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if(items[i] is not ColoredObject co || co.Category != Object.flowersCategory || co.modData.ContainsKey(prismaticKey))
                    {
                        continue;
                    }
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (items[j] is not ColoredObject co2 || co2.ItemId != co.ItemId || co.Quality != co2.Quality || co2.modData.ContainsKey(prismaticKey))
                        {
                            continue;
                        }
                        CombineFlowers(co, co2);
                        items[j] = null;
                    }
                }
            }

        }
            
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || (lastScrollDelta.Value == 0 && !Config.CopyButton.JustPressed() && !Config.PasteButton.JustPressed() && !Config.PickButton.JustPressed() && (!Config.CombineButton.JustPressed())))
                {
                    return;
                }
                var mousePos = Game1.getMousePosition(true);
                for(int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if(cc.containsPoint(mousePos.X, mousePos.Y))
                    {
                        if(__instance.actualInventory.Count > i && __instance.actualInventory[i] is ColoredObject co && co.Category == Object.flowersCategory)
                        {
                            var data = Game1.cropData.Values.FirstOrDefault(d => d.HarvestItemId == co.ItemId);
                            if (data == null)
                                return;
                            var combined = co.modData.TryGetValue(colorsKey, out var cs);
                            var isPrism = co.modData.TryGetValue(prismaticKey, out var prismatic);
                            if (lastScrollDelta.Value != 0)
                            {
                                if(combined || (Config.ScrollModKey != SButton.None && !SHelper.Input.IsDown(Config.ScrollModKey)))
                                {
                                    lastScrollDelta.Value = 0;
                                    return;
                                }
                                if (SHelper.Input.IsDown(Config.PrismaticModKey))
                                {
                                    if (isPrism)
                                    {
                                        co.modData[prismaticKeyDisabled] = prismatic;
                                        co.modData.Remove(prismaticKey);
                                        Game1.playSound("yoba");
                                        SHelper.Input.SuppressScrollWheel();
                                    }
                                    else if (co.modData.TryGetValue(prismaticKeyDisabled, out var prismatic2))
                                    {
                                        co.modData[prismaticKey] = prismatic2;
                                        co.modData.Remove(prismaticKeyDisabled);
                                        Game1.playSound("yoba");
                                        SHelper.Input.SuppressScrollWheel();
                                    }
                                    else if (prismaticFlowersAPI?.MakePrismatic(co) == true)
                                    {
                                        Game1.playSound("yoba");
                                        SHelper.Input.SuppressScrollWheel();
                                    }
                                }
                                else if(!isPrism)
                                {
                                    var newColor = GetNewColor(data.TintColors, co.color.Value, lastScrollDelta.Value);
                                    if (newColor is not null)
                                        co.color.Value = newColor.Value;
                                    Game1.playSound("shiny4");
                                    SHelper.Input.SuppressScrollWheel();
                                }
                                else
                                {
                                    return;
                                }
                                lastScrollDelta.Value = 0;
                            }
                            else if (!combined && Config.CopyButton.JustPressed())
                            {
                                if (co.modData.TryGetValue(prismaticKey, out var str))
                                {
                                    pastePrismatic.Value = str;
                                    co.modData[prismaticKey] = str;
                                }
                                else
                                {
                                    pastePrismatic.Value = null;
                                }
                                pasteColor.Value = co.color.Value;
                                Game1.hudMessages.Clear();
                                Game1.player.ShowItemReceivedHudMessage(co, 1);
                                Game1.playSound("bigSelect");
                                foreach (var k in Config.CopyButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        if (!new SButton[] { SButton.LeftControl, SButton.LeftShift, SButton.LeftAlt, SButton.RightShift, SButton.RightControl, SButton.RightAlt, }.Contains(b))
                                            SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                            else if (!combined && Config.PasteButton.JustPressed())
                            {
                                if (pasteColor.Value == Color.Transparent)
                                    return;
                                if (pastePrismatic.Value != null)
                                {
                                    co.modData[prismaticKey] = pastePrismatic.Value;
                                }
                                else
                                {
                                    co.modData.Remove(prismaticKey);
                                    co.color.Value = pasteColor.Value;
                                }
                                Game1.playSound("grassyStep");
                                foreach (var k in Config.PasteButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        if (!new SButton[] { SButton.LeftControl, SButton.LeftShift, SButton.LeftAlt, SButton.RightShift, SButton.RightControl, SButton.RightAlt, }.Contains(b))
                                            SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                            else if (!combined && Config.PickButton.JustPressed())
                            {
                                Game1.activeClickableMenu = new ColorPickMenu(co, Game1.activeClickableMenu);
                                Game1.playSound("bigSelect");
                                foreach (var k in Config.PickButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                            else if (Config.CombineButton.JustPressed() && !isPrism)
                            {

                                if (Game1.player.CursorSlotItem is null)
                                {
                                    Dictionary<Color, int> colors = new();
                                    if (combined)
                                    {
                                        var strs = cs.Split('|');
                                        foreach (var str in strs)
                                        {
                                            var split = str.Split('=');
                                            colors.Add(Utility.StringToColor(split[0]) ?? Color.Transparent, int.Parse(split[1]));
                                        }

                                    }
                                    else
                                    {
                                        colors[co.color.Value] = co.Stack;
                                    }
                                    if (colors.Count < 2)
                                        return;
                                    __instance.actualInventory[i] = null;
                                    foreach (var kvp in colors)
                                    {
                                        if (kvp.Key == Color.Transparent)
                                            continue;
                                        ColoredObject newItem = new ColoredObject(co.ItemId, kvp.Value, kvp.Key)
                                        {
                                            Quality = co.Quality
                                        };
                                        foreach(var p in co.modData.Pairs)
                                        {
                                            if (p.Key != colorsKey)
                                                newItem.modData[p.Key] = p.Value;
                                        }
                                        Item returned = Utility.addItemToThisInventoryList(newItem, __instance.actualInventory);
                                        if(returned != null)
                                        {
                                            TryReturnObject(returned, Game1.player);
                                        }
                                    }
                                }
                                else if (Game1.player.CursorSlotItem is ColoredObject co2 && co2.ItemId == co.ItemId && co.Quality == co2.Quality && !co2.modData.ContainsKey(prismaticKey))
                                {
                                    CombineFlowers(co, co2);
                                    Game1.player.CursorSlotItem = null;
                                }
                                else
                                {
                                    return;
                                }
                                Game1.playSound("grassyStep");
                                foreach (var k in Config.CombineButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        SHelper.Input.Suppress(b);
                                    }
                                }
                                return;
                            }
                        }
                        return;
                    }
                }

            }

        }

    }
}