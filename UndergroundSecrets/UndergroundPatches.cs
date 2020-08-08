using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace UndergroundSecrets
{
    internal class UndergroundPatches
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int[] ores = new int[] { 378, 380, 384, 386};

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

        internal static void GameLocation_loadMap_postfix(GameLocation __instance)
        {
            if (!(__instance is MineShaft) || !Context.IsWorldReady)
                return;

            MineShaft shaft = __instance as MineShaft;


            if ((shaft.mineLevel < 121 && shaft.mineLevel % 20 == 0) || shaft.mineLevel == 0 || shaft.mineLevel == 220 || shaft.mineLevel == 77377)
                return;

            Utils.AddSecrets(shaft);
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

        internal static bool MineShaft_checkAction_prefix(MineShaft __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            if (who.ActiveObject == null || !ores.Contains(who.ActiveObject.ParentSheetIndex))
                return true;

            Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
            if (tile != null && who.IsLocalPlayer)
            {
                
                PropertyValue property;
                tile.Properties.TryGetValue("Action", out property);
                if (property != null)
                {
                    string action = property.ToString();
                    if (action.StartsWith("offerPuzzle_"))
                    {
                        string[] parts = action.Split('_').Skip(1).ToArray();
                        if(ores[int.Parse(parts[0])] == who.ActiveObject.ParentSheetIndex)
                        {
                            who.reduceActiveItemByOne();
                            __instance.setMapTileIndex(tileLocation.X, tileLocation.Y, OfferingPuzzles.offerIdx + 1 + int.Parse(parts[0]), "Buildings");
                            __instance.setMapTileIndex(tileLocation.X, tileLocation.Y - 2, 245, "Front");
                            __instance.removeTileProperty(tileLocation.X, tileLocation.Y, "Buildings", "Action");
                            Utils.DropChest(__instance, new Vector2(int.Parse(parts[1]), int.Parse(parts[2])));
                            Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("disarmed-trap"), Color.Green, 3500f));
                        }
                    }
                }
            }
            return true;
        }
    }
}