using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace MultiStorageMenu
{
    public partial class ModEntry
    {
        public static void OpenMenu()
        {
            if (Config.ModEnabled && Context.IsPlayerFree)
            {
                Game1.activeClickableMenu = new StorageMenu();
            }
        }
    }
}