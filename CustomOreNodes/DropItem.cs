
namespace CustomOreNodes
{
    public class DropItem
    {
        public string itemIdOrName;
        public float dropChance;
        public int minAmount;
        public int maxAmount;
        public int luckyAmount;
        public int minerAmount;

        public DropItem()
        {

        }

        public DropItem(string itemInfo)
        {
            int i = 0;
            string[] infos = itemInfo.Split(';');
            if (infos.Length != 5)
            {
                ModEntry.context.Monitor.Log($"improper syntax in drop item string {itemInfo}: number of elements is {infos.Length} but should be 5", StardewModdingAPI.LogLevel.Error);
                throw new System.ArgumentException();
            }
            itemIdOrName = infos[i++];
            dropChance = float.Parse(infos[i++]);
            minAmount = int.Parse(infos[i].Split(',')[0]);
            maxAmount = int.Parse(infos[i++].Split(',')[1]);
            luckyAmount = int.Parse(infos[i++]);
            minerAmount = int.Parse(infos[i++]);

        }
    }
}