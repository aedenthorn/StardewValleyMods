using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace CatalogueFilter
{
	public partial class ModEntry
	{
		private static string lastFilterString = "";
		private static TextBox filterField;
		private static List<ISalable> allItems;

		public class ShopMenu_Patch
		{
			public static void Postfix(ShopMenu __instance)
			{
				if (!Config.ModEnabled)
					return;

				allItems = new List<ISalable>(__instance.forSale);
				filterField = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
				{
					X = __instance.xPositionOnScreen + 28,
					Y = __instance.yPositionOnScreen + __instance.height - 88,
					Text = ""
				};
			}
		}

		public class ShopMenu_drawCurrency_Patch
		{
			public static void Postfix(ShopMenu __instance, SpriteBatch b)
			{
				if (!Config.ModEnabled)
					return;

				if (lastFilterString != filterField.Text)
				{
					lastFilterString = filterField.Text;
					foreach (ISalable i in __instance.forSale)
					{
						if (!allItems.Contains(i))
						{
							allItems.Add(i);
						}
					}
					for (int i = allItems.Count - 1; i >= 0; i--)
					{
						if (!__instance.itemPriceAndStock.ContainsKey(allItems[i]))
						{
							allItems.RemoveAt(i);
						}
					}
					__instance.forSale.Clear();
					if (filterField.Text == "")
					{
						__instance.forSale.AddRange(allItems);
					}
					else
					{
						foreach (ISalable i in allItems)
						{
							if (__instance.itemPriceAndStock.ContainsKey(i) && i.DisplayName.ToLower().Contains(filterField.Text.ToLower()))
							{
								__instance.forSale.Add(i);
							}
						}
						__instance.currentItemIndex = 0;
						__instance.gameWindowSizeChanged(Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.graphics.GraphicsDevice.Viewport.Bounds);
					}
				}
				filterField.Draw(b);
				if (Config.ShowLabel)
				{
					SpriteText.drawStringHorizontallyCenteredAt(b, SHelper.Translation.Get("filter"), __instance.xPositionOnScreen + 128, __instance.yPositionOnScreen + __instance.height - 136, 999999, -1, 999999, 1, 0.88f, false, Config.LabelColor, 99999);
				}
			}
		}

		public class ShopMenu_receiveLeftClick_Patch
		{
			public static void Postfix()
			{
				if (!Config.ModEnabled)
					return;

				filterField.Update();
			}
		}

		public class ShopMenu_receiveKeyPress_Patch
		{
			public static bool Prefix(Keys key)
			{
				if (!Config.ModEnabled || !filterField.Selected || key == Keys.Escape)
					return true;

				return false;
			}
		}

		public class ShopMenu_performHoverAction_Patch
		{
			public static void Postfix(int x, int y)
			{
				if (!Config.ModEnabled)
					return;

				filterField.Hover(x, y);
			}
		}
	}
}
