using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Pets;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EventIndicators
{
	public partial class ModEntry : Mod
    {

        private void TryAddWarp(Warp w)
        {
            try
            {
                var l = Game1.getLocationFromName(w.TargetName);
                if (l == null)
                    return;
                var x = w.X;
                var y = w.Y;
                if (Game1.currentLocation.Map.Layers[0].Tiles.Array.GetLength(0) <= x)
                    x = Game1.currentLocation.Map.Layers[0].Tiles.Array.GetLength(0) - 1;
                if (Game1.currentLocation.Map.Layers[0].Tiles.Array.GetLength(1) <= y)
                    y = Game1.currentLocation.Map.Layers[0].Tiles.Array.GetLength(1) - 1;
                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;
                var point = new Point(x, y);

                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
                defaultInterpolatedStringHandler.AppendFormatted(Game1.currentSeason);
                defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.dayOfMonth);
                string key = defaultInterpolatedStringHandler.ToStringAndClear();
                if (Event.tryToLoadFestivalData(key, out var dataAssetName, out var ddata, out var locationName, out var startTime, out var endTime) && locationName == l.Name && Game1.timeOfDay >= startTime && Game1.timeOfDay < endTime)
                {
                    var festival_name = "festival_" + key;
                    Dictionary<string, string> festival_data = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth.ToString());

                    if (festival_data?.TryGetValue("startedMessage", out festival_name) == false)
                    {
                        if (!festival_data.TryGetValue("locationDisplayName", out festival_name))
                        {
                            festival_name = l.Name;
                            if (!(festival_name == "Forest"))
                            {
                                if (!(festival_name == "Town"))
                                {
                                    if (!(festival_name == "Beach"))
                                    {
                                        LocationData data = GameLocation.GetData(festival_name);
                                        festival_name = TokenParser.ParseText((data != null) ? data.DisplayName : null, null, null, null) ?? festival_name;
                                    }
                                    else
                                    {
                                        festival_name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2639");
                                    }
                                }
                                else
                                {
                                    festival_name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2637");
                                }
                            }
                            else
                            {
                                festival_name = (Game1.IsWinter ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2634") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2635"));
                            }
                        }
                        festival_name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2640", festival_data["name"]) + festival_name;
                    }
                    eventsDict[point] = festival_name;
                    return;
                }

                if (l.currentEvent != null)
                {
                    eventsDict[point] = l.currentEvent.id;
                    return;
                }
                if (l.TryGetLocationEvents(out var eventAssetName, out var events))
                {
                    if (events != null)
                    {
                        foreach (string eventKey in events.Keys)
                        {
                            string eventId = l.checkEventPrecondition(eventKey);
                            if (!string.IsNullOrEmpty(eventId) && eventId != "-1" && GameLocation.IsValidLocationEvent(eventKey, events[eventKey]))
                            {
                                eventsDict[point] = eventId;
                                eventsDictFull[point] = Game1.parseText(events[eventKey], Game1.smallFont, Game1.uiViewport.Width / 2);
                                return;
                            }
                        }
                        PetData data;
                        if (Game1.IsMasterGame && Game1.stats.DaysPlayed >= 20U && !Game1.player.mailReceived.Contains("rejectedPet") && !Game1.player.hasPet() && Pet.TryGetData(Game1.player.whichPetType, out data) && l.Name == data.AdoptionEventLocation && !string.IsNullOrWhiteSpace(data.AdoptionEventId) && !Game1.player.eventsSeen.Contains(data.AdoptionEventId))
                        {
                            eventsDict[point] = data.AdoptionEventId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error adding event indicator for {w?.TargetName} at {w?.X},{w?.Y}:\n\n{ex}");
            }
        }
    }
}
