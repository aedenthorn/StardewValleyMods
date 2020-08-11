using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;

namespace MobileCatalogues
{
    public class Visuals
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;
        private static IMobilePhoneApi api;
        private static Texture2D backgroundTexture;
        private static Texture2D hightlightTexture;
        private static Texture2D greyedTexture;
        private static Texture2D headerTexture;
        private static Texture2D coinTexture;
        public static bool clicking;
        private static bool dragging;
        public static Point lastMousePosition;
        public static float offsetY;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static void MakeTextures()
        {
            api = ModEntry.api;
            Vector2 screenSize = api.GetScreenSize();
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, (int)screenSize.Y);
            Color[] data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.BackgroundColor;
            }
            texture.SetData(data);
            backgroundTexture = texture;
            texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, Config.AppRowHeight);
            Texture2D texture2 = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, Config.AppRowHeight);
            data = new Color[texture.Width * texture.Height];
            Color[] data2 = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.HighlightColor;
                data2[pixel] = Config.GreyedColor;
            }
            texture.SetData(data);
            texture2.SetData(data2);
            hightlightTexture = texture;
            greyedTexture = texture2;

            texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, Config.AppHeaderHeight);
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.HeaderColor;
            }
            texture.SetData(data);
            headerTexture = texture;
            coinTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets", "coin.png"));

        }

        public static void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {

            if (api.IsCallingNPC())
                return;

            Vector2 screenPos = api.GetScreenPosition();
            Vector2 screenSize = api.GetScreenSize();
            if (!api.GetPhoneOpened() || !api.GetAppRunning() || api.GetRunningApp() != Helper.ModRegistry.ModID)
            {
                Monitor.Log($"Closing app: phone opened {api.GetPhoneOpened()} app running {api.GetAppRunning()} running app {api.GetRunningApp()}");
                CataloguesApp.CloseApp();
                return;
            }

            if (!clicking)
                dragging = false;

            Point mousePos = Game1.getMousePosition();
            if (clicking)
            {
                if (mousePos.Y != lastMousePosition.Y && (dragging || api.GetScreenRectangle().Contains(mousePos)))
                {
                    dragging = true;
                    offsetY += mousePos.Y - lastMousePosition.Y;
                    //Monitor.Log($"offsetY {offsetY} max {screenSize.Y - Config.MarginY + (Config.MarginY + Game1.dialogueFont.LineSpacing * 0.9f) * audio.Length}");
                    offsetY = Math.Min(0, Math.Max(offsetY, (int)(screenSize.Y - (Config.AppHeaderHeight + Config.MarginY + (Config.MarginY + Config.AppRowHeight) * CataloguesApp.catalogueList.Count))));
                    lastMousePosition = mousePos;
                }
            }

            if (clicking && !Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                Monitor.Log($"unclicking; dragging = {dragging}");
                if (dragging)
                    dragging = false;
                else if (api.GetScreenRectangle().Contains(mousePos) && !new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, Config.AppHeaderHeight).Contains(mousePos))
                {
                    CataloguesApp.ClickRow(mousePos);
                }
                clicking = false;
            }


            e.SpriteBatch.Draw(backgroundTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, (int)screenSize.Y), Color.White);
            for (int i = 0; i < CataloguesApp.catalogueList.Count; i++)
            {
                string a = Helper.Translation.Get(CataloguesApp.catalogueList[i]);
                float posY = screenPos.Y + Config.AppHeaderHeight + Config.MarginY * (i + 1) + i * Config.AppRowHeight + offsetY;
                Rectangle r = new Rectangle((int)screenPos.X, (int)posY, (int)screenSize.X, Config.AppRowHeight);
                bool bought = !Config.RequireCataloguePurchase || Game1.player.mailReceived.Contains($"BoughtCatalogue{a}");
                if (!bought)
                {
                    float backPosY = posY;
                    int cutTop = 0;
                    int cutBottom = 0;

                    if (posY < screenPos.Y + Config.AppHeaderHeight)
                    {
                        cutTop = (int)(screenPos.Y - posY);
                        backPosY = screenPos.Y;
                    }
                    if (posY > screenPos.Y + screenSize.Y - Config.AppRowHeight)
                        cutBottom = (int)(posY - screenPos.Y + screenSize.Y - Config.AppRowHeight);
                    r = new Rectangle((int)screenPos.X, (int)backPosY, (int)screenSize.X, (int)(Config.AppRowHeight) - cutTop - cutBottom);
                    if (!r.Contains(mousePos))
                        e.SpriteBatch.Draw(greyedTexture, r, Color.White);
                }

                if(r.Contains(mousePos))
                    e.SpriteBatch.Draw(hightlightTexture, r, Color.White);

                float textHeight = Game1.dialogueFont.MeasureString(a).Y * Config.TextScale;
                if (posY > screenPos.Y && posY < screenPos.Y + screenSize.Y - Config.AppRowHeight / 2f + textHeight / 2f)
                {
                    e.SpriteBatch.DrawString(Game1.dialogueFont, a, new Vector2(screenPos.X + Config.MarginX, posY + Config.AppRowHeight / 2f - textHeight / 2f), Config.TextColor, 0f, Vector2.Zero, Config.TextScale, SpriteEffects.None, 0.86f);
                    if (!bought)
                    {
                        string cost = ""+CataloguesApp.GetCataloguePrice(CataloguesApp.catalogueList[i]);
                        Vector2 buySize = Game1.dialogueFont.MeasureString(cost) * Config.TextScale;
                        e.SpriteBatch.DrawString(Game1.dialogueFont, cost, new Vector2(screenPos.X + screenSize.X - buySize.X - Config.AppRowHeight * 3 / 4, posY + Config.AppRowHeight / 2f - buySize.Y / 2f), Config.TextColor, 0f, Vector2.Zero, Config.TextScale, SpriteEffects.None, 0.86f);
                        e.SpriteBatch.Draw(coinTexture, new Rectangle((int)(screenPos.X + screenSize.X - Config.AppRowHeight * 3 / 4f), (int)(posY + Config.AppRowHeight / 4f), (int)(Config.AppRowHeight / 2f), (int)(Config.AppRowHeight / 2f)), Color.White);
                    }
                }
            }
            e.SpriteBatch.Draw(headerTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, Config.AppHeaderHeight), Color.White);
            float headerTextHeight = Game1.dialogueFont.MeasureString(Helper.Translation.Get("catalogues")).Y * Config.HeaderTextScale;
            Vector2 xSize = Game1.dialogueFont.MeasureString("x") * Config.HeaderTextScale;
            e.SpriteBatch.DrawString(Game1.dialogueFont, Helper.Translation.Get("catalogues"), new Vector2(screenPos.X + Config.MarginX, screenPos.Y + Config.AppHeaderHeight / 2f - headerTextHeight / 2f), Config.HeaderTextColor, 0f, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, "x", new Vector2(screenPos.X + screenSize.X - Config.AppHeaderHeight / 2f - xSize.X / 2f, screenPos.Y + Config.AppHeaderHeight / 2f - xSize.Y / 2f), Config.HeaderTextColor, 0f, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
        }
    }
}
