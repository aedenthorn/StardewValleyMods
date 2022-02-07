using Microsoft.Xna.Framework;
using StardewValley;

namespace ImportMap
{
    public interface ITrainTracksApi
    {
        bool TryPlaceTrack(GameLocation location, Vector2 tile, int index, string switchData, int speedData, bool force = false);
        bool IsTrackAt(GameLocation location, Vector2 tile);
        bool RemoveTrack(GameLocation location, Vector2 tile);
    }
}