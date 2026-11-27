using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using System;
using System.Runtime.CompilerServices;
using xTile;
using xTile.Tiles;

namespace SGJigsaw
{
	public partial class ModEntry : Mod
    {
        private void DrawMenuSlot(SpriteBatch b, Rectangle rectangle)
        {
            var text = SHelper.Translation.Get("menu-text");
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Color.White, 4f, false, -1f);
            b.Draw(SHelper.ModContent.Load<Texture2D>("assets/menu.png"), new Rectangle(rectangle.Location + new Point(8, 8), rectangle.Size - new Point(16,16)), Color.White);
            SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.viewport.Width / 2 + 64, rectangle.Y + 64, color: Color.LightGoldenrodYellow);
        }

        private void LoadGame(Rectangle area, int x, int y)
        {
            if(Config.ModEnabled)
                TitleMenu.subMenu = new JigsawGameMenu(null);
        }

    }
}
