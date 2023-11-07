using StardewValley;
using StardewModdingAPI;
using StardewValley.Locations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FreeLove
{
    public class PathFindControllerPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;
        public static IKissingAPI kissingAPI;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }
        public static void PathFindController_Prefix(Character c, GameLocation location, ref Point endPoint)
        {
            try
            {
                if (!Config.EnableMod || !(c is NPC) || !(c as NPC).isVillager() || !(c as NPC).isMarried() || !(location is FarmHouse) || endPoint == (location as FarmHouse).getEntryLocation())
                    return;

                if (ModEntry.IsInBed(location as FarmHouse, new Rectangle(endPoint.X * 64, endPoint.Y * 64, 64, 64)))
                {
                    Point point = ModEntry.GetSpouseBedEndPoint(location as FarmHouse, c.Name);
                    if(point.X < 0 || point.Y < 0)
                    {
                        Monitor.Log($"Error setting bed endpoint for {c.Name}", LogLevel.Warn);
                    }
                    else
                    {
                        endPoint = point;
                        Monitor.Log($"Moved {c.Name} bed endpoint to {endPoint}");
                    }
                }
                else if (IsColliding(c, location, endPoint))
                {
                    var point = (location as FarmHouse).getRandomOpenPointInHouse(Game1.random);
                    if(point != Point.Zero)
                    {
                        endPoint = point;
                        Monitor.Log($"Moved {c.Name} endpoint to random point {endPoint}");
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(PathFindController_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static bool IsColliding(Character c, GameLocation location, Point endPoint)
        {
           
            Monitor.Log($"Checking {c.Name} endpoint in farmhouse");
            using (IEnumerator<Character> characters = location.characters.GetEnumerator())
            {
                while (characters.MoveNext())
                {
                    if(characters.Current != c)
                    {
                        if(characters.Current.TilePoint == endPoint || (characters.Current is NPC && (characters.Current as NPC).controller?.endPoint == endPoint))
                        {
                            Monitor.Log($"{c.Name} endpoint {endPoint} collides with {characters.Current.Name}");
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}