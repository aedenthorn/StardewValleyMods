using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace UndergroundSecrets
{
    internal class UndergroundPatches
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }
        public static bool GameLocation_performTouchAction_prefix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            if (!(__instance is MineShaft))
                return true;
            ModEntry.context.Monitor.Log($"touch action at {playerStandingPosition} {fullActionString}");

            string action = fullActionString.Split(' ')[0];
            if(action == "collapseFloor")
            {
                CollapsedFloors.collapseFloor(__instance as MineShaft, playerStandingPosition);
                return false;
            }
            if(action.StartsWith("tilePuzzle_"))
            {
                TilePuzzles.pressTile(__instance as MineShaft, playerStandingPosition, action);
                return false;
            }
            if (action == "randomTrap")
            {
                Traps.TriggerRandomTrap(__instance as MineShaft, playerStandingPosition);
                return false;
            }
            return true;
        }

        internal static void MineShaft_loadMap_postfix(MineShaft __instance)
        {
            if (!Context.IsWorldReady || __instance.mineLevel == 0 || __instance.mineLevel == 77377)
                return;

            Utils.AddSecrets(__instance);
        }

        public static IEnumerable<CodeInstruction> MineShaft_populateLevel_transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            try
            {
                bool start = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldarg_0)
                    {
                        if (start)
                        {
                            codes.RemoveRange(i, 3);
                            break;
                        }
                        else
                            start = true;
                    }
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(MineShaft_populateLevel_transpiler)}:\n{ex}", LogLevel.Error);
            }

            return codes.AsEnumerable();
        }
    }
}