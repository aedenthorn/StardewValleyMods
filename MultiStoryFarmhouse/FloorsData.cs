//using Harmony;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MultiStoryFarmhouse
{
    public class FloorsData
    {
        public List<Floor> floors = new List<Floor>();
        public FloorsData()
        {
        }
    }

    public class Floor
    {
        public string name;
        public string mapPath;
        public Vector2 stairsStart;
        public List<Rectangle> floors = new List<Rectangle>();
        public List<Rectangle> walls = new List<Rectangle>();
    }
}