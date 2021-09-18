using MagnetMod;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;

public static class ObjectPatches
{
    private static IMonitor Monitor;

    // call this method from your Entry class
    public static void Initialize(IMonitor monitor)
    {
        Monitor = monitor;
    }

    public static float magnetRangeMult;

    public static bool playerInRange_Prefix(Vector2 position, Farmer farmer, ref bool __result)
    {
        try
        {
            if (magnetRangeMult < 0)
                __result = true;
            else __result = Math.Abs(position.X + 32f - (float)farmer.getStandingX()) <= (float)farmer.MagneticRadius * magnetRangeMult && Math.Abs(position.Y + 32f - (float)farmer.getStandingY()) <= (float)farmer.MagneticRadius * magnetRangeMult;
            return false;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(playerInRange_Prefix)}:\n{ex}", LogLevel.Error);
            return true;
        }
    }

}