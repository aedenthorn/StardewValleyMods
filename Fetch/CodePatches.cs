using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;

namespace Fetch
{
    public partial class ModEntry
    {

        private static Dictionary<string, FetchData> fetchDataDict = new Dictionary<string, FetchData>();

        private static void Pet_update_Prefix(Pet __instance)
        { 
            if(Config.EnableMod && fetchDataDict.TryGetValue(__instance.Name, out FetchData data) && data.isBringing && data.fetched.Chunks.Count > 0)
            {
                data.fetched.Chunks[0].position.Value = __instance.getTileLocation() * 64;
            }
        }
        private static void Character_MovePosition_Prefix(Character __instance, ref bool ___moveUp, ref bool ___moveLeft, ref bool ___moveRight, ref bool ___moveDown, GameLocation currentLocation)
        {
            if (!Config.EnableMod || !(__instance is Pet))
                return;

            if (!fetchDataDict.ContainsKey(__instance.Name))
            {
                SMonitor.Log("Not in dict");

                Farmer tfetchee = null;
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (farmer.currentLocation.Name == currentLocation.Name)
                    {
                        tfetchee = farmer;
                        break;
                    }
                }
                if (tfetchee == null)
                    return;
                bool fetching = false;
                List<Debris> debrisList = new List<Debris>(currentLocation.debris);
                debrisList.Sort(delegate (Debris a, Debris b) 
                { 
                    if(a.Chunks.Count == 0)
                    {
                        if (b.Chunks.Count == 0)
                        {
                            return 0;
                        }
                        return 1;
                    }
                    if(b.Chunks.Count == 0)
                    {
                        return -1;
                    }
                    return Vector2.Distance(a.Chunks[0].position.Value / 64, __instance.getTileLocation()).CompareTo(Vector2.Distance(b.Chunks[0].position.Value / 64, __instance.getTileLocation()));
                });
                foreach (var debris in debrisList)
                {
                    if ((debris.debrisType.Value == Debris.DebrisType.RESOURCE || debris.debrisType.Value == Debris.DebrisType.OBJECT) && debris.Chunks.Count > 0)
                    {
                        var path = GetShortestPath(__instance.getTileLocation(), new Vector2((int)debris.Chunks[0].position.Value.X / 64, (int)debris.Chunks[0].position.Value.Y / 64), __instance.currentLocation);
                        if (path != null)
                        {
                            fetchDataDict[__instance.Name] = new FetchData() 
                            { 
                                isFetching = true,
                                isBringing = false,
                                fetched = debris,
                                fetchee = tfetchee,
                                path = path,
                                nextTile = 0
                            };
                            fetching = true;
                            break;
                        }
                    }
                }

                if (!fetching)
                {
                    SMonitor.Log("No fetch object");

                    fetchDataDict.Remove(__instance.Name);
                    return;
                }
                SMonitor.Log("Fetching");

            }
            if (!fetchDataDict.TryGetValue(__instance.Name, out FetchData fetchData))
                return;

            if(fetchData.fetched.Chunks.Count == 0 || fetchData.fetchee.currentLocation.Name != __instance.currentLocation.Name)
            {
                SMonitor.Log("Fetchee not in location or no chunks");

                fetchDataDict.Remove(__instance.Name);
                return;
            }
            Vector2 direction = Vector2.Zero;
            if (fetchData.isFetching)
            {
                float distance = Vector2.Distance(fetchData.fetched.Chunks[0].position, __instance.getTileLocation() * 64);
                if(distance <= Config.GrabDistance)
                {

                    var path = GetShortestPath(__instance.getTileLocation(), fetchData.fetchee.getTileLocation(), __instance.currentLocation);
                    if (path != null)
                    {
                        fetchData.isFetching = false;
                        fetchData.isBringing = true;
                        fetchData.path = path;
                        fetchData.nextTile = 0;
                        fetchDataDict[__instance.Name] = fetchData;
                        SMonitor.Log("got path to fetchee");
                    }
                    else
                    {
                        SMonitor.Log("no path to fetchee");
                        fetchDataDict.Remove(__instance.Name);
                        return;
                    }
                    SMonitor.Log("Bringing to fetchee");
                }
            }

            if (fetchData.isBringing)
            {
                //fetchData.fetched.Chunks[0].position.Value = __instance.getTileLocation() * 64;
                float distance = Vector2.Distance(fetchData.fetched.Chunks[0].position, fetchData.fetchee.getTileLocation() * 64);
                if (distance <= Config.GrabDistance)
                {
                    SMonitor.Log("Brought to fetchee");
                    fetchDataDict.Remove(__instance.Name);
                    return;
                }
            }
            else if(!fetchData.isFetching)
            {
                SMonitor.Log("not fetching or bringing");
                fetchDataDict.Remove(__instance.Name);
                return;
            }

            if (__instance.getTileLocation() == fetchData.path[fetchData.nextTile])
            {
                if (fetchData.path.Count > fetchData.nextTile + 1)
                {
                    SMonitor.Log($"advancing to next tile from {fetchData.path[fetchData.nextTile]} to {fetchData.path[fetchData.nextTile+1]}");
                    fetchData.nextTile++;
                    fetchDataDict[__instance.Name] = fetchData;
                }
                else
                {
                    SMonitor.Log($"At last tile");
                    return;
                }

            }

            SMonitor.Log($"Moving from {__instance.getTileLocation()} to {fetchData.path[fetchData.nextTile]}");
            if (__instance.getTileLocation().X < fetchData.path[fetchData.nextTile].X)
            {
                ___moveDown = false;
                ___moveUp = false;
                ___moveLeft = false;
                ___moveRight = true;
            }
            else if (__instance.getTileLocation().X > fetchData.path[fetchData.nextTile].X)
            {
                ___moveDown = false;
                ___moveUp = false;
                ___moveLeft = true;
                ___moveRight = false;
            }
            else if (__instance.getTileLocation().Y > fetchData.path[fetchData.nextTile].Y)
            {
                ___moveDown = false;
                ___moveUp = true;
                ___moveLeft = false;
                ___moveRight = false;
            }
            else if (__instance.getTileLocation().Y < fetchData.path[fetchData.nextTile].Y)
            {
                ___moveDown = true;
                ___moveUp = false;
                ___moveLeft = false;
                ___moveRight = false;
            }
        }
    }
}