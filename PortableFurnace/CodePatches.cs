using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System;
using Object = StardewValley.Object;

namespace PortableFurnace
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Item), "getDescriptionWidth")]
        public class Item_getDescriptionWidth_Patch
        {
            public static void Postfix(Item __instance, ref int __result)
            {
                if (!Config.EnableMod || !__instance.ItemId.StartsWith(SHelper.ModRegistry.ModID))
                    return;
                __result *= 2;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.getDescription))]
        public class Object_getDescription_Patch
        {
            public static void Postfix(Object __instance, ref string __result)
            {
                if (!Config.EnableMod || !__instance.ItemId.StartsWith(SHelper.ModRegistry.ModID))
                    return;
                __result += " ";
                if(__instance.modData.TryGetValue(itemKey, out var itemID))
                {
                    __result += string.Format(SHelper.Translation.Get("current-x-y"), ItemRegistry.GetDataOrErrorItem(itemID).DisplayName, __instance.modData[timeKey]);
                }
                else
                {
                    __result += SHelper.Translation.Get("empty");
                }
                if (__instance.modData.ContainsKey(repeatKey))
                {
                    __result += " " + SHelper.Translation.Get("repeat");
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.maximumStackSize))]
        public class Object_maximumStackSize_Patch
        {
            public static bool Prefix(Object __instance, ref int __result)
            {
                if (!Config.EnableMod || !__instance.ItemId.StartsWith(SHelper.ModRegistry.ModID))
                {
                    return true;
                }
                __result = 1;
                return false;
            }
        }
        public static int tick = 0;
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static void Prefix(Object __instance, ref float scaleSize)
            {
                if (!Config.EnableMod || !__instance.ItemId.StartsWith(SHelper.ModRegistry.ModID)) 
                {
                    return;
                }
                bool active = __instance.modData.ContainsKey(timeKey);
                if (active)
                {
                    var ms = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                    scaleSize += ((float)(float)Math.Sin(ms / 400) - 0.5f) * 0.03f;

                }
            }
        }
        [HarmonyPatch(typeof(Item), nameof(Item.ParentSheetIndex))]
        [HarmonyPatch(MethodType.Getter)]
        public class Item_ParentSheetIndex_Patch
        {
            public static void Postfix(Item __instance, ref int __result)
            {
                if (!Config.EnableMod || __result < 0 || __instance is not Object obj || !obj.ItemId.StartsWith(SHelper.ModRegistry.ModID) || !__instance.modData.ContainsKey(timeKey))
                    return;
                __result++;
            }
        }
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.hover))]
        public class InventoryMenu_hover_Patch
        {
            public static void Postfix(InventoryMenu __instance, Item __result)
            {
                if(!Config.EnableMod || __result == null) 
                    return;
                if (__result.Name.StartsWith(SHelper.ModRegistry.ModID))
                {
                    if(Config.ToggleButton.GetState() == StardewModdingAPI.SButtonState.Pressed)
                    {
                        if (!__result.modData.ContainsKey(repeatKey))
                        {
                            Game1.playSound("bigSelect");
                            __result.modData[repeatKey] = "true";
                        }
                        else
                        {
                            Game1.playSound("bigDeSelect");
                            __result.modData.Remove(repeatKey);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.rightClick))]
        public class InventoryMenu_rightClick_Patch
        {
            public static bool Prefix(InventoryMenu __instance, int x, int y, Item toAddTo, bool playSound, bool onlyCheckToolAttachments, ref Item __result)
            {
                if (!Config.EnableMod || onlyCheckToolAttachments || toAddTo == null)
                    return true;

                foreach (ClickableComponent clickableComponent in __instance.inventory)
                {
                    int slotNumber = Convert.ToInt32(clickableComponent.name);
                    Item slot = ((slotNumber < __instance.actualInventory.Count) ? __instance.actualInventory[slotNumber] : null);
                    if (clickableComponent.containsPoint(x, y) && slotNumber < __instance.actualInventory.Count)
                    {
                        if (slot?.ItemId.StartsWith(SHelper.ModRegistry.ModID) == true)
                        {
                            if (slot.modData.ContainsKey(itemKey) && slot.modData.ContainsKey(amountKey) && slot.modData.ContainsKey(timeKey))
                            {
                                continue;
                            }
                            var speed = GetFurnaceSpeed(slot);
                            if (speed > 0)
                            {
                                if(TryStartFurnace(slot, toAddTo.QualifiedItemId, toAddTo.Stack, speed, ref toAddTo))
                                {
                                    __result = toAddTo;
                                    return false;
                                }
                            }
                        }
                        return true;
                    }
                }
                return true;
            }
        }
    }
}