using StardewModdingAPI;
using StardewValley;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace Bork
{
    public partial class ModEntry
    {
        public static List<string> ItemCountInLocation(Dictionary<string, string> list)
        {
            try
            {
                var loc = list["location"];
                var itemId = list["item"];
                var location = Game1.getLocationFromName(loc, true);
                int count = 0;
                string name = TokenParser.ParseText(Game1.objectData[itemId].DisplayName, null, null, null) ;

                Utility.ForEachItem(delegate (Item item)
                {
                    if (item is Object obj && obj.ItemId == itemId)
                    {
                        count += obj.Stack;
                    }
                    return true;
                });
                return new List<string>()
                {
                    string.Format(GetNumberedTranslation("item_count_in_location_"), name, count, location.Name)
                };
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred while processing item in location: {ex.Message}", LogLevel.Error);
            }
            return new List<string>() { GetNumberedTranslation("error_processing_query_") };
        }
        public static List<string> WhereNPC(Dictionary<string, string> list)
        {
            try
            {
                var npc = Game1.getCharacterFromName(list["npc"], false);
                if (npc == null)
                {
                    return new List<string>() { 
                        string.Format(GetNumberedTranslation("npc_not_found_"), list["npc"])
                    };
                }

                return new List<string>()
                {
                    string.Format(GetNumberedTranslation("npc_location_"), npc.displayName, npc.currentLocation.DisplayName)
                };
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred while processing where NPC: {ex.Message}", LogLevel.Error);
            }
            return new List<string>() { GetNumberedTranslation("error_processing_query_") };
        }
        public static List<string> NPCBirthday(Dictionary<string, string> list)
        {
            try
            {
                if(!Game1.characterData.TryGetValue(list["npc"], out var data))
                {
                    return new List<string>() { GetNumberedTranslation("npc_not_found_") };
                }

                return new List<string>()
                {
                    string.Format(GetNumberedTranslation("npc_birthday_"), TokenParser.ParseText(data.DisplayName, null, null, null), data.BirthSeason, data.BirthDay)
                };
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred in NPCBirthday: {ex.Message}", LogLevel.Error);
            }
            return new List<string>() { GetNumberedTranslation("error_processing_query_") };
        }
        public static List<string> WeatherSeasonDay(Dictionary<string, string> list)
        {
            try
            {
                var season = list["season"];
                var day = int.Parse(list["day"]);
                var days = WorldDate.Now().TotalDays;
                if(season != Game1.season.ToString() || day < WorldDate.Now().DayOfMonth)
                {
                    var seasons = new List<string>() { "spring", "summer", "fall", "winter" };
                    var seasonIndex = seasons.IndexOf(season);
                    var currentSeasonIndex = seasons.IndexOf(Game1.season.ToString().ToLower());
                    var seasonDiff = (seasonIndex - currentSeasonIndex + 4) % 4;
                    days += seasonDiff * 28;
                    days += day - WorldDate.Now().DayOfMonth;
                }
                else
                {
                    days += day - WorldDate.Now().DayOfMonth;
                }

                return new List<string>()
                {
                    string.Format(GetNumberedTranslation("weather_season_day_"), season, day, GetWeather(days))
                };
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred in WeatherSeasonDay: {ex.Message}", LogLevel.Error);
            }
            return new List<string>() { GetNumberedTranslation("error_processing_query_") };
        }
        public static List<string> WeatherTomorrow(Dictionary<string, string> list)
        {
            try
            {
                var days = WorldDate.Now().TotalDays + 1;

                return new List<string>()
                {
                    string.Format(GetNumberedTranslation("weather_tomorrow_"), GetWeather(days))
                };
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred in WeatherTomorrow: {ex.Message}", LogLevel.Error);
            }
            return new List<string>() { GetNumberedTranslation("error_processing_query_") };
        }
        
        public static List<string> Joke(Dictionary<string, string> list)
        {
            try
            {

                return SHelper.ModContent.Load<List<string>>("assets/jokes.json");
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred in Joke: {ex.Message}", LogLevel.Error);
            }
            return new List<string>() { GetNumberedTranslation("error_processing_query_") };
        }
        public static List<string> Quote(Dictionary<string, string> list)
        {
            try
            {

                return SHelper.ModContent.Load<List<string>>("assets/quotes.json");
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred in Quote: {ex.Message}", LogLevel.Error);
            }
            return new List<string> { GetNumberedTranslation("error_processing_query_") };
        }
        public static List<string> AboutYou(Dictionary<string, string> list)
        {
            try
            {
                return new List<string> { GetNumberedTranslation("about_you_") };
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error occurred in AboutYou: {ex.Message}", LogLevel.Error);
            }
            return new List<string>() { GetNumberedTranslation("error_processing_query_") };
        }

    }
}