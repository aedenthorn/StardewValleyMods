using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.SaveSerialization;
using System;

namespace LineOfSight
{
    public partial class ModEntry
    {

        private static bool IsActive()
        {
            return Config.ModEnabled && Context.IsWorldReady && !Config.ExcludedLocations.Contains(Game1.currentLocation.Name) && !Config.ExcludedLocationTypes.Contains(Game1.currentLocation.GetType().Name) && Game1.currentLocation?.Map?.Layers?.Count > 0;
        }
        private static bool IsVisible(Vector2 vector2)
        {
            Vector2 pos = vector2;
            float distance = Vector2.Distance(pos, Game1.player.Tile);
            var build = Game1.currentLocation.Map.GetLayer("Buildings");
            while (distance > 1)
            {
                pos = Vector2.Lerp(pos, Game1.player.Tile, 1 / distance);
                int x = (int)Math.Round(pos.X);
                int y = (int)Math.Round(pos.Y);
                if (build?.Tiles[x, y] != null)
                {
                    return false;
                }
                distance = Vector2.Distance(pos, Game1.player.Tile);
            }
            return true;
        }
    }
}