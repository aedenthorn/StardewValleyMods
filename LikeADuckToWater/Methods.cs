using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace LikeADuckToWater
{
    public partial class ModEntry
    {

        private static bool NotReadyToSwim(FarmAnimal animal)
        {
            return !Config.ModEnabled || animal.controller is not null || animal.currentLocation != Game1.getFarm() || animal.currentLocation.waterTiles is null || !animal.CanSwim() || animal.isSwimming.Value || (!animal.wasPet.Value && (!Config.SwimAfterAutoPet || !animal.wasAutoPet.Value)) || (Config.EatBeforeSwimming && animal.fullness.Value < 195) || ducksToCheck.ContainsKey(animal) || animal.modData.ContainsKey(swamTodayKey);
        }
        private bool CheckDuck(FarmAnimal animal, HopInfo info)
        {
            if (animal is null || info is null || animal.currentLocation is not Farm || pickedTiles.Contains(info.hopTile) || FarmAnimal.NumPathfindingThisTick >= FarmAnimal.MaxPathfindingPerTick)
                return false;
            foreach(var a in (animal.currentLocation as Farm).Animals.Values)
            {
                if (a is not null && a.GetBoundingBox().Intersects(info.hoppedBox))
                    return false;
            }
            var c = new PathFindController(animal, animal.currentLocation, new PathFindController.isAtEnd(PathFindController.isAtEndPoint), info.dir, false, new PathFindController.endBehavior(TryHop), 200, new Point((int)info.hopTile.X, (int)info.hopTile.Y), true);
            FarmAnimal.NumPathfindingThisTick++;
            if (c.pathToEndPoint is not null)
            {
                pickedTiles.Add(info.hopTile);
                animal.controller = c;
                SMonitor.Log($"{animal.displayName} is travelling from {animal.getTileLocation()} to {info.hopTile} to swim");
                return true;
            }
            return false;
        }
        private static void DoHop(FarmAnimal animal, Vector2 key)
        {
            animal.Position = key * 64;
            animal.isSwimming.Value = true;
            animal.hopOffset = hopTileDict[key][0].offset;
            animal.pauseTimer = 0;
            SwamToday(animal);
            SMonitor.Log($"{animal.displayName} is hopping into the water");
        }
        private static bool isCollidingWater(GameLocation __instance, Character character, int x, int y)
        {
            if (!Config.ModEnabled || __instance is not Farm || __instance.waterTiles is null || character is not FarmAnimal || !((FarmAnimal)character).CanSwim())
                return true;
            int bi = __instance.getTileIndexAt(x, y, "Buildings");
            if (__instance.isWaterTile(x, y) && (bi < 0 || waterBuildingTiles.Contains(bi)) && !__instance.objects.ContainsKey(new Vector2(x, y)))
            {
                return false;
            }
            return true;
        }

        private static void TryHop(Character character, GameLocation location)
        {
            if (!hopTileDict.Any())
                return;
            var center = character.GetBoundingBox().Center.ToVector2();

            var keys = hopTileDict.Keys.ToList();
            keys.Sort(delegate (Vector2 a, Vector2 b)
            {
                return Vector2.Distance((a + new Vector2(0.5f, 0.5f)) * 64, center).CompareTo(Vector2.Distance((b + new Vector2(0.5f, 0.5f)) * 64, center));
            });
            foreach (var t in keys)
            {
                if (Vector2.Distance(center, (t + new Vector2(0.5f, 0.5f)) * 64) < 64)
                {
                    DoHop((FarmAnimal) character, t);
                    return;
                }
            }
        }
        private static void TryAddToQueue(FarmAnimal animal, GameLocation location)
        {
            if (!hopTileDict.Any())
                return;
            var center = animal.GetBoundingBox().Center.ToVector2();
            var keys = hopTileDict.Keys.ToList();
            keys.Sort(delegate (Vector2 a, Vector2 b)
            {
                return Vector2.Distance((b + new Vector2(0.5f, 0.5f)) * 64, center).CompareTo(Vector2.Distance((a + new Vector2(0.5f, 0.5f)) * 64, center));
            });
            Stack<HopInfo> stack = new Stack<HopInfo>();
            foreach (var t in keys)
            {
                var d = Vector2.Distance(center, (t + new Vector2(0.5f, 0.5f)) * 64);
                if (d < 64)
                {
                    DoHop(animal, t);
                    return;
                }
                else if(d / 64 > Config.MaxDistance)
                {
                    continue;
                }
                foreach(var i in hopTileDict[t])
                    stack.Push(i);
            }
            ducksToCheck[animal] = stack;
        }
        private static void RebuildHopSpots(Farm farm)
        {
            if (!Config.ModEnabled || farm.waterTiles is null)
                return;

            Stopwatch s = new Stopwatch();
            s.Start();

            long id = ((Multiplayer)AccessTools.Field(typeof(Game1), "multiplayer").GetValue(null)).getNewID();
            FarmAnimal animal = new FarmAnimal("Duck", id, Game1.player.UniqueMultiplayerID);
            hopTileDict = new();
            for (int y = 0; y < farm.map.Layers[0].LayerHeight; y++)
            {
                for (int x = 0; x < farm.map.Layers[0].LayerWidth; x++)
                {
                    if(farm.isWaterTile(x, y) && farm.doesTileHaveProperty(x, y, "Passable", "Back") != null)
                    {
                        farm.removeTileProperty(x, y, "Back", "Passable");
                    }
                    animal.Position = new Vector2(x, y) * 64;
                    if (!farm.waterTiles.waterTiles[x,y].isWater && !farm.isCollidingPosition(animal.GetBoundingBox(), Game1.viewport, false, 0, false, animal, false, false, false))
                    {
                        List<HopInfo> hoppableDirs = GetHoppableDirs(farm, animal);
                        if(hoppableDirs.Any())
                        {
                            hopTileDict.Add(new Vector2(x, y), hoppableDirs);
                        }
                    }
                }
            }
            s.Stop();
            SMonitor.Log($"Rebuild took {s.ElapsedMilliseconds}ms");

            SMonitor.Log($"Got {hopTileDict.Count} hoppable tiles");
        }

        private static List<HopInfo> GetHoppableDirs(Farm farm, FarmAnimal animal)
        {
            List<HopInfo> list = new List<HopInfo>();
            var tile = animal.getTileLocationPoint();
            for(int i = 0; i < 4; i++)
            {
                Vector2 offset = Utility.getTranslatedVector2(Vector2.Zero, i, 1f);
                Point offsetPoint = Utility.Vector2ToPoint(offset);
                if (offset != Vector2.Zero)
                {
                    Point hop_over_tile = tile + offsetPoint;
                    Point hop_tile = hop_over_tile + offsetPoint;
                    Rectangle hop_destination = animal.GetBoundingBox();
                    hop_destination.Offset(offset * 128);
                    if (farm.isWaterTile(hop_over_tile.X, hop_over_tile.Y) && farm.doesTileHaveProperty(hop_over_tile.X, hop_over_tile.Y, "Passable", "Buildings") == null && !farm.isCollidingPosition(hop_destination, Game1.viewport, false, 0, false, animal) && (!isCollidingWater(farm, animal, hop_destination.X / 64, hop_destination.Y / 64) || farm.isOpenWater(hop_tile.X, hop_tile.Y)))
                    {
                        list.Add(new HopInfo() { 
                            hopTile = tile.ToVector2(),
                            dir = i,
                            offset = offset * 128,
                            hoppedBox = hop_destination
                        });
                    }
                }
            }
            return list;
        }
        private static void SwamToday(FarmAnimal animal)
        {
            if (!animal.modData.ContainsKey(swamTodayKey) && Config.FriendshipGain > 0)
            {
                animal.friendshipTowardFarmer.Value = Math.Min(1000, animal.friendshipTowardFarmer.Value + Config.FriendshipGain);
            }
            animal.modData[swamTodayKey] = "true";
        }

    }
}