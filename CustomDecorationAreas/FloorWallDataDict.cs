//using Harmony;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomDecorationAreas
{
    public class FloorWallDataDict
    {
        public List<FloorWallData> data = new List<FloorWallData>();
    }

    public class FloorWallData
    {
        public string name;
        public bool replaceWalls;
        public bool replaceFloors;
        public bool replaceNonDecorationTiles = true;
        public string getFloorsFromFile;
        public string getWallsFromFile;
        public List<Rectangle> floors = new List<Rectangle>();
        public List<Rectangle> floorsOmit = new List<Rectangle>();
        public List<Rectangle> walls = new List<Rectangle>();
        public List<Rectangle> wallsOmit = new List<Rectangle>();
    }
}