using Newtonsoft.Json.Serialization;
using StardewValley;
using System;
using Object = StardewValley.Object;

namespace FurnitureDisplayFramework
{
    public partial class ModEntry
    {

        private static void HandleDeserializationError(object sender, ErrorEventArgs e)
        {
            //var currentError = e.ErrorContext.Error.Message;
            //SMonitor.Log(currentError);
            e.ErrorContext.Handled = true;
        }

        private static void HandleSerializationError(object sender, ErrorEventArgs e)
        {
            //var currentError = e.ErrorContext.Error.Message;
            //SMonitor.Log(currentError);
            e.ErrorContext.Handled = true;
        }

        private static Object GetObjectFromID(string id, int amount, int quality)
        {
            return new Object(id, amount, false, -1, quality);
            /*
            //SMonitor.Log($"Trying to get object {id}, DGA {apiDGA != null}, JA {apiJA != null}");

            Object obj = null;
            try
            {

                if (int.TryParse(id, out int index))
                {
                    //SMonitor.Log($"Spawning object with index {id}");
                    return new Object(index, amount, false, -1, quality);
                }
                else
                {
                    var dict = SHelper.Content.Load<Dictionary<int, string>>("Data/ObjectInformation", ContentSource.GameContent);
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value.StartsWith(id + "/"))
                            return new Object(kvp.Key, amount, false, -1, quality);
                    }
                }
                if (apiDGA != null && id.Contains("/"))
                {
                    object o = apiDGA.SpawnDGAItem(id);
                    if (o is Object)
                    {
                        //SMonitor.Log($"Spawning DGA object {id}");
                        (o as Object).Stack = amount;
                        (o as Object).Quality = quality;
                        return (o as Object);
                    }
                }
                if (apiJA != null)
                {
                    int idx = apiJA.GetObjectId(id);
                    if (idx != -1)
                    {
                        //SMonitor.Log($"Spawning JA object {id}");
                        return new Object(idx, amount, false, -1, quality);

                    }
                }
            }
            catch (Exception ex)
            {
                //SMonitor.Log($"Exception: {ex}", LogLevel.Error);
            }
            //SMonitor.Log($"Couldn't find item with id {id}");
            return obj;
            */
        }
    }
}