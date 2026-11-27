using StardewValley.Menus;

namespace CustomBackpack
{
    public static class Extensions
    {
        public static int Columns(this InventoryMenu menu)
        {
            return menu.capacity / menu.rows;
        }
        public static int GetOffset(this InventoryMenu menu)
        {
            return ModEntry.scrolled.Value * menu.capacity / menu.rows;
        }
        public static bool Scrolling(this InventoryMenu menu)
        {
            return menu.actualInventory is not null && menu.capacity < menu.actualInventory.Count;
        }
    }
}