using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomFarmBuildings
{
    public class CustomBuildingData
    {
        public string name;
        public string description;
        public string mapPath;
        public string texturePath;
        public int width;
        public int height;
        public Point humanDoor;
        public Point animalDoor;
        public int maxOccupants;
        public int daysToConstruct;
        public bool magical;
        public bool decoratable;
        public int cost;
        public List<MaterialData> materials = new List<MaterialData>();
    }

    public class MaterialData
    {
        public string id;
        public int amount;
    }
}