using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace TrainTracks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static bool TryPlaceTrack(GameLocation location, Vector2 tile, int index, string switchData = null, int speed = -1, bool force = false)
        {
            if (!location.terrainFeatures.TryGetValue(tile, out TerrainFeature oldFeature) || (force && oldFeature is not Flooring) || (oldFeature is Flooring && oldFeature.modData.ContainsKey(trackKey)))
            {
                if (Config.PlaceTrackSound.Length > 0)
                    location.playSound(Config.PlaceTrackSound);
                if (oldFeature is not Flooring)
                {
                    Flooring f = new Flooring(-42) { modData = new ModDataDictionary() { { trackKey, index + "" } } };
                    location.terrainFeatures[tile] = f;
                }
                else
                {
                    location.terrainFeatures[tile].modData[trackKey] = index + "";
                }
            }
            else if (oldFeature != null && oldFeature is Flooring)
            {
                location.terrainFeatures[tile].modData[trackKey] = index + "";
                if (Config.PlaceTrackSound.Length > 0)
                    location.playSound(Config.PlaceTrackSound);
            }
            else return false;
            
            if(switchData != null)
            {
                SMonitor.Log($"Adding switch data to track {tile}: {switchData}");
                location.terrainFeatures[tile].modData[switchDataKey] = switchData;
            }
            
            if(speed > -1)
            {
                SMonitor.Log($"Adding speed to track {tile}: {speed}");
                location.terrainFeatures[tile].modData[speedDataKey] = speed+"";
            }

            return true;
        }

        private static Vector2 GetOffset(int facingDirection)
        {
            var offset = Vector2.Zero;
            switch (facingDirection)
            {
                case 0:
                    offset = new Vector2(16, 16);
                    break;
                case 1:
                    offset = new Vector2(16, -16);
                    break;
                case 2:
                    offset = new Vector2(16, 0);
                    break;
                case 3:
                    offset = new Vector2(16, -16);
                    break;
            }
            return offset;
        }
    }
}
