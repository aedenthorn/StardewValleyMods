using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCreator
{
    public class EventData
    {
        public string id;
        public List<string> conditions = new List<string>();
        public List<EventScript> scripts = new List<EventScript>();

        public EventData(KeyValuePair<string, string> keyValuePair)
        {
            id = keyValuePair.Key;
            ImportEvent(keyValuePair.Value);
        }
        private void ImportEvent(string script)
        {
            scripts = ArgUtility.SplitQuoteAware(script, '/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, true).Select(s => new EventScript(s)).ToList();

        }
    }
}