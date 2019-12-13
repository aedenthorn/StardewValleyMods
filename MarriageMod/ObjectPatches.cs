using StardewModdingAPI;
using StardewValley;
using System;

public class ObjectPatches
{
    private static IMonitor Monitor;

    // call this method from your Entry class
    public static void Initialize(IMonitor monitor)
    {
        Monitor = monitor;
    }

    public static void tryToReceiveActiveObject_Prefix(Farmer who, string __state)
    {
        try
        {
            __state = null;
            if (who.ActiveObject.ParentSheetIndex == 460)
            {
                __state = who.spouse;
                who.spouse = null;
            }
            return;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(tryToReceiveActiveObject_Prefix)}:\n{ex}", LogLevel.Error);
            return; // run original logic
        }
    }
    public static void tryToReceiveActiveObject_Postfix(Farmer who, string __state)
    {
        try
        {
            if(__state != null && who.spouse == null)
            {
                who.spouse = __state;
            }
            return;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(tryToReceiveActiveObject_Prefix)}:\n{ex}", LogLevel.Error);
            return; // run original logic
        }
    }
}