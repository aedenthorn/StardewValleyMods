using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace MultiStorageMenu
{
    public partial class ModEntry
    {
        private void OpenMenu()
        {
            if (Config.ModEnabled && Context.IsPlayerFree)
            {
                Game1.activeClickableMenu = new StorageMenu();
            }
        }
        private void drawDialogueBox(int xPositionOnScreen, int yPositionOnScreen, int width, int height, bool v1, bool v2, object value, bool v3, bool v4)
        {
        }
    }
}