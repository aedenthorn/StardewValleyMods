using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace RobinWorkHours
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public bool IsThereABuldingUnderConstruction(GameLocation location)
        {
            if (location.buildings.Count > 0)
            {
                using (List<Building>.Enumerator enumerator = location.buildings.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.isUnderConstruction(false))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }
        private string GetTodayScheduleString(NPC robin)
        {
            if (robin.isMarried())
            {
                if (robin.hasMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth))
                {
                    return robin.getMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth);
                }
                string day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
                if (!Game1.isRaining && robin.hasMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
                {
                    return robin.getMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
                }
            }
            else
            {
                if (robin.hasMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth))
                {
                    return robin.getMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth);
                }
                int friendship = Utility.GetAllPlayerFriendshipLevel(robin);
                if (friendship >= 0)
                {
                    friendship /= 250;
                }
                while (friendship > 0)
                {
                    if (robin.hasMasterScheduleEntry(Game1.dayOfMonth.ToString() + "_" + friendship))
                    {
                        return robin.getMasterScheduleEntry(Game1.dayOfMonth.ToString() + "_" + friendship);
                    }
                    friendship--;
                }
                if (robin.hasMasterScheduleEntry(Game1.dayOfMonth.ToString()))
                {
                    return robin.getMasterScheduleEntry(Game1.dayOfMonth.ToString());
                }
                if (Game1.IsRainingHere(Game1.getLocationFromName(robin.DefaultMap)))
                {
                    if (Game1.random.NextDouble() < 0.5 && robin.hasMasterScheduleEntry("rain2"))
                    {
                        return robin.getMasterScheduleEntry("rain2");
                    }
                    if (robin.hasMasterScheduleEntry("rain"))
                    {
                        return robin.getMasterScheduleEntry("rain");
                    }
                }
                List<string> key = new List<string>
                {
                    Game1.currentSeason,
                    Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)
                };
                friendship = Utility.GetAllPlayerFriendshipLevel(robin);
                if (friendship >= 0)
                {
                    friendship /= 250;
                }
                while (friendship > 0)
                {
                    key.Add(friendship.ToString());
                    if (robin.hasMasterScheduleEntry(string.Join("_", key)))
                    {
                        return robin.getMasterScheduleEntry(string.Join("_", key));
                    }
                    friendship--;
                    key.RemoveAt(key.Count - 1);
                }
                if (robin.hasMasterScheduleEntry(string.Join("_", key)))
                {
                    return robin.getMasterScheduleEntry(string.Join("_", key));
                }
                if (robin.hasMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
                {
                    return robin.getMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
                }
                if (robin.hasMasterScheduleEntry(Game1.currentSeason))
                {
                    return robin.getMasterScheduleEntry(Game1.currentSeason);
                }
                if (robin.hasMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
                {
                    return robin.getMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
                }
                key.RemoveAt(key.Count - 1);
                key.Add("spring");
                friendship = Utility.GetAllPlayerFriendshipLevel(robin);
                if (friendship >= 0)
                {
                    friendship /= 250;
                }
                while (friendship > 0)
                {
                    key.Add(string.Empty + friendship.ToString());
                    if (robin.hasMasterScheduleEntry(string.Join("_", key)))
                    {
                        return robin.getMasterScheduleEntry(string.Join("_", key));
                    }
                    friendship--;
                    key.RemoveAt(key.Count - 1);
                }
                if (robin.hasMasterScheduleEntry("spring"))
                {
                    return robin.getMasterScheduleEntry("spring");
                }
            }
            return null;
        }
        public static bool IsRobinAtPlayerFarm()
        {
            NPC robin = Game1.getCharacterFromName("Robin");
            Farm farm = Game1.getFarm();
            if (robin == null || robin.currentLocation == null)
            {
                SMonitor.Log("Robin is not found or has no current location.", LogLevel.Warn);
                return false;
            }
            if (robin.currentLocation == farm)
            {
                SMonitor.Log("Robin is at the farm.", LogLevel.Trace);
                return true;
            }
            foreach (Building b in farm.buildings)
            {
                if (b.indoors.Value != null && robin.currentLocation == b.indoors.Value)
                {
                    SMonitor.Log("Robin is inside a building at the farm.", LogLevel.Trace);
                    return true;
                }
            }
            return false;
        }
    }
}