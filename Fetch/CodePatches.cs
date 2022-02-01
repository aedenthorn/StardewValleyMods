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

        private static void Character_MovePosition_Prefix(Character __instance, GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            if (!Config.EnableMod || !(__instance is Pet))
                return;
            if (!fetchDataDict.ContainsKey(__instance.Name))
            {
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
                                path = path
                            };
                            fetching = true;
                            break;
                        }
                    }
                }

                if (!fetching)
                {
                    fetchDataDict.Remove(__instance.Name);
                    return;
                }
                SMonitor.Log("Fetching");

            }
            if (!fetchDataDict.TryGetValue(__instance.Name, out FetchData fetchData))
                return;

            if(fetchData.fetched.Chunks.Count == 0 || fetchData.fetchee.currentLocation.Name != __instance.currentLocation.Name)
            {
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
                        fetchDataDict[__instance.Name].isFetching = false;
                        fetchDataDict[__instance.Name].isBringing = true;
                        fetchDataDict[__instance.Name].path = path;
                    }
                    else
                    {
                        fetchDataDict.Remove(__instance.Name);
                        return;
                    }
                    SMonitor.Log("Bringing to fetchee");
                }
            }
            Vector2 next = new Vector2(-1, -1);
            for (int i = 0; i < fetchData.path.Count; i++)
            {
                if (__instance.getTileLocation() == fetchData.path[i])
                {
                    if (i < fetchData.path.Count - 1)
                        next = fetchData.path[i + 1];
                    else next = fetchData.path[i];
                }
            }
            if (next.X < 0)
            {
                fetchDataDict.Remove(__instance.Name);
                return;
            }
            if (fetchData.isBringing)
            {
                fetchData.fetched.Chunks[0].position.Value = __instance.getTileLocation() * 64;
                float distance = Vector2.Distance(fetchData.fetched.Chunks[0].position, fetchData.fetchee.getTileLocation() * 64);
                if (distance <= Config.GrabDistance)
                {
                    SMonitor.Log("Brought to fetchee");
                    fetchDataDict.Remove(__instance.Name);
                    return;
                }
            }
            else
            {
                fetchDataDict.Remove(__instance.Name);
                return;
            }
            __instance.SetMovingUp(false);
            __instance.SetMovingDown(false);
            __instance.SetMovingLeft(false);
            __instance.SetMovingRight(false);

            if (__instance.getTileLocation() == next)
                return;
            SMonitor.Log($"Moving to {next}");
            if (__instance.getTileLocation().X < next.X)
            {
                __instance.SetMovingRight(true);
            }
            else if (__instance.getTileLocation().X > next.X)
            {
                __instance.SetMovingLeft(true);
            }
            else if (__instance.getTileLocation().Y > next.Y)
            {
                __instance.SetMovingUp(true);
            }
            else if (__instance.getTileLocation().Y < next.Y)
            {
                __instance.SetMovingDown(true);
            }
        }
    }
}