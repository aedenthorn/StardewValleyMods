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
        public int price;
        public IList<Texture2D> texture = new List<Texture2D>();
        public int frames = 1;
        public string openSound = "openChest";
        public string texturePath;
        public string description;
        public Rectangle boundingBox;
    }
}