using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using System;
using System.Collections.Generic;

namespace CloseDoors
{
    public partial class ModEntry
    {
        private static bool TryCloseDoor(GameLocation location, Point tilePoint)
        {
            foreach (var d in location.interiorDoors.Doors)
            {
                if (d.Position == tilePoint)
                {
                    if (!d.Value)
                        return false;
                    location.playSound("doorClose", Utility.PointToVector2(tilePoint), null, SoundContext.Default);
                    d.CleanUpLocalState();
                    d.ResetLocalState();
                    location.interiorDoors[tilePoint] = false;
                    return true;
                }
            }
            return false;
        }
        private static bool IsDoorOpen(GameLocation location, Point tilePoint)
        {

            foreach (var d in location.interiorDoors.Doors)
            {
                if (d.Position == tilePoint)
                {
                    return d.Value;
                }
            }
            return false;
        }
    }
}