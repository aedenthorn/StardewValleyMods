namespace FastForward
{
    internal class OrderData
    {
        public int dish;
        public string dishName;
        public bool loved;

        public OrderData(int dish, string dishName, bool loved)
        {
            this.dish = dish;
            this.dishName = dishName;
            this.loved = loved;
        }
    }
}