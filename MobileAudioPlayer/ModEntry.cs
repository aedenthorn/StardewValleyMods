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
using System.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        private SoundPlayer soundPlayer = new SoundPlayer();
        private string state = "stopped";
        private int trackPlaying;
        private Texture2D hightlightTexture;

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
            Helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;
            dragging = true;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (api.GetRunningApp() != Helper.ModRegistry.ModID)
                return;
            if(e.Button == SButton.MouseLeft)
            {
                if (!api.GetScreenRectangle().Contains(Game1.getMousePosition()))
                {
                    api.SetAppRunning(false);
                    api.SetRunningApp(null);
                    Game1.activeClickableMenu = null;
                    StopTrack();
                    Helper.Input.Suppress(SButton.MouseLeft);
                }

                if (Game1.activeClickableMenu is AudioPlayerMenu)
                {
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
            int idx = (int)((mousePos.Y - api.GetScreenPosition().Y - Config.MarginY - offsetY) / (Config.MarginY + Game1.dialogueFont.LineSpacing * (Config.LineOneScale + Config.LineTwoScale)));
            Monitor.Log($"index: {idx}");
            if (idx < audio.Length && idx >= 0)
            {
                soundPlayer.Stop();
                soundPlayer.SoundLocation = audio[idx];
                PlayPlayer();
                state = "playing";
                trackPlaying = idx;
            }
        }

        private async void PlayPlayer()
        {
            while(api.GetAppRunning() && api.GetRunningApp() == Helper.ModRegistry.ModID)
            {
                await Task.Run(() => soundPlayer.PlaySync());
                if (!Config.PlayAll)
                    return;
                trackPlaying++;
                if(trackPlaying >= audio.Length && !Config.LoopPlaylist)
                    return;
                trackPlaying %= audio.Length;
                soundPlayer.SoundLocation = audio[trackPlaying];
            }
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
            background = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, (int)(Game1.dialogueFont.LineSpacing * (Config.LineOneScale + Config.LineTwoScale)));
            data = new Color[background.Width * background.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.HighlightColor;
            }
            background.SetData(data);
            hightlightTexture = background;
        }

        private void Display_RenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            float itemHeight = Game1.dialogueFont.LineSpacing * (Config.LineOneScale + Config.LineTwoScale);

            screenPos = api.GetScreenPosition();
            screenSize = api.GetScreenSize();
            if (!api.GetPhoneOpened() || api.GetRunningApp() != Helper.ModRegistry.ModID)
            {
                StopTrack(true);
                Helper.Events.Display.RenderingActiveMenu -= Display_RenderingActiveMenu;
                return;
            }

            if (Helper.Input.IsDown(SButton.MouseLeft))
            {
                Point mousePos = Game1.getMousePosition();
                if (mousePos.Y != lastMousePosition.Y)
                {
                    dragging = true;
                    offsetY += mousePos.Y - lastMousePosition.Y;
                    //Monitor.Log($"offsetY {offsetY} max {screenSize.Y - Config.MarginY + (Config.MarginY + Game1.dialogueFont.LineSpacing * 0.9f) * audio.Length}");
                    offsetY = Math.Min(0, Math.Max(offsetY, (int)(screenSize.Y - (Config.MarginY + (Config.MarginY + itemHeight) * audio.Length))));
                    lastMousePosition = mousePos;
                }
            }


            e.SpriteBatch.Draw(backgroundTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, (int)screenSize.Y), Color.White);
            for(int i = 0; i < audio.Length; i++)
            {
                string a = Path.GetFileName(audio[i]);
                string lineOne = Config.ListLineOne;
                string lineTwo = Config.ListLineTwo;
                MakeListString(a, ref lineOne);
                MakeListString(a, ref lineTwo);
                float posY = screenPos.Y + Config.MarginY * (i + 1) + i * itemHeight + offsetY;
                if(i == trackPlaying && posY > screenPos.Y - itemHeight && posY < screenPos.Y + screenSize.Y)
                {
                    float backPosY = posY;
                    int cutTop = 0;
                    int cutBottom = 0;
                    
                    if (posY < screenPos.Y)
                    {
                        cutTop = (int)(screenPos.Y - posY);
                        backPosY = screenPos.Y;
                    }
                    if (posY > screenPos.Y + screenSize.Y - itemHeight)
                        cutBottom = (int)(posY - screenPos.Y + screenSize.Y - itemHeight);
                    Rectangle r = new Rectangle((int)screenPos.X, (int)backPosY, (int)screenSize.X, (int)(itemHeight) - cutTop - cutBottom);
                    e.SpriteBatch.Draw(hightlightTexture, r, Color.White);
                }
                if (posY > screenPos.Y && posY < screenPos.Y + screenSize.Y - Game1.dialogueFont.LineSpacing * Config.LineTwoScale)
                    e.SpriteBatch.DrawString(Game1.dialogueFont, lineOne, new Vector2(screenPos.X + Config.MarginX, posY), Config.LineOneColor, 0f, Vector2.Zero, Config.LineOneScale, SpriteEffects.None, 0.86f);
                if (posY > screenPos.Y - Game1.dialogueFont.LineSpacing * Config.LineOneScale && posY < screenPos.Y + screenSize.Y - itemHeight)
                    e.SpriteBatch.DrawString(Game1.dialogueFont, lineTwo, new Vector2(screenPos.X + Config.MarginX, posY + Game1.dialogueFont.LineSpacing * Config.LineOneScale), Config.LineTwoColor, 0f, Vector2.Zero, Config.LineTwoScale, SpriteEffects.None, 0.86f);
            }
        }

        private void MakeListString(string a, ref string line)
        {
            a = new Regex(@"\.[^.]+$").Replace(a, "");
            string[] aa = a.Split('_');
            for(int i = 0; i < aa.Length; i++)
            {
                line = line.Replace("{"+ (i + 1) + "}", aa[i]);
            }

        }

        private void StopTrack(bool force = false)
        {
            soundPlayer.Stop();
        }
    }
}
