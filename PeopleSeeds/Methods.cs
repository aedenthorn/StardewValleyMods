using StardewValley;
using System.Collections.Generic;

namespace PeopleSeeds
{
    public partial class ModEntry
    {
        private static bool GetSeasoningData(Item obj, out SeasoningData seasoningData)
        {
            seasoningData = null;
            return obj.modData.TryGetValue(seasoningKey, out string seasoning) && SeasoningDict.TryGetValue(seasoning, out seasoningData);
        }
        private static List<KeyValuePair<string, int>> AddSeasoning()
        {
            if(whichSeasoning == null || crafted == null || whichSeasoning == "none")
                return null;
            if(whichSeasoning == "917")
            {
                crafted.Quality = 2;
                return new() { new("917", 1) };
            }
            var seasoningData = SeasoningDict[whichSeasoning];
            crafted.Quality = seasoningData.Quality;
            crafted.modData[seasoningKey] = whichSeasoning;
            return new() { new(whichSeasoning, 1) };
        }

    }
}