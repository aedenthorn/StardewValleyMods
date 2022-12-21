using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BeePaths
{
    public class HiveData
    {
        public Vector2 hiveTile;
        public Vector2 cropTile;
        public List<BeeData> bees = new();
    }
    public class BeeData
    {
        public Vector2 pos;
        public Vector2 startPos;
        public Vector2 endPos;
        public Vector2 startTile;
        public Vector2 endTile;
        public float speed;
    }
}