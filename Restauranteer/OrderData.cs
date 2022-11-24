namespace Restauranteer
{
    internal class OrderData
    {
        public int dish;
        public string dishName;
        public int dishPrice;
        public bool loved;

        public OrderData(int dish, string dishName, int dishPrice, bool loved)
        {
            this.dish = dish;
            this.dishName = dishName;
            this.dishPrice = dishPrice;
            this.loved = loved;
        }
    }
}