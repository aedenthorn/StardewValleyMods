using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MobileAudioPlayer
{
    public class ModEntry : Mod 
	{
		public static ModEntry context;

		public static ModConfig Config;
        private Random myRand;
        private string[] audio;
        private IMobilePhoneApi api;
        private Vector2 screenPos;
        private Vector2 screenSize;
        private Texture2D backgroundTexture;
        private Point lastMousePosition;
        private bool dragging;
        private int offsetY;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

			myRand = new Random(Guid.NewGuid().GetHashCode());
            
            audio = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "audio"));

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
        }
        private void OpenAudioPlayer()
        {
            api.SetAppRunning(true);
            api.SetRunningApp(Helper.ModRegistry.ModID);
            Game1.activeClickableMenu = new AudioPlayerMenu();
            Helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;
            dragging = true;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (api.GetRunningApp() != Helper.ModRegistry.ModID)
                return;
            if(e.Button == SButton.MouseLeft)
            {
                if (Game1.activeClickableMenu is AudioPlayerMenu)
                {
                    dragging = false;
                    lastMousePosition = Game1.getMousePosition();
                }

            }
        }
        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                if (api.GetRunningApp() != Helper.ModRegistry.ModID)
                    return;
                if (dragging)
                    dragging = false;
                else
                {
                    if (Game1.activeClickableMenu is AudioPlayerMenu)
                    {
                        Point mousePos = Game1.getMousePosition();
                        if (api.GetScreenRectangle().Contains(mousePos))
                        {
                            ClickTrack(mousePos);
                        }
                    }
                }
            }
        }

        private void ClickTrack(Point mousePos)
        {
            
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                bool success = api.AddApp(Helper.ModRegistry.ModID, "Audio Player", OpenAudioPlayer, appIcon);
                Monitor.Log($"loaded app successfully: {success}", LogLevel.Debug);
            }
            MakeTextures();
        }

        private void MakeTextures()
        {
            screenSize = api.GetScreenSize();
            Texture2D background = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, (int)screenSize.Y);
            Color[] data = new Color[background.Width * background.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.BackgroundColor;
            }
            background.SetData(data);
            backgroundTexture = background;
        }

        private void Display_RenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            screenPos = api.GetScreenPosition();
            screenSize = api.GetScreenSize();
            if (!api.GetPhoneOpened() || api.GetRunningApp() != Helper.ModRegistry.ModID)
            {
                StopTrack(true);
                Helper.Events.Display.RenderedActiveMenu -= Display_RenderedActiveMenu;
                return;
            }

            if (Helper.Input.IsDown(SButton.MouseLeft))
            {
                Point mousePos = Game1.getMousePosition();
                if (mousePos.Y != lastMousePosition.Y)
                {
                    dragging = true;
                }
                if (dragging == true)
                {
                    offsetY += mousePos.Y - lastMousePosition.Y;
                    lastMousePosition = mousePos;
                }
            }


            e.SpriteBatch.Draw(backgroundTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, (int)screenSize.Y), Color.White);
            for(int i = 0; i < audio.Length; i++)
            {
                string a = audio[i];
                string lineOne = Config.ListLineOne;
                string lineTwo = Config.ListLineTwo;
                MakeListString(a, ref lineOne);
                MakeListString(a, ref lineTwo);
                float posY = screenPos.Y + Config.MarginY * (i + 1) + i * Game1.dialogueFont.LineSpacing * 0.6f + offsetY;
                e.SpriteBatch.DrawString(Game1.dialogueFont, lineOne, new Vector2(screenPos.X + Config.MarginX, posY), Config.LineOneColor, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0.86f);
                e.SpriteBatch.DrawString(Game1.dialogueFont, lineTwo, new Vector2(screenPos.X + Config.MarginX, posY + Game1.dialogueFont.LineSpacing * 0.4f), Config.LineTwoColor, 0f, Vector2.Zero, 0.2f, SpriteEffects.None, 0.86f);
            }
        }

        private void MakeListString(string a, ref string line)
        {
            a = new Regex(@"\.[^.]+$").Replace(a, "");
            string[] aa = a.Split('_');
            line = line.Replace("{name}", aa[0]);
            if(aa.Length > 1)
                line = line.Replace("{artist}", aa[1]);
            if (aa.Length > 2)
                line = line.Replace("{album}", aa[2]);
            
        }

        private void StopTrack(bool v)
        {
            throw new NotImplementedException();
        }
    }
}
