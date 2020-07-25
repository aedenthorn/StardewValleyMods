using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace VideoPlayerMod
{
    public class ModEntry : Mod
	{
		public static ModEntry context;
        public static ModConfig Config;
        public int currentTrack = 0;
        private List<Video> videos = new List<Video>();
        public VideoPlayer videoPlayer = new VideoPlayer();
        public Video currentVideo;
        private string[] videoFiles;
        private IMobilePhoneApi api;

        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

            string path = Path.Combine(Helper.DirectoryPath, "assets");
            if (!Directory.Exists(path))
            {
                Monitor.Log($"Assets folder not found. No videos will be loaded.", LogLevel.Warn);
                return;
            }
            videoFiles = Directory.GetFiles(path, "*.xnb");
            if (videoFiles.Length == 0)
            {
                Monitor.Log($"No videos found to play.", LogLevel.Warn);
                return;
            }
            Monitor.Log($"Loaded {videoFiles.Length} videos.", LogLevel.Debug);

            foreach(string v in videoFiles)
            {
                try
                {
                    string videoPath = Path.Combine("assets", Path.GetFileName(v));
                    videos.Add(Helper.Content.Load<Video>(videoPath)); 
                }
                catch
                {

                }
            }

            SetVideo(0);

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.Rendered += Display_Rendered;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (Config.PhoneApp)
            {
                api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
                if (api != null)
                {
                    Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                    bool success = api.AddApp(Helper.ModRegistry.ModID, "Video Player", OpenVideoPlayer, appIcon);
                    Monitor.Log($"loaded phone app successfully: {success}", LogLevel.Debug);
                }
            }
        }

        private void OpenVideoPlayer()
        {
            if (Config.PhoneApp && api != null)
            {
                if (api.GetAppRunning())
                {
                    Monitor.Log($"can't start, app already running", LogLevel.Debug);
                    return;
                }
                api.SetAppRunning(true);
                PlayTrack();
            }
        }

        private void SetVideo(int idx)
        {
            currentVideo = videos[idx];
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.NumPad4)
            {
                if (Helper.Input.IsDown(SButton.LeftControl))
                    Config.XOffset = Math.Max(0, Config.XOffset + (Config.RightSide ? -5 : 5));
                else
                    SetTrack(false);
            }
            else if (e.Button == SButton.NumPad6)
            {
                if (Helper.Input.IsDown(SButton.LeftControl))
                    Config.XOffset = Math.Min(Game1.viewport.Width - Config.Width, Config.XOffset - (Config.RightSide ? -5 : 5));
                else
                    SetTrack(true);
            }
            else if (e.Button == SButton.NumPad8)
            {
                if (Helper.Input.IsDown(SButton.LeftControl))
                    Config.YOffset = Math.Max(0, Config.YOffset + (Config.Bottom? -5 : 5));
                else
                    PlayTrack();
            }
            else if (e.Button == SButton.NumPad2)
            {
                if (Helper.Input.IsDown(SButton.LeftControl))
                    Config.YOffset = Math.Min(Game1.viewport.Height - Config.Height, Config.YOffset - (Config.Bottom ? -5 : 5));
                else
                    StopTrack();
            }
        }

        private void StopTrack()
        {
            if (videoPlayer.State == MediaState.Playing)
                videoPlayer.Pause();
            else
                videoPlayer.Stop();

            if (Config.PhoneApp && api != null)
                api.SetAppRunning(false);
        }

        private void PlayTrack()
        {
            if (Config.PhoneApp && api != null)
            {
                if(!api.GetPhoneOpened())
                {
                    Monitor.Log($"can't start, phone not open", LogLevel.Debug);
                    return;
                }
                Monitor.Log($"ready to run app");
            }
            Monitor.Log($"playing video {videoFiles[currentTrack]}");
            videoPlayer.Play(currentVideo);

        }

        private void SetTrack(bool next)
        {
            if (next)
            {
                currentTrack++;
                currentTrack %= videoFiles.Length;
            }
            else
            {
                currentTrack--;
                if (currentTrack < 0)
                    currentTrack = videoFiles.Length - 1;
            }
            SetVideo(currentTrack);
            videoPlayer.Play(currentVideo);
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (videoPlayer.State != MediaState.Playing)
                return;

            if (Config.PhoneApp && api != null && (!api.GetPhoneOpened() || !api.GetAppRunning()))
            {
                StopTrack();
                return;
            }

            Texture2D texture = videoPlayer.GetTexture();
            if (texture != null)
            {
                if (Config.PhoneApp && api != null)
                {
                    Vector2 pos = api.GetScreenPosition();
                    Vector2 size = api.GetScreenSize();
                    e.SpriteBatch.Draw(texture, new Rectangle((int)pos.X, (int)pos.Y, (int) size.X, (int)size.Y), Color.White);
                }
                else
                {
                    int x = Config.RightSide ? Game1.viewport.Width - Config.Width - Config.XOffset : Config.XOffset;
                    int y = Config.Bottom ? Game1.viewport.Height - Config.Height - Config.YOffset : Config.YOffset;
                    e.SpriteBatch.Draw(texture, new Rectangle(x, y, Config.Width, Config.Height), Color.White);
                }
            }
        }
    }
}
