using Microsoft.Xna.Framework;
using System;

namespace ModularBuildings
{
    internal class ModularBuilding
    {
        public Rectangle rect;
        public string roof;
        public string wall;
        public string gable;

        public ModularBuilding(Rectangle buildingRect)
        {
            this.rect = buildingRect;
        }
        
        public ModularBuilding(string buildingString)
        {
            var parts = buildingString.Split('|');
            var rectParts = parts[0].Split(',');
            rect = new Rectangle(int.Parse(rectParts[0]),int.Parse(rectParts[1]),int.Parse(rectParts[2]),int.Parse(rectParts[3]));
            wall = parts[1];
            roof = parts[2];
            gable = parts[3];
        }

        public string GetBuildingString()
        {
            return $"{rect.X},{rect.Y},{rect.Width},{rect.Height}|{wall}|{roof}|{gable}";
        }
    }
}