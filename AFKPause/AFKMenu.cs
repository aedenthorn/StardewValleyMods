using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;

namespace AFKPause
{
	public class AFKMenu : IClickableMenu
	{
		public override void draw(SpriteBatch b)
		{
			if (ModEntry.Config.ShowAFKText)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, ModEntry.Config.AFKText, Game1.viewport.Width / 2, Game1.viewport.Height / 2);
			}
		}
	}
}
