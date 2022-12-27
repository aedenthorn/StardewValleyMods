using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace OmniTools
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(WateringCan), nameof(WateringCan.drawInMenu))]
        public class WateringCan_drawInMenu_Patch
        {
            public static void Postfix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !Config.ShowNumber || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                var count = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Count + 1;
                Utility.drawTinyDigits(count, spriteBatch, location + new Vector2(4, 0), 3f * scaleSize, 1f, Config.NumberColor);
            }
        }
        [HarmonyPatch(typeof(Slingshot), nameof(Slingshot.drawInMenu))]
        public class Slingshot_drawInMenu_Patch
        {
            public static void Postfix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !Config.ShowNumber || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                var count = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Count + 1;
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
                var count = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Count + 1;
                Utility.drawTinyDigits(count, spriteBatch, location + new Vector2(4, 0), 3f * scaleSize, 1f, Config.NumberColor);
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static void Postfix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !Config.ShowNumber || __instance is WateringCan || !__instance.modData.TryGetValue(toolsKey, out string toolsString))
                    return;
                var count = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Count + 1;
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
                if (!Config.EnableMod || __instance is not InventoryPage || (key != (Keys)Config.CycleButton && key != (Keys)Config.RemoveButton))
                    return;
                var inv = (__instance as InventoryPage).inventory;
                var mouse = Game1.getMousePosition();
                if(key == (Keys)Config.CycleButton)
                {
                    foreach (ClickableComponent c in inv.inventory)
                    {
                        if (c.containsPoint(mouse.X, mouse.Y))
                        {
                            int slotNumber = Convert.ToInt32(c.name);
                            if (slotNumber >= inv.actualInventory.Count || inv.actualInventory[slotNumber] is null || !inv.actualInventory[slotNumber].modData.TryGetValue(toolsKey, out string toolsString))
                                return;
                            inv.actualInventory[slotNumber] = CycleTool(inv.actualInventory[slotNumber] as Tool, toolsString);
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
                            if (slotNumber >= inv.actualInventory.Count || inv.actualInventory[slotNumber] is null || !inv.actualInventory[slotNumber].modData.TryGetValue(toolsKey, out string toolsString))
                                return;
                            inv.actualInventory[slotNumber] = RemoveTool(inv.actualInventory[slotNumber] as Tool, toolsString);
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
                if (!Config.EnableMod || !__instance.IsLocalPlayer || __instance.CurrentTool?.modData.TryGetValue(toolsKey, out string toolsString) != true)
                    return;
                List<ToolInfo> tools = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString);
                var tile = __instance.GetToolLocation(false) / 64;
                tile = new Vector2((int)tile.X, (int)tile.Y);
                if (Config.SwitchForObjects && __instance.currentLocation.objects.TryGetValue(tile, out Object obj))
                {
                    if (obj.Name.Equals("Stone") && SwitchTool(__instance, typeof(Pickaxe), tools))
                    {
                        return;
                    }
                    else if (obj.Name.Contains("Twig") && SwitchTool(__instance, typeof(Axe), tools))
                    {
                        return;
                    }
                    else if (obj.ParentSheetIndex == 590 && SwitchTool(__instance, typeof(Hoe), tools))
                    {
                        return;
                    }
                }
                if (__instance.currentLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature tf))
                {
                    if (Config.SwitchForTrees && tf is Tree && SwitchTool(__instance, typeof(Axe), tools))
                    {
                        return;
                    }
                    else if (Config.SwitchForGrass && tf is Grass && SwitchTool(__instance, null, tools))
                    {
                        return;
                    }
                }
                if (Config.SwitchForResourceClumps)
                {
                    Rectangle tileRect = new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);

                    foreach (ResourceClump stump in __instance.currentLocation.resourceClumps)
                    { 
                        var bb = stump.getBoundingBox(stump.tile.Value);
                        if (bb.Intersects(tileRect))
                        {
                            if ((stump.parentSheetIndex.Value == 600 || stump.parentSheetIndex.Value == 602) && SwitchTool(__instance, typeof(Axe), tools))
                            {
                                return;
                            }
                            else if (new int[] { 622, 672, 752, 754, 756, 758 }.Contains(stump.parentSheetIndex.Value) && SwitchTool(__instance, typeof(Pickaxe), tools))
                            {
                                return;
                            }
                        }
                    }
                }
                if (Config.SwitchForPan)
                {
                    __instance.currentLocation.orePanPoint.Value = new Point(49, 24);
                    Rectangle orePanRect = new Rectangle(__instance.currentLocation.orePanPoint.X * 64 - 64, __instance.currentLocation.orePanPoint.Y * 64 - 64, 256, 256);
                    if (orePanRect.Contains((int)tile.X * 64, (int)tile.Y * 64) && Utility.distance((float)__instance.getStandingX(), (float)orePanRect.Center.X, (float)__instance.getStandingY(), (float)orePanRect.Center.Y) <= 192f && SwitchTool(__instance, typeof(Pan), tools))
                    {
                        return;
                    }
                }
                if (Config.SwitchForMonsters)
                {
                    foreach (var c in __instance.currentLocation.characters)
                    {
                        if (c.GetBoundingBox().Intersects(new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64)) && c is Monster && SwitchTool(__instance, typeof(MeleeWeapon), tools))
                        {
                            return;
                        }
                    }
                }
                if (Config.SwitchForAnimals)
                {
                    FarmAnimal[] animals = new FarmAnimal[0];
                    if (__instance.currentLocation is Farm)
                    {
                        animals = (__instance.currentLocation as Farm).animals.Values.ToArray();
                    }
                    else if (__instance.currentLocation is AnimalHouse)
                    {
                        animals = (__instance.currentLocation as AnimalHouse).animals.Values.ToArray();
                    }
                    foreach (var c in animals)
                    {
                        Rectangle r = new Rectangle((int)tile.X * 64 - 32, (int)tile.Y * 64 - 32, 64, 64);
                        if (c.GetHarvestBoundingBox().Intersects(r))
                        {
                            //SMonitor.Log($"harvesting {c.Name}; age {c.age.Value}/{c.ageWhenMature.Value}; produce {c.currentProduce.Value} ({c.defaultProduceIndex.Value}); tool {Game1.player.CurrentTool?.Name} ({c.toolUsedForHarvest.Value} - {toolsString})");
                            if (c.toolUsedForHarvest.Value.Equals("Shears") && SwitchTool(__instance, typeof(Shears), tools))
                            {
                                return;
                            }
                            if (c.toolUsedForHarvest.Value.Equals("Milk Pail") && SwitchTool(__instance, typeof(MilkPail), tools))
                            {
                                return;
                            }
                            if (c.currentProduce.Value > 0 && c.age.Value >= (int)c.ageWhenMature.Value)
                            {

                            }
                        }
                    }

                }

                if (Config.SwitchForWateringCan && __instance.currentLocation.CanRefillWateringCanOnTile((int)tile.X, (int)tile.Y) && SwitchTool(__instance, typeof(WateringCan), tools))
                {
                    return;
                }
                if (Config.SwitchForFishing && __instance.currentLocation.waterTiles is not null && __instance.currentLocation.waterTiles[(int)tile.X, (int)tile.Y] && SwitchTool(__instance, typeof(FishingRod), tools))
                {
                    return;
                }
            }
        }
    }
}