using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace MapEdit
{
    public partial class ModEntry
    {
        private static bool MouseInMenu()
        {
            if (!modActive.Value)
                return false;
            if(tileMenu.Value is null)
            {
                tileMenu.Value = new();
            }
            if (tileMenu.Value.showing)
            {
                return Game1.getMouseX() < tileMenu.Value.width - IClickableMenu.spaceToClearSideBorder || TileSelectMenu.button.containsPoint(Game1.getMouseX(), Game1.getMouseY());
            }
            else
            {
                return TileSelectMenu.button.containsPoint(Game1.getMouseX(), Game1.getMouseY());
            }
        }
    }
}