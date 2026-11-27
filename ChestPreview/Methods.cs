using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace ChestPreview
{
    public partial class ModEntry
    {
        private void ShowChestPreview(SpriteBatch spriteBatch, Chest chest, Vector2 tile)
        {
            int cap = chest.GetActualCapacity();
            int rows =  cap / 12;
            int columns = cap / rows;
            int width = columns * 64;
            int height = rows * 64;
            var posOnScreen = Game1.GlobalToLocal(tile * 64);
            Game1.drawDialogueBox((int)posOnScreen.X + 32 - width / 2 - 32, (int)posOnScreen.Y - height - 164, width + 64, height + 140, false, true);
            InventoryMenu menu = new InventoryMenu((int)posOnScreen.X + 32 - width / 2, (int)posOnScreen.Y - height - 64, false, chest.items, null, cap, rows, 0, 0, true);
            menu.draw(spriteBatch);
        }
    }
}