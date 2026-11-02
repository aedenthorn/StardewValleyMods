using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SittingAction
{
    public partial class ModEntry
    {
        //[HarmonyPatch(typeof(Game1), nameof(Game1.pressActionButton))]
        public static class Game1_pressActionButton_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.withinRadiusOfPlayer))]
        public static class Utility_withinRadiusOfPlayer_Patch
        {
            public static void Prefix(ref int tileRadius, Farmer f)
            {
                if (Config.ModEnabled && f.IsSitting())
                {
                    tileRadius = Config.MaxRadius;
                }

            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.tileWithinRadiusOfPlayer))]
        public static class Utility_tileWithinRadiusOfPlayer_Patch
        {
            public static void Prefix(ref int tileRadius, Farmer f)
            {
                if (Config.ModEnabled && f.IsSitting())
                {
                    tileRadius = Config.MaxRadius;
                }

            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.GetGrabTile))]
        public static class Character_GetGrabTile_Patch
        {
            public static bool Prefix(Character __instance, ref Vector2 __result)
            {
                if (!Config.ModEnabled || __instance is not Farmer f || f != Game1.player || !f.IsSitting())
                    return true;
                var x = Environment.StackTrace;
                var tile = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / 64f;
                var distance = Vector2.Distance(Game1.player.Tile, tile);
                if (distance > Config.MaxRadius)
                {
                    tile = Vector2.Lerp(Game1.player.Tile, tile, Config.MaxRadius / distance);
                }
                __result = tile;
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.CanEmote))]
        public static class Farmer_CanEmote_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling Farmer.CanEmote");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Farmer), nameof(Farmer.IsSitting)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                    else if (codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Farmer), nameof(Farmer.isRidingHorse)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static void Postfix(Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || __result || !who.IsSitting())
                    return;
                who.StopSitting(true);
                __result = true;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling GameLocation.checkAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Farmer), nameof(Farmer.IsSitting)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                    else if (codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Farmer), nameof(Farmer.isRidingHorse)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}