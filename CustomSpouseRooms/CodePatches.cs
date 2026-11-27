using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using static System.Net.Mime.MediaTypeNames;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomSpouseRooms
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmHouse), nameof(FarmHouse.checkAction))]
        public static class FarmHouse_checkAction_Patch 
        {
            public static void Postfix(FarmHouse __instance, Location tileLocation)
            {

                try
                {
                    if (__instance.map.GetLayer("Buildings").Tiles[tileLocation] != null)
                    {
                        int tileIndex = __instance.map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
                        if (tileIndex == 2173 && Game1.player.friendshipData.ContainsKey("Emily") && Game1.player.friendshipData["Emily"].IsMarried())
                        {
                            TemporaryAnimatedSprite t = __instance.getTemporarySpriteByID(5858585);
                            if (t != null && t is EmilysParrot)
                            {
                                (t as EmilysParrot).doAction();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in FarmHouse_checkAction_Postfix:\n{ex}", LogLevel.Error);
                }
            }
        }
        //[HarmonyPatch(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile")]
        public static class DecoratableLocation_IsFloorableOrWallpaperableTile_Patch
        {
            public static bool Prefix(DecoratableLocation __instance, int x, int y, ref bool __result)
            {
                if (!Config.EnableMod || !(__instance is FarmHouse))
                    return true;
                foreach (var room in ModEntry.currentRoomData.Values)
                {
                    Rectangle rect = new Rectangle(room.startPos.X + 1, room.startPos.Y + 1, 6, 7);
                    if (rect.Contains(x, y))
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FarmHouse), nameof(FarmHouse.updateFarmLayout))]
        public static class FarmHouse_updateFarmLayout_Patch
        {
            public static void Prefix(FarmHouse __instance, ref bool ___displayingSpouseRoom)
            {
                if (!Config.EnableMod)
                    return;
                loadingSpouseRooms = false;
                try
                {
                    var allSpouses = GetSpouses(__instance.owner, -1).Keys.ToList();
                    if (allSpouses.Count == 0)
                        return;
                    ___displayingSpouseRoom = false;
                    //loadingSpouseRooms = true;
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in FarmHouse_updateFarmLayout_Prefix:\n{ex}", LogLevel.Error);
                }
            }
        }
        [HarmonyPatch(typeof(FarmHouse), nameof(FarmHouse.updateFarmLayout))]

        public static class FarmHouse_updateFarmLayout_Postfix
        {
            public static void Postfix(FarmHouse __instance, HashSet<string> ____appliedMapOverrides)
            {
                if (!Config.EnableMod || !loadingSpouseRooms)
                    return;
                loadingSpouseRooms = false;
                var allSpouses = GetSpouses(__instance.owner, -1).Keys.ToList();
                if (allSpouses.Count == 0)
                    return;
                SMonitor.Log("Loading spouse rooms, clearing current room data");
                currentRoomData.Clear();
                GetFarmHouseSpouseRooms(__instance, allSpouses, out List<string> orderedSpouses, out List<string> customSpouses);

                __instance.reloadMap();

                if (____appliedMapOverrides.Contains("spouse_room"))
                {
                    ____appliedMapOverrides.Remove("spouse_room");
                }
                __instance.map.Properties.Remove("DayTiles");
                __instance.map.Properties.Remove("NightTiles");

                foreach (string spouse in customSpouses)
                {
                    SpouseRoomData srd = customRoomData[spouse];
                    MakeSpouseRoom(__instance, ____appliedMapOverrides, srd);
                }

                int xOffset = 7;

                for (int i = 0; i < orderedSpouses.Count; i++)
                {
                    string spouse = orderedSpouses[i];

                    SpouseRoomData srd = null;
                    if (customRoomData.TryGetValue(spouse, out SpouseRoomData srd1))
                    {
                        srd = new SpouseRoomData(srd1);
                    }

                    Point corner = __instance.GetSpouseRoomCorner() + new Point(xOffset * i, 0);

                    Point spouseOffset;
                    int indexInSpouseMapSheet = -1;

                    if (srd != null && (srd.upgradeLevel < 0 || srd.upgradeLevel == __instance.upgradeLevel))
                    {
                        spouseOffset = srd.spousePosOffset;
                        indexInSpouseMapSheet = srd.templateIndex;
                    }
                    else
                    {
                        spouseOffset = new Point(4, 5);
                    }

                    var shellStart = corner - new Point(1, 1);

                    if (i == 0)
                        __instance.spouseRoomSpot = shellStart;

                    if (srd == null)
                    {
                        srd = new SpouseRoomData()
                        {
                            name = spouse,
                            spousePosOffset = spouseOffset,
                            templateIndex = indexInSpouseMapSheet
                        };

                    }

                    srd.shellType = i < orderedSpouses.Count - 1 ? "custom_spouse_room_open_right" : "custom_spouse_room_closed_right";
                    if (i == 0 && __instance.upgradeLevel > 1)
                    {
                        srd.shellType += "_2";
                    }
                    SMonitor.Log($"Using shell {srd.shellType} for {srd.name}");

                    srd.startPos = shellStart;

                    if (customRoomData.TryGetValue((i + 1).ToString(), out SpouseRoomData srdi) && (srdi.upgradeLevel >= __instance.upgradeLevel || srdi.upgradeLevel < 0) && !srdi.islandFarmHouse)
                    {
                        SMonitor.Log($"Found index override {i + 1}, using start pos, etc.");
                        srd.startPos = srdi.startPos;
                        if (srdi.shellType is not null)
                            srd.shellType = srdi.shellType;
                        if (srdi.templateName is not null)
                            srd.templateName = srdi.templateName;
                    }

                    MakeSpouseRoom(__instance, ____appliedMapOverrides, srd, i == 0);
                }
            }
        }
        [HarmonyPatch(typeof(Layer), "_RebakeRow")]
        public static class Layer__RebakeRow_Patch
        {
            public static void Prefix(Layer __instance, int y, ref int[,] ____skipMap)
            {
                if (____skipMap.GetLength(0) < __instance.LayerWidth || ____skipMap.GetLength(1) < __instance.LayerHeight)
                {
                    ____skipMap = new int[__instance.LayerWidth, __instance.LayerHeight];
                }
            }
        }
        [HarmonyPatch(typeof(FarmHouse), nameof(FarmHouse.loadSpouseRoom))]
        public static class FarmHouse_loadSpouseRoom_Patch
        {
            public static bool Prefix(FarmHouse __instance)
            {
                if (!Config.EnableMod)
                    return true;

                try
                {
                    var allSpouses = GetSpouses(__instance.owner, -1).Keys.ToList();
                    if (allSpouses.Count == 0)
                        return true;

                    if (ModEntry.customRoomData.Count == 0 && allSpouses.Count == 1)// single spouse, no customizations
                    {
                        SMonitor.Log("Single uncustomized spouse room, letting vanilla game take over");
                        return true;
                    }
                    SMonitor.Log("Preparing to load custom spouse rooms");
                    loadingSpouseRooms = true;
                    return false;
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in FarmHouse_loadSpouseRoom_Prefix:\n{ex}", LogLevel.Error);
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(DecoratableLocation), nameof(DecoratableLocation.MakeMapModifications))]
        public static class DecoratableLocation_MakeMapModifications_Patch
        {
            public static void Postfix(DecoratableLocation __instance, HashSet<string> ____appliedMapOverrides)
            {
                if (!Config.EnableMod || __instance is not IslandFarmHouse)
                    return;
                try
                {
                    SMonitor.Log("Loading spouse rooms, clearing current room data");
                    ModEntry.currentIslandRoomData.Clear();
                    var allSpouses = GetSpouses(Game1.player, -1).Keys.ToList();
                    if (allSpouses.Count == 0)
                        return;

                    GetIslandFarmHouseSpouseRooms(__instance as IslandFarmHouse, allSpouses, out List<string> customSpouses);

                    __instance.reloadMap();

                    if (____appliedMapOverrides.Contains("spouse_room"))
                    {
                        ____appliedMapOverrides.Remove("spouse_room");
                    }
                    __instance.map.Properties.Remove("DayTiles");
                    __instance.map.Properties.Remove("NightTiles");

                    foreach (string spouse in customSpouses)
                    {
                        SpouseRoomData srd = ModEntry.customRoomData[spouse];
                        MakeSpouseRoom(__instance, ____appliedMapOverrides, srd);
                    }

                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in DecoratableLocation_MakeMapModifications_Postfix:\n{ex}", LogLevel.Error);
                }
            }
        }


        [HarmonyPatch(typeof(FarmHouse), "resetLocalState")]
        public static class FarmHouse_resetLocalState_Patch
        {
            public static void Postfix(FarmHouse __instance)
            {
                try
                {
                    if (!Config.EnableMod)
                        return;
                    Farmer f = __instance.owner;

                    if (f == null)
                    {
                        return;
                    }

                    foreach (var kvp in currentRoomData)
                    {
                        CheckSpouseThing(__instance, kvp.Value);
                    }


                    if (GetSpouses(f, 0).ContainsKey("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
                    {
                        if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
                        {
                            DelayedAction.playSoundAfterDelay("croak", 1000, null, null, -1, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Failed in FarmHouse_resetLocalState_Postfix:\n{ex}", LogLevel.Error);
                }
            }
        }
        [HarmonyPatch(typeof(NetWorldState), nameof(NetWorldState.hasWorldStateID))]
        public static class NetWorldState_hasWorldStateID_Patch
        {
            public static bool Prefix(ref string id, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                if (id == "sebastianFrogReal")
                {
                    SMonitor.Log($"Allowing frogs");
                    id = "sebastianFrog";
                    return true;
                }
                if (id == "sebastianFrog")
                {
                    SMonitor.Log($"Preventing frogs");
                    __result = false;
                    return false;
                }
                return true;
            }
        }


    }
}