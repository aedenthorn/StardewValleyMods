using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace ImmersiveSprinklersAndScarecrows
{
    public interface IImmersiveApi
    {
        public Object GetObjectAtMouse();
        public Object GetObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner);
        public bool IsObjectAtMouse();
        public bool IsObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner);
        public int GetRadius(Object obj);
        public List<Vector2> GetRange(Vector2 tile, int corner, int radius);
        public List<Vector2> GetRange(GameLocation location, Vector2 tile);

    }
    public class ImmersiveApi : IImmersiveApi
    {
        public Object GetObjectAtMouse()
        {
            return ModEntry.GetSprinklerAtMouse();
        }
        public Object GetObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            if (!ModEntry.GetSprinklerTileBool(location, ref tile, ref corner, out var str))
                return null;

            location.terrainFeatures.TryGetValue(tile, out var tf);
            return ModEntry.GetSprinkler(tf, corner, false);
        }

        public int GetRadius(Object obj)
        {
            return ModEntry.GetSprinklerRadius(obj);
        }

        public List<Vector2> GetRange(Vector2 tile, int corner, int radius)
        {
            return ModEntry.GetSprinklerTiles(tile, corner, radius);
        }

        public List<Vector2> GetRange(GameLocation location, Vector2 tile)
        {
            HashSet<Vector2> tiles = new HashSet<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                Vector2 cornerTile = tile;
                if(IsObjectAtTileCorner(location, ref cornerTile, ref i))
                {
                    var obj = ModEntry.GetSprinklerCached(location.terrainFeatures[cornerTile], i, location.terrainFeatures[cornerTile].modData.ContainsKey(ModEntry.nozzleKey + i));
                    tiles.AddRange(ModEntry.GetSprinklerTiles(cornerTile, i, GetRadius(obj)));
                }
            }
            return tiles.ToList();
        }

        public bool IsObjectAtMouse()
        {
            var tile = Game1.currentCursorTile;
            var corner = ModEntry.GetMouseCorner();
            return ModEntry.GetSprinklerTileBool(Game1.currentLocation, ref tile, ref corner, out var str);
        }
        public bool IsObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            return ModEntry.GetSprinklerTileBool(location, ref tile, ref corner, out var str);
        }
    }
}