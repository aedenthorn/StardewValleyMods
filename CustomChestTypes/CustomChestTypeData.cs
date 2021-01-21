using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CustomChestTypes
{
    public class CustomChestTypeData
    {
        public List<CustomChestType> chestTypes = new List<CustomChestType>();
    }

    public class CustomChestType
    {
        public string name;
        public int id;
        public int capacity;
        public int rows;
        public int price;
        public Texture2D texture;
        public string texturePath;
        public string description;
        public Rectangle boundingBox;
    }
}