using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomWallpaperFramework
{
    public class WallPaperTileData
    {
        public string id;
        public Vector2 startTile = new Vector2(-1, -1);
        public List<Vector2> backTiles = new List<Vector2>();
        public List<Vector2> buildingTiles = new List<Vector2>();
    }
}