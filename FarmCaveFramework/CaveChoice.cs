using System.Collections.Generic;

namespace FarmCaveFramework
{
    public class CaveChoice
    {
        public string id;
        public string choice = "";
        public string description;
        public List<CaveObject> objects = new List<CaveObject>();
        public List<CaveResource> resources = new List<CaveResource>();
        public int resourceChance = 66;
        public List<CaveAnimation> animations = new List<CaveAnimation>();
    }

    public class CaveObject
    {
        public int index;
        public int X;
        public int Y;
    }
    public class CaveResource
    {
        public int index;
        public int weight;
        public int min = 1;
        public int max = 1;
    }
    public class CaveAnimation
    {
        public int index;
        public int X;
        public int Y;
        public string sourceFile;
        public int sourceX;
        public int sourceY;
        public float interval;
        public int length;
        public int loops;
        public float scale;
        public bool light;
        public float lightRadius;
    }
}