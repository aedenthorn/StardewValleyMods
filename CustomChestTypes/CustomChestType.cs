using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CustomChestTypes
{
    public class CustomChestType
    {
        public string name;
        public int capacity;
        public int price;
        public IList<string> texture = new List<string>();
        public int frames = 1;
        public string openSound = "openChest";
        public string texturePath;
        public string description;
        public Rectangle boundingBox;
    }
}