using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace CustomSeasoning
{
    public interface ICustomSeasoningAPI
    {
        public string[] GetSeasonings();
        public void SetSeasoning(Object dish, string seasoning);
    }
    public class CustomSeasoningAPI : ICustomSeasoningAPI
    {
        public string[] GetSeasonings()
        {
            return ModEntry.SeasoningDict.Keys.ToArray();
        }
        public void SetSeasoning(Object dish, string seasoning)
        {
            if (ModEntry.SeasoningDict.ContainsKey(seasoning))
            {
                dish.modData[ModEntry.seasoningKey] = seasoning;
            }
        }
    }
}