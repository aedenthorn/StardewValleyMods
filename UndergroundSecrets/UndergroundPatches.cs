using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
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
                CollapsingFloors.collapseFloor(__instance as MineShaft, playerStandingPosition);
                return false;
            }
            if(action.StartsWith("tilePuzzle_"))
            {
                TilePuzzles.pressTile(__instance as MineShaft, playerStandingPosition, action);
                return false;
            }
            if(action.StartsWith("lightPuzzle_"))
            {
                LightPuzzles.pressTile(__instance as MineShaft, playerStandingPosition, action);
                return false;
            }
            if (action == "randomTrap")
            {
                Traps.TriggerRandomTrap(__instance as MineShaft, playerStandingPosition, true);
                return false;
            }
            return true;
        }

        public static void GameLocation_loadMap_postfix(GameLocation __instance)
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

        public static bool MineShaft_addLevelChests_prefix(MineShaft __instance)
        {
            if(config.OverrideTreasureRooms && !Game1.player.chestConsumedMineLevels.ContainsKey(__instance.mineLevel) && __instance.loadedMapNumber == 10 && __instance.mapPath.Value == "Maps\\Mines\\10" && (helper.Reflection.GetField<NetBool>(__instance, "netIsTreasureRoom").GetValue().Value || (__instance.mineLevel < 120 && __instance.mineLevel % 20 == 0) || __instance.mineLevel == 10 || __instance.mineLevel == 50 || __instance.mineLevel == 70 ||__instance.mineLevel == 90 || __instance.mineLevel == 110))
            {
                Vector2 spot = new Vector2(9, 9);
                if (__instance.mineLevel % 20 == 0 && __instance.mineLevel % 40 != 0)
                {
                    spot.Y += 4f;
                }

                switch (Game1.random.Next(4))
                {
                    case 0:
                        LightPuzzles.CreatePuzzle(spot, __instance);
                        break;
                    case 1:
                        OfferingPuzzles.CreatePuzzle(spot, __instance);
                        break;
                    case 2:
                        RiddlePuzzles.CreatePuzzle(spot, __instance);
                        break;
                    case 3:
                        TilePuzzles.CreatePuzzle(spot, __instance);
                        break;
                }
                return false;
            }
            return true;
        }

        public static bool MineShaft_checkAction_prefix(MineShaft __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
            if (tile != null && who.IsLocalPlayer)
            {
                tile.Properties.TryGetValue("Action", out PropertyValue property);
                if (property != null)
                {
                    string action = property.ToString();

                    if (action.StartsWith("offerPuzzleSteal"))
                    {
                        OfferingPuzzles.StealAttempt(__instance, action, tileLocation, who);
                    }
                    else if (who.ActiveObject != null && action.StartsWith("offerPuzzle_"))
                    {
                        OfferingPuzzles.OfferObject(__instance, action, tileLocation, who);
                    }
                    else if (action.StartsWith("undergroundAltar_"))
                    {
                        Altars.OfferObject(__instance, action, tileLocation, who);
                    }
                    else if (action == "undergroundRiddles")
                    {
                        RiddlePuzzles.Interact(__instance, tileLocation, who);
                    }
                }
            }
            return true;
        }

        public static void MineShaft_enterMineShaft_postfix(MineShaft __instance, ref int ___lastLevelsDownFallen)
        {
            if (__instance.mineLevel < 121 && __instance.mineLevel + ___lastLevelsDownFallen > 120)
            {
                ___lastLevelsDownFallen = 120 - __instance.mineLevel;
            }
        }
    }
}