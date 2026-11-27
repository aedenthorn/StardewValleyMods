using StardewValley.Menus;

namespace StardewVN
{
    internal class VisualNovelMenu : IClickableMenu
    {
        private VisualNovelData visualNovelData;

        public VisualNovelMenu(VisualNovelData visualNovelData)
        {
            this.visualNovelData = visualNovelData;
        }
    }
}