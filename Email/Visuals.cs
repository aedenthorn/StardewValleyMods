using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;

namespace Email
{
    public partial class ModEntry
    {
        private static Texture2D backgroundTexture;
        private static Texture2D hightlightTexture;
        private static Texture2D headerTexture;
        public static bool clicking;
        private static bool dragging;
        public static Point lastMousePosition;
        public static float offsetY;

        public void MakeTextures()
        {
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
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.HighlightColor;
            }
            texture.SetData(data);
            hightlightTexture = texture;

            texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, Config.AppHeaderHeight);
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.HeaderColor;
            }
            texture.SetData(data);
            headerTexture = texture;

        }

        public void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {

            if (api.IsCallingNPC())
                return;

            Vector2 screenPos = api.GetScreenPosition();
            Vector2 screenSize = api.GetScreenSize();
            if (!api.GetPhoneOpened() || !api.GetAppRunning() || api.GetRunningApp() != Helper.ModRegistry.ModID)
            {
                Monitor.Log($"Closing app: phone opened {api.GetPhoneOpened()} app running {api.GetAppRunning()} running app {api.GetRunningApp()}");
                CloseApp();
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
                    offsetY = Math.Min(0, Math.Max(offsetY, (int)(screenSize.Y - (Config.AppHeaderHeight + Config.MarginY + (Config.MarginY + Config.AppRowHeight) * Game1.player.mailbox.Count))));
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
                    ClickRow(mousePos);
                }
                clicking = false;
            }


            e.SpriteBatch.Draw(backgroundTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, (int)screenSize.Y), Color.White);
            for (int i = 0; i < Game1.player.mailbox.Count; i++)
            {
                string a = Game1.player.mailbox[i];
                float posY = screenPos.Y + Config.AppHeaderHeight + Config.MarginY * (i + 1) + i * Config.AppRowHeight + offsetY;
                Rectangle r = new Rectangle((int)screenPos.X, (int)posY, (int)screenSize.X, Config.AppRowHeight);

                if(r.Contains(mousePos))
                    e.SpriteBatch.Draw(hightlightTexture, r, Color.White);

                float textHeight = Game1.dialogueFont.MeasureString(a).Y * Config.TextScale;
                if (posY > screenPos.Y && posY < screenPos.Y + screenSize.Y - Config.AppRowHeight / 2f + textHeight / 2f)
                {
                    e.SpriteBatch.DrawString(Game1.dialogueFont, a, new Vector2(screenPos.X + Config.MarginX, posY + Config.AppRowHeight / 2f - textHeight / 2f), Config.TextColor, 0f, Vector2.Zero, Config.TextScale, SpriteEffects.None, 0.86f);
                }
            }
            e.SpriteBatch.Draw(headerTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, Config.AppHeaderHeight), Color.White);
            float headerTextHeight = Game1.dialogueFont.MeasureString(Helper.Translation.Get("email")).Y * Config.HeaderTextScale;
            Vector2 xSize = Game1.dialogueFont.MeasureString("x") * Config.HeaderTextScale;
            e.SpriteBatch.DrawString(Game1.dialogueFont, Helper.Translation.Get("email"), new Vector2(screenPos.X + Config.MarginX, screenPos.Y + Config.AppHeaderHeight / 2f - headerTextHeight / 2f), Config.HeaderTextColor, 0f, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, "x", new Vector2(screenPos.X + screenSize.X - Config.AppHeaderHeight / 2f - xSize.X / 2f, screenPos.Y + Config.AppHeaderHeight / 2f - xSize.Y / 2f), Config.HeaderTextColor, 0f, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
        }
    }
}
