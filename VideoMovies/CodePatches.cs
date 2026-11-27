using HarmonyLib;
using Microsoft.Xna.Framework.Media;
using StardewValley;
using StardewValley.Events;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using System;

namespace VideoMovies
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(MovieTheaterScreeningEvent), nameof(MovieTheaterScreeningEvent.getMovieEvent))]
        public static class MovieTheaterScreeningEvent_getMovieEvent_Patch
        {
            public static void Postfix(string movieId)
            {
                if (!Config.ModEnabled )
                    return;
                movieId = MovieTheater.GetMovieIdFromLegacyIndex(movieId);
                if(!Videos.TryGetValue(movieId, out var data))
                {
                    return;
                }
                currentVideos.Clear();
                foreach (var v in data.Videos)
                {
                    TryLoadFromWMV(v, out var video);
                    currentVideos.Add(video);
                }
            }
        }

        [HarmonyPatch(typeof(Event), "addSpecificTemporarySprite")]
        public static class Event_addSpecificTemporarySprite_Patch
        {
            public static bool Prefix(Event __instance, string key, GameLocation location, string[] args)
            {
                if (!Config.ModEnabled || key != "movieFrame")
                    return true;
                if (!ArgUtility.TryGet(args, 2, out string movieId, out string error2, true, "string movieId") || !ArgUtility.TryGetInt(args, 3, out int frame, out error2, "int frame") || !ArgUtility.TryGetInt(args, 4, out int duration, out error2, "int duration"))
                {
                    __instance.LogCommandError(args, error2, false);
                    return false;
                }
                movieId = MovieTheater.GetMovieIdFromLegacyIndex(movieId);
                if(!Videos.TryGetValue(movieId, out var video))
                {
                    return true;
                }
                MovieData data;
                if (!MovieTheater.TryGetMovieData(movieId, out data))
                {
                    __instance.LogCommandError(args, "no movie found with ID '" + movieId + "'", false);
                    return false;
                }
                if(frame < currentVideos.Count)
                {
                    videoPlayer.Play(currentVideos[frame]);
                }
                return false;
            }
        }
    }
}
