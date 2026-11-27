using Microsoft.Xna.Framework;
using StardewValley;

namespace ImmersiveSprinklersScarecrows
{
    public interface IImmersiveApi
    {
        public Object GetObjectAtMouse();
        public Object GetObjectAtTile(GameLocation location, ref Vector2 tile, ref int corner);
        public bool IsObjectAtMouse();
        public bool IsObjectAtTile(GameLocation location, ref Vector2 tile, ref int corner);
    }
    public class ImmersiveApi : IImmersiveApi
    {
        public Object GetObjectAtMouse()
        {
            return ModEntry.GetSprinklerAtMouse();
        }
        public Object GetObjectAtTile(GameLocation location, ref Vector2 tile, ref int corner)
        {
            ModEntry.TryGetSprinkler(location, tile, out var sprinkler);
            return sprinkler;
        }
        public bool IsObjectAtMouse()
        {
            var tile = ModEntry.GetMouseTile();
            return ModEntry.TryGetSprinkler(Game1.currentLocation, tile, out var sprinkler);
        }
        public bool IsObjectAtTile(GameLocation location, ref Vector2 tile, ref int corner)
        {
            return ModEntry.TryGetSprinkler(location, tile, out var sprinkler);
        }
    }
}