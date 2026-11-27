using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;

namespace AllChestsMenu
{
	public class ShippingBinChest : Chest
	{
		public ShippingBinChest() : base()
		{
			netItems.Value = Game1.getFarm().getShippingBin(Game1.player) as Inventory;
			playerChest.Value = false;
			canBeGrabbed.Value = false;
		}

		public override int GetActualCapacity()
		{
			return Items.Count;
		}

		public override void ShowMenu()
		{
			ItemGrabMenu itemGrabMenu = new(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightShippableObjects, Game1.getFarm().shipItem, "", null, snapToBottom: true, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);

			itemGrabMenu.initializeUpperRightCloseButton();
			itemGrabMenu.setBackgroundTransparency(b: false);
			itemGrabMenu.setDestroyItemOnClick(b: true);
			itemGrabMenu.initializeShippingBin();
			Game1.activeClickableMenu = itemGrabMenu;
		}
	}
}
