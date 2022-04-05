using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace MultiLevelLocations
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(NPC), nameof(NPC.draw))]
        public class NPC_draw_Patch
        {
            public static bool Prefix(NPC __instance)
            {
                return !Config.EnableMod || __instance.currentLocation != Game1.player.currentLocation || lastPlayerLevel == 1;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.HideShadow))]
        [HarmonyPatch(MethodType.Getter)]
        public class NPC_HideShadow_Getter_Patch
        {
            public static bool Prefix(NPC __instance, ref bool __result)
            {
                if (!Config.EnableMod || __instance.currentLocation != Game1.player.currentLocation || lastPlayerLevel == 1)
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
        public class NPC_checkAction_Patch
        {
            public static bool Prefix(NPC __instance, ref bool __result)
            {
                if (!Config.EnableMod || __instance.currentLocation != Game1.player.currentLocation || lastPlayerLevel == 1)
                    return true;
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.checkForCharacterInteractionAtTile))]
        public class Utility_checkForCharacterInteractionAtTile_Patch
        {
            public static bool Prefix()
            {
                return (!Config.EnableMod || lastPlayerLevel == 1);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) })]
        public class GameLocation_isCollidingPosition_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GameLocation.isCollidingPosition");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Character), nameof(Character.farmerPassesThrough)))
                    {
                        SMonitor.Log("overriding farmer passes through");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetWhetherPassesThrough))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static bool GetWhetherPassesThrough(bool through, GameLocation location)
        {
            if (!Config.EnableMod || location != Game1.player.currentLocation || lastPlayerLevel == 1)
                return through;
            return true;
        }
    }
}