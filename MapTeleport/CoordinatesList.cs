using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MapTeleport
{
    public class CoordinatesList
    {
        public List<Coordinates> coordinates = new List<Coordinates>();
    }
    public class Coordinates
    {
        public string mapName;
        public int x;
        public int y;
        public int id;
    }
}