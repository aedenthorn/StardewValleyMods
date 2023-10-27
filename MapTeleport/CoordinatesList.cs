using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace MapTeleport
{
    public class CoordinatesList
    {
        public List<Coordinates> coordinates = new List<Coordinates>();

        public void AddAll(CoordinatesList other)
        {
            this.coordinates = this.coordinates.Concat(other.coordinates).ToList<Coordinates>();
        }
        public void Add(Coordinates other)
        {
            this.coordinates.Add(other);
        }
    }
    public class Coordinates
    {
        public string name;
        public string mapName;
        public int x;
        public int y;
        public int id;
        public string altId;
        public bool enabled = true;
    }
}