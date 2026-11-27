using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MultiStoryFarmhouse
{
    internal class CodePatches
    {


        public static void resetLocalState_Prefix(ref Vector2 __state)
        {
            __state = new Vector2(-1, -1);
            if (Game1.isWarping && Game1.player.previousLocationName == "MultipleFloors0")
            {
                __state = new Vector2(Game1.xLocationAfterWarp, Game1.yLocationAfterWarp);
            }
        }
        public static void resetLocalState_Postfix(Vector2 __state)
        {
            if (__state.X >= 0)
            {
                Game1.player.Position = __state * 64f;
                Game1.xLocationAfterWarp = Game1.player.getTileX();
                Game1.yLocationAfterWarp = Game1.player.getTileY();
            }
        }

        public static void loadDataToLocations_Prefix(List<GameLocation> gamelocations)
        {
            ModEntry.context.Monitor.Log($"Checking save for multiple floors");

            List<string> possibleFloors = ModEntry.GetPossibleFloors();

            for (int i = 0; i < possibleFloors.Count(); i++)
            {
                string floorName = possibleFloors[i];
                DecoratableLocation location = (DecoratableLocation)Game1.locations.FirstOrDefault(l => l.Name == $"MultipleFloors{i}");
                if (location == null)
                {
                    Vector2 stairs = ModEntry.floorsList[floorName].stairsStart;
                    int x = (int)stairs.X;
                    int y = (int)stairs.Y;

                    ModEntry.context.Monitor.Log($"adding floor MultipleFloors{i}");
                    location = new DecoratableLocation($"Maps/MultipleFloorsMap{i}", $"MultipleFloors{i}");
                    Warp warp;
                    Warp warp2;
                    if (i < possibleFloors.Count - 1)
                    {
                        Vector2 stairs1 = ModEntry.floorsList[possibleFloors[i + 1]].stairsStart;
                        int x1 = (int)stairs1.X;
                        int y1 = (int)stairs1.Y;
                        ModEntry.context.Monitor.Log($"adding upstairs warps");

                        warp = new Warp(x + 4, y + 3, $"MultipleFloors{i + 1}", x1 + 1, y1 + 2, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp2 = new Warp(x + 5, y + 3, $"MultipleFloors{i + 1}", x1 + 2, y1 + 2, true, false);
                        if (!location.warps.Contains(warp2))
                            location.warps.Add(warp2);
                    }
                    if (i > 0)
                    {
                        Vector2 stairs0 = ModEntry.floorsList[possibleFloors[i - 1]].stairsStart;
                        int x0 = (int)stairs0.X;
                        int y0 = (int)stairs0.Y;
                        ModEntry.context.Monitor.Log($"adding downstairs warps");
                        warp = new Warp(x + 1, y + 3, $"MultipleFloors{i - 1}", x0 + 4, y0 + 2, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp2 = new Warp(x + 2, y + 3, $"MultipleFloors{i - 1}", x0 + 5, y0 + 2, true, false);
                        if (!location.warps.Contains(warp2))
                            location.warps.Add(warp2);
                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"adding farmhouse warps");
                        warp = new Warp(x + 1, y + 3, "FarmHouse", 8, 24, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp2 = new Warp(x + 2, y + 3, "FarmHouse", 9, 24, true, false);
                        if (!location.warps.Contains(warp2))
                            location.warps.Add(warp2);
                    }

                    Game1.locations.Add(location);

                }
                else
                    ModEntry.context.Monitor.Log($"Game already has floor MultipleFloors{i}");
            }

        }

        public static bool getFloors_Prefix(DecoratableLocation __instance, ref List<Rectangle> __result)
        {
            if (!__instance.Name.StartsWith("MultipleFloors"))
                return true;
            Floor floor = ModEntry.GetFloor(__instance.Name);
            __result = floor.floors;

            return false;
        }

        public static bool getWalls_Prefix(DecoratableLocation __instance, ref List<Rectangle> __result)
        {
            if (!__instance.Name.StartsWith("MultipleFloors"))
                return true;
            Floor floor = ModEntry.GetFloor(__instance.Name);
            __result = floor.walls;

            return false;
        }
        
        public static bool CanPlaceThisFurnitureHere_Prefix(GameLocation __instance, ref bool __result, Furniture furniture)
        {
            if (!__instance.Name.StartsWith("MultipleFloors") || furniture == null)
                return true;

            __result = true;
            return false;
        }
    }
}