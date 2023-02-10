using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OmniTools
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Item), nameof(Item.canBeTrashed))]
        public class Item_canBeTrashed_Patch
        {
            public static void Postfix(Item __instance, ref bool __result)
            {
                if (!Config.EnableMod || !__result || !__instance.modData.ContainsKey(toolsKey))
                    return;
                __result = false;
            }
        }
        [HarmonyPatch(typeof(Slingshot), nameof(Slingshot.drawInMenu))]
        public class Slingshot_drawInMenu_Patch
        {
            public static void Postfix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !Config.ShowNumber || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                if(!__instance.modData.TryGetValue(toolCountKey, out string countString))
                {
                    countString = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Count.ToString();
                    __instance.modData[toolCountKey] = countString;
                }
                var count = int.Parse(countString) + 1;
                Utility.drawTinyDigits(count, spriteBatch, location + new Vector2(4, 0), 3f * scaleSize, 1f, Config.NumberColor);
            }
        }
        [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.drawInMenu))]
        public class MeleeWeapon_drawInMenu_Patch
        {
            public static void Postfix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !Config.ShowNumber || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                if (!__instance.modData.TryGetValue(toolCountKey, out string countString))
                {
                    countString = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Count.ToString();
                    __instance.modData[toolCountKey] = countString;
                }
                var count = int.Parse(countString) + 1;
                Utility.drawTinyDigits(count, spriteBatch, location + new Vector2(4, 0), 3f * scaleSize, 1f, Config.NumberColor);
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static void Postfix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !Config.ShowNumber || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                if (!__instance.modData.TryGetValue(toolCountKey, out string countString))
                {
                    countString = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Count.ToString();
                    __instance.modData[toolCountKey] = countString;
                }
                var count = int.Parse(countString) + 1;
                Utility.drawTinyDigits(count, spriteBatch, location + new Vector2(4, 0), 3f * scaleSize, 1f, Config.NumberColor);
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.DisplayName))]
        [HarmonyPatch(MethodType.Getter)]
        public class Tool_DisplayName_Patch
        {
            public static void Postfix(Tool __instance, ref string __result)
            {
                if (!Config.EnableMod || skip || !SHelper.Input.IsDown(Config.ModButton) || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                var list = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Select(t => t.displayName);
                __result += $" ({string.Join(", ", list)})";
            }
        }
        [HarmonyPatch(typeof(FishingRod), nameof(FishingRod.DisplayName))]
        [HarmonyPatch(MethodType.Getter)]
        public class FishingRod_DisplayName_Patch
        {
            public static void Postfix(FishingRod __instance, ref string __result)
            {
                if (!Config.EnableMod || skip || !SHelper.Input.IsDown(Config.ModButton) || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                var list = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Select(t => t.displayName);
                __result += $" ({string.Join(", ", list)})";
            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.receiveKeyPress))]
        public class IClickableMenu_receiveKeyPress_Patch
        {
            public static void Postfix(IClickableMenu __instance, Keys key)
            {
                if (!Config.EnableMod || (__instance is not InventoryPage && __instance is not MenuWithInventory) || (key != (Keys)Config.CycleButton && key != (Keys)Config.RemoveButton))
                    return;
                var inv = __instance is InventoryPage ? (__instance as InventoryPage).inventory : (__instance as MenuWithInventory).inventory;
                var mouse = Game1.getMousePosition();
                if(key == (Keys)Config.CycleButton)
                {
                    foreach (ClickableComponent c in inv.inventory)
                    {
                        if (c.containsPoint(mouse.X, mouse.Y))
                        {
                            int slotNumber = Convert.ToInt32(c.name);
                            if (slotNumber >= inv.actualInventory.Count || inv.actualInventory[slotNumber] is not Tool || !inv.actualInventory[slotNumber].modData.TryGetValue(toolsKey, out string toolsString))
                                return;
                            var newTool = CycleTool(inv.actualInventory[slotNumber] as Tool, toolsString);
                            if(slotNumber == Game1.player.CurrentToolIndex)
                                UpdateEnchantments(Game1.player, inv.actualInventory[slotNumber] as Tool, newTool);
                            inv.actualInventory[slotNumber] = newTool;
                            return;
                        }
                    }
                }
                else if(key == (Keys)Config.RemoveButton)
                {
                    foreach (ClickableComponent c in inv.inventory)
                    {
                        if (c.containsPoint(mouse.X, mouse.Y))
                        {
                            int slotNumber = Convert.ToInt32(c.name);
                            if (slotNumber >= inv.actualInventory.Count || inv.actualInventory[slotNumber] is not Tool || !inv.actualInventory[slotNumber].modData.TryGetValue(toolsKey, out string toolsString))
                                return;
                            var newTool = RemoveTool(inv.actualInventory[slotNumber] as Tool, toolsString);
                            if (slotNumber == Game1.player.CurrentToolIndex)
                                UpdateEnchantments(Game1.player, inv.actualInventory[slotNumber] as Tool, newTool);
                            inv.actualInventory[slotNumber] = newTool;
                            return;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.leftClick))]
        public class InventoryMenu_leftClick_Patch
        {
            public static bool Prefix(InventoryMenu __instance, int x, int y, Item toPlace, ref Item __result)
            {
                if (!Config.EnableMod || !SHelper.Input.IsDown(Config.ModButton) || toPlace  is null || toPlace.modData.ContainsKey(toolsKey) || !toolList.Contains(toPlace.GetType()) || !__instance.isWithinBounds(x, y))
                    return true;
                var td = GetDescriptionFromTool(toPlace as Tool);
                if (td is null)
                    return true;
                foreach (ClickableComponent c in __instance.inventory)
                {
                    if (c.containsPoint(x, y))
                    {
                        int slotNumber = Convert.ToInt32(c.name);
                        if (slotNumber >= __instance.actualInventory.Count || __instance.actualInventory[slotNumber] is null || !toolList.Contains(__instance.actualInventory[slotNumber].GetType()))
                            return true;
                        if (__instance.actualInventory[slotNumber].GetType().Equals(toPlace.GetType()))
                        {
                            if(toPlace is not MeleeWeapon || ((toPlace as MeleeWeapon).isScythe(toPlace.ParentSheetIndex) == (__instance.actualInventory[slotNumber] as MeleeWeapon).isScythe(__instance.actualInventory[slotNumber].ParentSheetIndex)))
                            {
                                return true;
                            }
                        }
                        List<ToolInfo> list = new List<ToolInfo>();
                        if(__instance.actualInventory[slotNumber].modData.TryGetValue(toolsKey, out string toolsString))
                        {
                            list = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString);
                            for(int i = 0; i < list.Count; i++)
                            {
                                Tool t = GetToolFromInfo(list[i]);
                                if (t.GetType().Equals(toPlace.GetType()) && (toPlace is not MeleeWeapon || ((toPlace as MeleeWeapon).isScythe(toPlace.ParentSheetIndex) == (t as MeleeWeapon).isScythe(t.ParentSheetIndex))))
                                {
                                    list.RemoveAt(i);
                                    SMonitor.Log($"Removing {t.Name} from {__instance.actualInventory[slotNumber].Name}");
                                    __result = t;
                                    break;
                                }
                            }
                        }
                        SMonitor.Log($"Adding {toPlace.Name} to {__instance.actualInventory[slotNumber].Name}");
                        Game1.playSound(GetToolSound(toPlace as Tool));
                        list.Add(new ToolInfo(toPlace as Tool));
                        __instance.actualInventory[slotNumber].modData[toolsKey] = JsonConvert.SerializeObject(list);
                        __instance.actualInventory[slotNumber].modData[toolCountKey] = list.Count + "";
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Farmer), "performBeginUsingTool")]
        public class Farmer_performBeginUsingTool_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                if (!Config.EnableMod || !Config.SmartSwitch || !__instance.IsLocalPlayer || __instance.CurrentTool?.modData.TryGetValue(toolsKey, out string toolsString) != true)
                    return;
                List<ToolInfo> tools = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString);
                var tile = __instance.GetToolLocation(false) / 64;
                tile = new Vector2((int)tile.X, (int)tile.Y);
                Tool t = SmartSwitch(__instance.CurrentTool, __instance.currentLocation, tile, tools);
                if (t is not null)
                {
                    UpdateEnchantments(__instance, __instance.CurrentTool, t);
                    __instance.CurrentTool = t;
                }
            }
        }

        [HarmonyPatch(typeof(Game1), "checkIsMissingTool")]
        public class Game1_checkIsMissingTool_Patch
        {
            public static void Postfix(Dictionary<Type, int> missingTools, ref int missingScythes, Item item)
            {
                if (!Config.EnableMod || item is not Tool || !item.modData.ContainsKey(toolsKey))
                    return;
                var tools = GetToolsFromTool(item as Tool);
                foreach(var tool in tools)
                {
                    for (int i = 0; i < missingTools.Count; i++)
                    {
                        if (tool.GetType() == missingTools.ElementAt(i).Key)
                        {
                            Type key = missingTools.ElementAt(i).Key;
                            int num = missingTools[key];
                            missingTools[key] = num - 1;
                        }
                    }
                    if (tool is MeleeWeapon && (tool as MeleeWeapon).Name.Equals("Scythe"))
                    {
                        missingScythes--;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.removeItemFromInventory), new Type[] { typeof(Item) })]
        public class Farmer_removeItemFromInventory_Patch
        {
            public static void Prefix(Farmer __instance, Item which)
            {
                if (!Config.EnableMod || which is not Tool || !which.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                var tools = GetToolInfosFromString(toolsString);
                foreach (var tool in tools)
                {
                    Tool t = GetToolFromInfo(tool);
                    if (t is not null)
                    {
                        if (!__instance.addItemToInventoryBool(t))
                        {
                            Game1.createItemDebris(t, __instance.getStandingPosition(), __instance.FacingDirection, __instance.currentLocation, -1);
                        }
                    }
                }
                which.modData.Remove(toolsKey);
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.performUseAction))]
        public class HoeDirt_performUseAction_Patch
        {
            public static void Prefix(HoeDirt __instance)
            {
                if (!Config.EnableMod || !Config.SwitchForCrops || Game1.player.CurrentTool is not Tool || !Game1.player.CurrentTool.modData.ContainsKey(toolsKey))
                    return;
                Tool t = SwitchForTerrainFeature(Game1.player.CurrentTool, __instance);
                if (t is not null)
                {
                    foreach (var e in Game1.player.CurrentTool.enchantments)
                    {
                        e.OnUnequip(Game1.player);
                    }
                    Game1.player.CurrentTool = t;
                    foreach (var e in t.enchantments)
                    {
                        e.ApplyTo(t, Game1.player);
                    }
                }
            }
        }
    }
}