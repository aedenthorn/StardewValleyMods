using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bork
{
    public partial class ModEntry
    {

        public static Dialogue GetDialogue(string text = null)
        {
            if(text == null)
            {
                text = string.Format(GetNumberedTranslation(Game1.player.friendshipData.ContainsKey("bork") ? "welcome_" : "first_meet_"), Game1.player.Name);
            }
            else
            {
                text = ParseQuery(text);
            }
            return new Dialogue(new NPC(new AnimatedSprite(), Vector2.Zero, null, 2, "Bork", false, SHelper.GameContent.Load<Texture2D>("Portraits/Bork")), "", text);
        }

        private static string ParseQuery(string text)
        {
            text = text.ToLower().Trim();
            foreach(var kvp in DataDict)
            {
                int matchCount = MatchQuery(kvp.Value.Terms, text);
                Dictionary<string, string> varMatches = MatchVariables(kvp.Value.Variables, text);
                if (matchCount == kvp.Value.Terms.Count && varMatches.Count == kvp.Value.Variables.Count)
                {
                    var list = ResponseDict.TryGetValue(kvp.Value.Response, out var function) ? function.Invoke(varMatches) : SHelper.GameContent.Load<List<string>>(kvp.Value.Response);
                    var response = list[Game1.random.Next(list.Count)];
                    foreach(var v in varMatches)
                    {
                        response = response.Replace(v.Key, v.Value);
                    }
                    return response;
                }
            }
            return GetNumberedTranslation("unknown_query_");
        }

        private static string GetNumberedTranslation(string v)
        {
            int count = 1;
            while (SHelper.Translation.ContainsKey(v + count))
            {
                count++;
            }
            return SHelper.Translation.Get(v + Game1.random.Next(1, count));

        }

        private static int MatchQuery(List<string> terms, string text)
        {
            int matchCount = 0;
            foreach(var term in terms)
            {
                if (text.Contains(term) || (SynonymDict.TryGetValue(term, out var syns) && syns.Any(s => text.Contains(s))))
                {
                    matchCount++;
                }
            }
            return matchCount;
        }

        private static Dictionary<string, string> MatchVariables(List<string> vars, string text)
        {
            var matches = new Dictionary<string, string>();
            foreach (var v in vars)
            {
                switch (v)
                {
                    case "item":
                        foreach (var obj in Game1.objectData)
                        {
                            var name = obj.Value.Name.ToLower();
                            var dname = TokenParser.ParseText(obj.Value.DisplayName, null, null, null);
                            try
                            {
                                if (Regex.IsMatch(text, @"\b" + name + @"\b") || Regex.IsMatch(text, @"\b" + name + @"s\b") || Regex.IsMatch(text, @"\b" + dname + @"\b") || Regex.IsMatch(text, @"\b" + dname + @"s\b"))
                                {
                                    matches[v] = obj.Key;
                                    break;
                                }
                            }
                            catch { }
                        }
                        break;
                    case "location":
                        foreach (var loc in Game1.locations)
                        {
                            if (Regex.IsMatch(text, @"\b" + loc.Name.ToLower() + @"\b"))
                            {
                                matches[v] = loc.Name;
                                break;
                            }
                        }
                        break;
                    case "npc":
                        foreach(var kvp in Game1.characterData)
                        {
                            try
                            {
                                if (Regex.IsMatch(text, @"\b" + kvp.Key.ToLower() + @"\b") || Regex.IsMatch(text, @"\b" + TokenParser.ParseText(kvp.Value.DisplayName, null, null, null).ToLower() + @"\b"))
                                {
                                    matches[v] = kvp.Key;
                                    break;
                                }
                            }
                            catch { }
                        }
                        break;
                    case "day":
                        var match = Regex.Match(text, @"\b2[0-8]\b");
                        if(!match.Success)
                            match = Regex.Match(text, @"\b1[0-9]\b");
                        if(!match.Success)
                            match = Regex.Match(text, @"\b[0-9]\b");
                        if (match.Success)
                        {
                            matches[v] = match.Value;
                        }
                        break;
                    case "season":
                        foreach(var s in Enum.GetNames(typeof(Season)))
                        {
                            if (Regex.IsMatch(text, @"\b" + s.ToLower() + @"\b"))
                            {
                                matches[v] = s;
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return matches;
        }

        private static string GetWeather(int days)
        {

            WorldDate date = WorldDate.ForDaysPlayed(days);
            string forecast = Game1.IsMasterGame
                ? Game1.getWeatherModificationsForDate(date, Game1.weatherForTomorrow)
                : Game1.getWeatherModificationsForDate(date, Game1.netWorldState.Value.WeatherForTomorrow);

            string weather = null;
            switch (forecast)
            {
                case Game1.weather_sunny:
                case Game1.weather_debris:
                case Game1.weather_festival:
                case Game1.weather_wedding:
                    weather = GetNumberedTranslation("weather_sunny_");
                    break;
                case Game1.weather_rain:
                    weather = GetNumberedTranslation("weather_rainy_");
                    break;

                case Game1.weather_lightning:
                    weather = GetNumberedTranslation("weather_lightning_");
                    break;

                case Game1.weather_snow:
                    weather = GetNumberedTranslation("weather_snowy_");
                    break;

                case Game1.weather_green_rain:
                    weather = GetNumberedTranslation("weather_green_rain_");
                    break;

                default:
                    break;
            }
            return weather;
        }
    }
}