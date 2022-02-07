using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace TrainTracks
{
    public class TrainTracksApi
    {
        public bool TryPlaceTrack(GameLocation location, Vector2 tile, int index, string switchData, int speed, bool force = false)
        {
            return ModEntry.TryPlaceTrack(location, tile, index, switchData, speed, force);

        }
        public bool IsTrackAt(GameLocation location, Vector2 tile)
        {
            return location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is Flooring && (feature as Flooring).whichFloor.Value == -42 && feature.modData.TryGetValue(ModEntry.trackKey, out string indexString) && int.TryParse(indexString, out int useless);
        }
        public bool RemoveTrack(GameLocation location, Vector2 tile)
        {
            if (!IsTrackAt(location, tile))
                return false;
            if ((location.terrainFeatures[tile] as Flooring).whichFloor.Value == -42)
                location.terrainFeatures.Remove(tile);
            else
            {
                location.terrainFeatures[tile].modData.Remove(ModEntry.trackKey);
            }
            return true;
        }
    }
}