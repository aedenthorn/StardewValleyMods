using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmPlots
{
    public class AutoPlot
    {
        public HashSet<Vector2> tiles = new HashSet<Vector2>();
        public bool[] harvest = new bool[4];
        public bool[] till = new bool[4];
        public bool[] buy = new bool[4];
        public bool[] active = new bool[4];
        public string[] fertilizers = new string[4];
        public string[] seeds = new string[4];
        public long creator;
        public Vector2 GetKeyTile()
        {
            Vector2 tile = new Vector2(float.MaxValue, float.MaxValue);
            foreach(Vector2 t in tiles)
            {
                if (t.X < tile.X || (t.X == tile.X && t.Y < tile.Y))
                    tile = t;
            }
            return tile;
        }
    }
}