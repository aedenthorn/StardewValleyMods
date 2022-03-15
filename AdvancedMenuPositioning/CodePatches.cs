using StardewValley;
using StardewValley.Menus;

namespace AdvancedMenuPositioning
{
    public partial class ModEntry
    {
        public static void InventoryMenu_leftClick_Prefix(InventoryMenu __instance, ref Item toPlace)
        {
            if (!Config.EnableMod)
                return;
            //toPlace = lastHeldItem;
        }
        public static void ItemGrabMenu_Change_HeldItem(ItemGrabMenu __instance)
        {
            if (!Config.EnableMod)
                return;
            lastHeldItem = __instance.heldItem;
        }
    }
}