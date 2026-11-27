using StardewValley;

namespace PersisitentGrangeDisplay
{
    public class PersistentItem
    {
        public int id;
        public int quality;

        public PersistentItem()
        {
        }
        public PersistentItem(Item item)
        {
            id = item.parentSheetIndex;
            quality = (item as Object).Quality;
        }
    }
}