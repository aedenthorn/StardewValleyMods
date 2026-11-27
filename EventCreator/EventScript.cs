using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCreator
{
    public class EventScript
    {
        public static Dictionary<string, List<EventParam>> eventScripts = new Dictionary<string, List<EventParam>>()
        {
        };

        public string command;
        public List<EventParam> parameters = new List<EventParam>();

        public EventScript(string script)
        {
            var a = ArgUtility.SplitQuoteAware(script, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, true).ToList();
            command = a[0];
            if(eventScripts.TryGetValue(command, out var list))
            {
                for (int i = 0; i < list.Count && i < a.Count - 1; i++)
                {
                    list[i].value = a[i + 1];
                    parameters.Add(list[i]);
                }
            }
        }

        public EventScript(string command, string parameters, string description)
        {
            var a = ArgUtility.SplitQuoteAware(script, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, true).ToList();
            command = a[0];
            if(eventScripts.TryGetValue(command, out var list))
            {
                for (int i = 0; i < list.Count && i < a.Count - 1; i++)
                {
                    list[i].value = a[i + 1];
                    parameters.Add(list[i]);
                }
            }
        }
    }
}