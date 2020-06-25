using System.Collections.Generic;
using xTile.Dimensions;

namespace Swim
{
    public class DiveMapData
    {
        public List<DiveMap> Maps { get; set; } = new List<DiveMap>();
    }

    public class DiveMap
    {
        public string Name { get; set; }
        public List<DiveLocation> DiveLocations { get; set; }
    }

    public class DiveLocation
    {
        public Rectangle GetRectangle()
        {
            return new Rectangle(StartX, StartY, Width, Height);
        }

        public int StartX { get; set; } = -1;
        public int StartY { get; set; } = -1;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public string OtherMapName { get; set; }
        public DivePosition OtherMapPos { get; set; } = null;
    }
    public class DivePosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}