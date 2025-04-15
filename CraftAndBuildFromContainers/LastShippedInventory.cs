using StardewValley;
using StardewValley.Inventories;

namespace CraftAndBuildFromContainers
{
	public class LastShippedInventory : Inventory
	{
		public LastShippedInventory()
		{
			if (Game1.getFarm().lastItemShipped is not null)
			{
				Add(Game1.getFarm().lastItemShipped);
			}
		}

		public new int ReduceId(string itemId, int count)
		{
			int value = base.ReduceId(itemId, count);

			if (CountItemStacks() <= 0)
			{
				IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);

				if (shippingBin.Count > 0)
				{
					shippingBin.RemoveAt(shippingBin.Count - 1);
				}
				Game1.getFarm().lastItemShipped = null;
			}
			return value;
		}
	}
}
