using StardewValley.Menus;

namespace CustomBackpack
{
    public interface ICustomBackpackApi
    {
        public bool SetPlayerSlots(int slots, bool force);
        public bool ChangeScroll(InventoryMenu menu, int delta);
    }
    public class CustomBackpackApi
    {
        public bool SetPlayerSlots(int slots, bool force)
        {
            return ModEntry.SetPlayerSlots(slots, force);
        }
        public bool ChangeScroll(InventoryMenu menu, int delta)
        {
            return ModEntry.ChangeScroll(menu, delta);
        }
    }
}