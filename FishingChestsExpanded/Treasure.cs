namespace FishingChestsExpanded
{
    public class Treasure
    {
        public int index;
        public int value;
        public string type;

        public Treasure(int key, int price, string type)
        {
            this.index = key;
            this.value = price;
            this.type = type;
        }
    }
}