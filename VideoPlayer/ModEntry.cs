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

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
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

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.Rendered += Display_Rendered;
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
        }

        private void PlayTrack()
        {
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

            Texture2D texture = videoPlayer.GetTexture();
            if (texture != null)
            {
                int x = Config.RightSide ? Game1.viewport.Width - Config.Width - Config.XOffset : Config.XOffset;
                int y = Config.Bottom ? Game1.viewport.Height - Config.Height - Config.YOffset : Config.YOffset;
                e.SpriteBatch.Draw(texture, new Rectangle(x, y, Config.Width, Config.Height), Color.White);
            }
        }

    }
}
