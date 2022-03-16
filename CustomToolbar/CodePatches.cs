using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomToolbar
{
    public partial class ModEntry
    {
		public static void Toolbar_postfix(IClickableMenu __instance)
		{
			if (!Config.EnableMod || __instance is not Toolbar)
				return;
			__instance.xPositionOnScreen = Config.PositionX;
			__instance.yPositionOnScreen = Config.PositionY;
			return;
		}
        public static bool Toolbar_isWithinBounds_prefix(Toolbar __instance, int x, int y, ref bool __result)
        {
			if (!Config.EnableMod)
				return true;
			__result = x - __instance.xPositionOnScreen < __instance.width && x - __instance.xPositionOnScreen >= 0 && y - __instance.yPositionOnScreen < __instance.height && y - __instance.yPositionOnScreen >= 0;
			return false;
		}
        public static bool Toolbar_draw_prefix(Toolbar __instance, SpriteBatch b, ref float ___transparency, List<ClickableComponent> ___buttons, ref Item ___hoverItem)
        {
			if (!Config.EnableMod)
				return true;
			if (Game1.activeClickableMenu != null && !Config.ShowWithActiveMenu)
			{
				return false;
			}
			float scale = 1f;
			int breadth = (int)Math.Round(800 * scale);
			int girth = (int)Math.Round(96 * scale);
			Point playerGlobalPos = Game1.player.GetBoundingBox().Center;
			Vector2 playerGlobalVec = new Vector2(playerGlobalPos.X, playerGlobalPos.Y);
			Vector2 playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalVec);
			int marginX = Utility.makeSafeMarginX(8);
			int marginY = Utility.makeSafeMarginY(8);
			Point newPos = new Point(__instance.xPositionOnScreen, __instance.yPositionOnScreen);
			if (Game1.options.pinToolbarToggle)
			{
				___transparency = Math.Min(1f, ___transparency + 0.075f);
				bool transparent;
				switch (Config.PinnedPosition)
				{
					case "bottom":
						transparent = playerLocalVec.Y > Game1.viewport.Height - girth - Config.MarginY;
						__instance.yPositionOnScreen = Game1.uiViewport.Height - girth - Config.MarginY;
						__instance.yPositionOnScreen += 8;
						__instance.yPositionOnScreen -= marginY;
						__instance.xPositionOnScreen = Game1.uiViewport.Width / 2 - breadth / 2 + Config.OffsetX;
						break;
					case "left":
						transparent = playerLocalVec.X < 96 + Config.MarginX;
						__instance.xPositionOnScreen = Config.MarginX;
						__instance.xPositionOnScreen -= 8;
						__instance.xPositionOnScreen += marginY;
						__instance.yPositionOnScreen = Game1.uiViewport.Height / 2 - breadth / 2 + Config.OffsetY;
						break;
					case "right":
						transparent = playerLocalVec.X > Game1.viewport.Width - girth - Config.MarginX;
						__instance.xPositionOnScreen = Game1.uiViewport.Width - girth - Config.MarginX;
						__instance.xPositionOnScreen += 8;
						__instance.xPositionOnScreen -= marginY;
						__instance.yPositionOnScreen = Game1.uiViewport.Height / 2 - breadth / 2 + Config.OffsetY;
						break;
					case "top":
						transparent = playerLocalVec.Y < 96 + Config.MarginY;
						__instance.yPositionOnScreen = Config.MarginY;
						__instance.yPositionOnScreen -= 8;
						__instance.yPositionOnScreen += marginY;
						__instance.xPositionOnScreen = Game1.uiViewport.Width / 2 - breadth / 2 + Config.OffsetX;
						break;

					default:
						transparent = false;
						break;
				}
				if (transparent)
				{
					___transparency = Math.Max(0.33f, ___transparency - 0.15f);
				}
			}
			else if(__instance.xPositionOnScreen == -1 && __instance.yPositionOnScreen == -1)
			{
                if (Config.Vertical)
                {
					__instance.xPositionOnScreen = Config.MarginX;
					__instance.xPositionOnScreen -= 8;
					__instance.xPositionOnScreen += marginX;
					__instance.yPositionOnScreen = Game1.uiViewport.Height / 2 - breadth / 2 + Config.OffsetY;
				}
				else
                {
					__instance.yPositionOnScreen = Game1.uiViewport.Height - girth - Config.MarginY;
					__instance.yPositionOnScreen += 8;
					__instance.yPositionOnScreen -= marginY;
					__instance.xPositionOnScreen = Game1.uiViewport.Width / 2 - breadth / 2 + Config.OffsetX;
				}
			}
			else if(Config.Vertical)
			{
                if (Config.SetPosition)
                {
					if (playerLocalVec.X > Game1.viewport.Width / 2 + 32)
					{
						__instance.xPositionOnScreen = Config.MarginX;
						__instance.xPositionOnScreen -= 8;
						__instance.xPositionOnScreen += marginX;
					}
					else
					{
						__instance.xPositionOnScreen = Game1.uiViewport.Width - girth - Config.MarginX;
						__instance.xPositionOnScreen += 8;
						__instance.xPositionOnScreen -= marginX;
					}
					__instance.yPositionOnScreen = Game1.uiViewport.Height / 2 - breadth / 2 + Config.OffsetY;
				}
				___transparency = 1f;
			}
			else
			{
				if (Config.SetPosition)
				{
					if (playerLocalVec.Y > Game1.viewport.Height / 2 + 64)
					{
						__instance.yPositionOnScreen = Config.MarginY;
						__instance.yPositionOnScreen -= 8;
						__instance.yPositionOnScreen += marginY;
					}
                    else
                    {
						__instance.yPositionOnScreen = Game1.uiViewport.Height - girth - Config.MarginY;
						__instance.yPositionOnScreen += 8;
						__instance.yPositionOnScreen -= marginY;
					}
					__instance.xPositionOnScreen = Game1.uiViewport.Width / 2 - breadth / 2 + Config.OffsetX;
					__instance.xPositionOnScreen -= 8;
					__instance.xPositionOnScreen += marginX;
				}
				___transparency = 1f;

			}
            if (Config.Vertical)
            {
				__instance.width = girth;
				__instance.height = breadth;
			}
            else
            {
				__instance.width = breadth;
				__instance.height = girth;
			}
            if (!Config.SetPosition)
            {
				if(__instance.xPositionOnScreen != Config.PositionX || Config.PositionY != __instance.yPositionOnScreen)
                {
					Config.PositionX = __instance.xPositionOnScreen;
					Config.PositionY = __instance.yPositionOnScreen;
					SHelper.WriteConfig(Config);
				}
			}

			IClickableMenu.drawTextureBox(b, Game1.menuTexture, __instance.toolbarTextSource, __instance.xPositionOnScreen, __instance.yPositionOnScreen, (int)Math.Round(scale * (Config.Vertical ? 96 : 800)), (int)Math.Round(scale * (Config.Vertical ? 800 : 96)), Color.White * ___transparency, 1f, false, -1f);
			for (int i = 0; i < 12; i++)
			{
				Vector2 toDraw;
				if (Config.Vertical)
				{
					toDraw = new Vector2(__instance.xPositionOnScreen + 16 * scale, __instance.yPositionOnScreen + (16 + i * 64) * scale);
				}
				else
				{
					toDraw = new Vector2(__instance.xPositionOnScreen + (16 + i * 64)  * scale, __instance.yPositionOnScreen + 16 * scale);
				}
				b.Draw(Game1.menuTexture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, (Game1.player.CurrentToolIndex == i) ? 56 : 10, -1, -1)), Color.White * ___transparency, 0, Vector2.Zero, scale, SpriteEffects.None, 0.87f);

				if (!Game1.options.gamepadControls)
				{
					b.DrawString(Game1.tinyFont, __instance.slotText[i], toDraw + new Vector2(4f, -8f), Color.DimGray * ___transparency, 0, Vector2.Zero, scale, SpriteEffects.None, 0.88f);
				}
			}
			for (int i = 0; i < 12; i++)
			{
				Vector2 toDraw;
				Rectangle rect;
				if (Config.Vertical)
                {
					rect = new Rectangle((int)Math.Round(__instance.xPositionOnScreen + 16 * scale), (int)Math.Round(__instance.yPositionOnScreen + (16 + i * 64) * scale), (int)Math.Round(64 * scale), (int)Math.Round(64 * scale));
					toDraw = new Vector2(__instance.xPositionOnScreen + 16 * scale, __instance.yPositionOnScreen + (16 + i * 64) * scale);
				}
				else
                {
					rect = new Rectangle((int)Math.Round(__instance.xPositionOnScreen + (16 + i * 64) * scale), (int)Math.Round(__instance.yPositionOnScreen + 16 * scale), (int)Math.Round(64 * scale), (int)Math.Round(64 * scale));
					toDraw = new Vector2(__instance.xPositionOnScreen + (16 + i * 64) * scale, __instance.yPositionOnScreen + 16 * scale);
				}
				___buttons[i].bounds = rect;
				___buttons[i].scale = Math.Max(1f, ___buttons[i].scale - 0.025f);
				if (Game1.player.Items.Count > i && Game1.player.Items.ElementAt(i) != null)
				{
					/*
					var ptr = AccessTools.Method(typeof(Tool), "drawInMenu", new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float) }).MethodHandle.GetFunctionPointer();
					var baseMethod = (Func<SpriteBatch, Vector2, float, float, float, Item>)Activator.CreateInstance(typeof(Func<SpriteBatch, Vector2, float, float, float, Item>), __instance, ptr);
					baseMethod(b, toDraw, ((Game1.player.CurrentToolIndex == i) ? 0.9f : (___buttons.ElementAt(i).scale * 0.8f)) * scale, ___transparency, 0.88f);
					*/
					Game1.player.Items[i].drawInMenu(b, toDraw, ((Game1.player.CurrentToolIndex == i) ? 0.9f : (___buttons.ElementAt(i).scale * 0.8f)) * scale, ___transparency, 0.88f);
				}
			}
			if (___hoverItem != null)
			{
				IClickableMenu.drawToolTip(b, ___hoverItem.getDescription(), ___hoverItem.DisplayName, ___hoverItem, false, -1, 0, -1, -1, null, -1);
				___hoverItem = null;
			}
			return false;
		}
    }
}