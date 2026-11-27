using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
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

namespace LightSwitches
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.checkForAction))]
        public static class Furniture_checkForAction_Patch
        {
            public static bool Prefix(Furniture __instance, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || !LightSwitches.TryGetValue(__instance.ItemId, out var data))
                {
                    return true;
                }
                ToggleSwitch(__instance, data);
                return false;
            }

        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.placementAction))]
        public static class Furniture_placementAction_Patch
        {
            public static void Postfix(Furniture __instance, GameLocation location, bool __result)
            {
                if (!Config.ModEnabled || !__result || !LightSwitches.TryGetValue(__instance.ItemId, out var data) || !__instance.modData.TryGetValue(onOffKey, out var str) || str != "on")
                {
                    return;
                }
                __instance.modData[onOffKey] = "off";
                ToggleSwitch(__instance, data);
            }

        }
        [HarmonyPatch(typeof(GameLocation), "removeQueuedFurniture")]
        public static class GameLocation_removeQueuedFurniture_Patch
        {
            public static void Prefix(GameLocation __instance, Guid guid)
            {
                if (!Config.ModEnabled || !__instance.furniture.TryGetValue(guid, out var f) || !Game1.player.couldInventoryAcceptThisItem(f) || !LightSwitches.TryGetValue(f.ItemId, out var data) || !f.modData.TryGetValue(onOffKey, out var str) || str != "on")
                {
                    return;
                }
                __instance.modData[onOffKey] = "off";
            }

        }
        
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float), })]
        public static class Furniture_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling Furniture.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 4; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(ParsedItemData), nameof(ParsedItemData.GetTexture)))
                    {
                        SMonitor.Log("Intercepting texture");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetTextureForTranspiler))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    //if (false && codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)) && codes[i + 2].opcode == OpCodes.Call && codes[i + 2].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Color), "op_Multiply", new Type[] { typeof(Color), typeof(float) }))
                    //{
                    //    SMonitor.Log("Intercepting color");
                    //    codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetColorForTranspiler))));
                    //    codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
                    //    i += 4;
                    //}
                }
                return codes.AsEnumerable();
            }

        }
        
        [HarmonyPatch(typeof(GameLocation), "_updateAmbientLighting")]
        public static class GameLocation_updateAmbientLighting_Patch
        {
            public static bool Prefix(FarmHouse __instance, NetFloat ___lightLevel)
            {
                return TrySetAmbientLight(__instance, ___lightLevel.Value);
            }
        }
        [HarmonyPatch(typeof(FarmHouse), "_updateAmbientLighting")]
        public static class FarmHouse_updateAmbientLighting_Patch
        {
            public static bool Prefix(FarmHouse __instance, NetFloat ___lightLevel)
            {
                return TrySetAmbientLight(__instance, ___lightLevel.Value);
            }
        }
        
        [HarmonyPatch(typeof(IslandFarmHouse), "_updateAmbientLighting")]
        public static class IslandFarmHouse_updateAmbientLighting_Patch
        {
            public static bool Prefix(FarmHouse __instance, NetFloat ___lightLevel)
            {
                return TrySetAmbientLight(__instance, ___lightLevel.Value);
            }
        }
    }
}