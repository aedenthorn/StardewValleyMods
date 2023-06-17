using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace AllChestsMenu
{
    public partial class ModEntry
    {
        public static void OpenMenu()
        {
            if (Config.ModEnabled && Context.IsPlayerFree)
            {
                Game1.activeClickableMenu = new StorageMenu();
                Game1.playSound("bigSelect");
            }
        }
    }
}