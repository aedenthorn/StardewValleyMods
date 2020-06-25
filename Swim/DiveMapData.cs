using System.Collections.Generic;

namespace Swim
{
    public class DiveMapData
    {
        public List<DiveMap> maps = new List<DiveMap>();
    }

    public class DiveMap
    {
        public string name;
        public List<DiveLocation> diveLocations;
    }

    public class DiveLocation
    {
        public int startx;
        public int starty;
        public int width;
        public int height;
        public string otherMapName;
    }
}