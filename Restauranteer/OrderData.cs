using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Restauranteer
{
    public class OrderData
    {
        public Texture2D dishTexture;
        public Rectangle dishSourceRect;
        public string dishId;
        public string dishName;
        public string dishDisplayName;
        public int dishPrice;
        public bool loved;

        public OrderData(string dishId, string dishName, string dishDisplayName, int dishPrice, bool loved)
        {
            this.dishId = dishId;
            this.dishName = dishName;
            this.dishDisplayName = dishDisplayName;
            this.dishPrice = dishPrice;
            this.loved = loved;
        }
    }
}