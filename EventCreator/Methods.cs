using StardewValley;

namespace EventCreator
{
    public partial class ModEntry
    {
        public static void OpenMenu()
        {
            Game1.activeClickableMenu = new EventCreatorMenu();
        }
   }
}