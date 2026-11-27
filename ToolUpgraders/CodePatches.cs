using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.GameData.Tools;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ToolUpgraders
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.dayupdate))]
        public static class Farmer_dayupdate_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                if (!Config.ModEnabled || !TryGetUpgrades(__instance, out var dict))
                    return;
                
                foreach (var key in dict.Keys.ToArray())
                {
                    dict[key]--;
                }
                SetUpgrades(__instance, dict);
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.showToolUpgradeAvailability))]
        public static class Farmer_showToolUpgradeAvailability_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                if (!Config.ModEnabled || !TryGetUpgrades(__instance, out var dict))
                    return;
                
                foreach (var key in dict.Keys.ToArray())
                {
                    if (dict[key] == 0)
                    {
                        var tool = ItemRegistry.Create<Tool>("(T)" + key);
                        Game1.showGlobalMessage(GetToolReadyString(tool));
                    }
                }
                SetUpgrades(__instance, dict);
            }
        }
        
        [HarmonyPatch(typeof(Tool), nameof(Tool.CanBuyItem))]
        public static class Tool_CanBuyItem_Patch
        {
            public static bool Prefix(Tool __instance, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !TryGetUpgrader(__instance.ItemId, out var data))
                    return true;
                if (TryGetUpgrades(who, out var dict))
                {
                    var id = dict.Keys.FirstOrDefault(k => data.Tools.Contains(k));
                    if (id is not null)
                    {
                        Game1.showRedMessage(string.Format(SHelper.Translation.Get("busy-x"), ItemRegistry.Create("(T)"+id).DisplayName));
                        __result = false;
                    }
                    else
                    {
                        __result = true;
                    }
                }
                else
                {
                    var method = typeof(Item).GetMethod("CanBuyItem");
                    var ftn = method.MethodHandle.GetFunctionPointer();
                    var func = (Func<Farmer, bool>)Activator.CreateInstance(typeof(Func<Farmer, bool>), __instance, ftn);
                    __result = func(who);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.actionWhenPurchased))]
        public static class Tool_actionWhenPurchased_Patch
        {
            public static bool Prefix(Tool __instance, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                var data = UpgraderDict.Values.FirstOrDefault(d => d.Tools.Contains(__instance.ItemId));
                if (data == null)
                    return true;
                ToolUpgradeData toolUpgradeData = ShopBuilder.GetToolUpgradeData(__instance.GetToolData(), Game1.player);
                string previousToolId = ((toolUpgradeData != null) ? toolUpgradeData.RequireToolId : null);
                if (previousToolId != null)
                {
                    Item oldItem = Game1.player.Items.GetById(previousToolId).FirstOrDefault<Item>();
                    Game1.player.removeItemFromInventory(oldItem);
                    Tool oldTool = oldItem as Tool;
                    if (oldTool != null)
                    {
                        __instance.UpgradeFrom(oldTool);
                    }
                }
                TryGetUpgrades(Game1.player, out var dict);
                dict[__instance.ItemId] = data.UpgradeDays;
                SetUpgrades(Game1.player, dict);
                Game1.playSound(data.BeginSound ?? "parry", null);
                Game1.exitActiveMenu();
                Game1.DrawDialogue(new Dialogue(data.BeginNPC == null ? null : Game1.getCharacterFromName(data.BeginNPC, true, false), "ToolUpgrader", data.BeginText.Contains("{0}") ? string.Format(data.BeginText, data.UpgradeDays) : data.BeginText));
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(ItemQueryResolver.DefaultResolvers), nameof(ItemQueryResolver.DefaultResolvers.TOOL_UPGRADES))]
        public static class DefaultResolvers_TOOL_UPGRADES_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ItemQueryResolver.DefaultResolvers.TOOL_UPGRADES");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand is string s && s == "(T)" && codes[i + 2].opcode == OpCodes.Brfalse_S)
                    {
                        SMonitor.Log($"reversing tool check logic");
                        codes[i + 2].opcode = OpCodes.Brtrue_S;
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(xTile.Dimensions.Location) })]
        public static class GameLocation_performAction_Patch
        {
            public static bool Prefix(string[] action, Farmer who)
            {
                if (!Config.ModEnabled)
                    return true;
                if (!ArgUtility.TryGet(action, 0, out var actionType, out _, true, "string actionType"))
                {
                    return true;
                }
                var data = UpgraderDict.Values.FirstOrDefault(d => d.TileAction == actionType);
                if (data == null || !TryGetUpgrades(who, out var dict))
                    return true;
                foreach(var key in dict.Where(p => p.Value <= 0 && data.Tools.Contains(p.Key)).Select(p => p.Key).ToArray())
                {
                    if(TryReturnUpgradedTool(data, key))
                    {
                        dict.Remove(key);
                        SetUpgrades(who, dict);
                    }
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.TryOpenShopMenu), new Type[] { typeof(string), typeof(string), typeof(bool) })]
        public static class Utility_TryOpenShopMenu_Patch
        {
            public static bool Prefix(string shopId)
            {
                if (!Config.ModEnabled)
                    return true;
                var data = UpgraderDict.Values.FirstOrDefault(d => d.ShopName == shopId);
                if (data == null || !TryGetUpgrades(Game1.player, out var dict))
                    return true;
                foreach (var kvp in dict.Where(p => p.Value <= 0 && data.Tools.Contains(p.Key)))
                {
                    if(TryReturnUpgradedTool(data, kvp.Key))
                    {
                        dict.Remove(kvp.Key);
                        SetUpgrades(Game1.player, dict);
                    }
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.TryOpenShopMenu), new Type[] { typeof(string), typeof(GameLocation), typeof(Rectangle?), typeof(int?), typeof(bool), typeof(bool), typeof(Action<string>) })]
        public static class Utility_TryOpenShopMenu_Patch2
        {
            public static bool Prefix(string shopId)
            {
                if (!Config.ModEnabled)
                    return true;
                var data = UpgraderDict.Values.FirstOrDefault(d => d.ShopName == shopId);
                if (data == null || !TryGetUpgrades(Game1.player, out var dict))
                    return true;
                foreach (var kvp in dict.Where(p => p.Value <= 0 && data.Tools.Contains(p.Key)))
                {
                    if(TryReturnUpgradedTool(data, kvp.Key))
                    {
                        dict.Remove(kvp.Key);
                        SetUpgrades(Game1.player, dict);
                    }
                    return false;
                }
                return true;
            }
        }
    }
}