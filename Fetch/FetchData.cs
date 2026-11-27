using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace Fetch
{
    public class FetchData
    {
        public List<Vector2> path;
        public int nextTile;
        public bool isFetching;
        public bool isBringing;
        public Farmer fetchee;
        public Debris fetched;

    }
}