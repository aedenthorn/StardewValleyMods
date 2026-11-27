using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ImmersiveScarecrows
{
    public interface IImmersiveApi
    {
        public Object GetObjectAtMouse();
        public Object GetObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner);
        public bool IsObjectAtMouse();
        public bool IsObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner);
        public List<Vector2> GetRange(Vector2 tile, int corner, int radius);
        public List<Vector2> GetRange(GameLocation location, Vector2 tile);
    }
    public class ImmersiveApi : IImmersiveApi
    {
        public Object GetObjectAtMouse()
        {
            return ModEntry.GetScarecrowAtMouse();
        }
        public Object GetObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            if (!ModEntry.GetScarecrowTileBool(location, ref tile, ref corner))
                return null;

            location.terrainFeatures.TryGetValue(tile, out var tf);
            return ModEntry.GetScarecrow(tf, corner);
        }
        public List<Vector2> GetRange(Vector2 tile, int corner, int radius)
        {
            return ModEntry.GetScarecrowTiles(tile, corner, radius);
        }
        public List<Vector2> GetRange(GameLocation location, Vector2 tile)
        {
            HashSet<Vector2> tiles = new HashSet<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                Vector2 cornerTile = tile;
                if (IsObjectAtTileCorner(location, ref cornerTile, ref i))
                {
                    var obj = ModEntry.GetScarecrow(location.terrainFeatures[cornerTile], i);
                    tiles.AddRange(ModEntry.GetScarecrowTiles(cornerTile, i, obj.GetRadiusForScarecrow()));
                }
            }
            return tiles.ToList();
        }
        public bool IsObjectAtMouse()
        {
            var tile = Game1.currentCursorTile;
            var corner = ModEntry.GetMouseCorner();
            return ModEntry.GetScarecrowTileBool(Game1.currentLocation, ref tile, ref corner);
        }
        public bool IsObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            return ModEntry.GetScarecrowTileBool(location, ref tile, ref corner);
        }
    }
}