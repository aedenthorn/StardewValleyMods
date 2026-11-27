using StardewValley;
using StardewValley.Menus;

namespace DoorFurniture
{
    public class KeyNamingMenu : NamingMenu
    {
        private static Object key;
        private static string id;
        public KeyNamingMenu(Object obj, string guid, string name) : base(new NamingMenu.doneNamingBehavior(KeyNamingMenu.NameKey), ModEntry.SHelper.Translation.Get("name-menu"), name)
        {
            key = obj;
            id = guid;
            textBox.limitWidth = false;
        }
        public static void NameKey(string name)
        {
            key.displayName = name;
            key.modData[ModEntry.nameKey] = name;
            key.modData[ModEntry.guidKey] = $"{id}={name}";
            Game1.exitActiveMenu();
        }
    }
}