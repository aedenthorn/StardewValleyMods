using AsfMojo.Parsing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using StardewModdingAPI;
using StardewValley;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VideoPlayerMod
{
    public class ModEntry : Mod
    {
        public static ModEntry context;
        public static ModConfig Config;
        public int currentTrack = 0;
        public VideoPlayer videoPlayer = new VideoPlayer();
        public Video currentVideo;
        private string[] videoFiles;
        private IMobilePhoneApi api;
        private Texture2D backgroundTexture;
        private Texture2D xTexture;
        private Texture2D buttonsTexture;
        private Texture2D playTexture;
        private int uiTicks = 0;
        private Texture2D lastTexture;
        private Vector2 screenPos;
        private Vector2 screenSize;

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
            videoFiles = Directory.GetFiles(path, "*.wmv");
            if (videoFiles.Length == 0)
            {
                Monitor.Log($"No videos found to play.", LogLevel.Warn);
                return;
            }
            Monitor.Log($"Loaded {videoFiles.Length} videos.", LogLevel.Debug);

            SetVideo(0);
            MakeTextures();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.Rendered += Display_Rendered;
        }
        public bool TryLoadFromWMV(string filePath, out Video video)
        {
            video = null;
            Monitor.Log($"Loading wmv: {filePath}");

            using (AsfMojo.File.AsfFile asfFile = new AsfMojo.File.AsfFile(filePath))
            {
                int duration = (int)asfFile.PacketConfiguration.Duration * 1000, width = asfFile.PacketConfiguration.ImageWidth, height = asfFile.PacketConfiguration.ImageHeight;
                Monitor.Log($"Duration: {duration}");
                if (asfFile.GetAsfObjectByType(AsfGuid.ASF_Metadata_Object).FirstOrDefault() is AsfMetadataObject metadataObject)
                {
                    foreach(AsfMetadataObject o in asfFile.GetAsfObjectByType(AsfGuid.ASF_Metadata_Object))
                    {
                        Monitor.Log($"object: {o.Name}"); 
                        foreach (AsfProperty p in o.DescriptionRecords)
                        {
                            Monitor.Log($"property: {p.Name}");
                        }
                    }
                    ConstructorInfo videoConstructor = typeof(Video).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(GraphicsDevice), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(VideoSoundtrackType) }, null);
                    Monitor.Log($"Constructor: {videoConstructor != null}");
                    if (videoConstructor?.Invoke(new object[] { Game1.graphics.GraphicsDevice, filePath, duration, width, height, -1, VideoSoundtrackType.MusicAndDialog }) is Video v)
                    {
                        video = v;
                        Monitor.Log($"loaded video: {video != null}");
                    }
                }
            }

            return video is Video;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (Config.PhoneApp)
            {
                api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
                if (api != null)
                {
                    Texture2D appIcon = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                    bool success = api.AddApp(Helper.ModRegistry.ModID, Helper.Translation.Get("app-name"), OpenVideoPlayer, appIcon);
                    Monitor.Log($"loaded phone app successfully: {success}", LogLevel.Debug);
                }
            }
        }

        public void MakeTextures()
        {
            Texture2D background = new Texture2D(Game1.graphics.GraphicsDevice, (int)Config.Width, (int)Config.Height);
            Color[] data = new Color[background.Width * background.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Color.Black;
            }
            background.SetData(data);
            backgroundTexture = background;

            xTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "x.png"));
            buttonsTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "buttons.png"));
            playTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "play.png"));
        }

        private void OpenVideoPlayer()
        {
            if (Config.PhoneApp && api != null)
            {
                if (api.GetAppRunning() && api.GetRunningApp() != Helper.ModRegistry.ModID)
                {
                    Monitor.Log($"can't start, app already running", LogLevel.Debug);
                    return;
                }
                api.SetAppRunning(true);
                api.SetRunningApp(Helper.ModRegistry.ModID);
                Helper.Events.Display.Rendered += Display_Rendered;
                PlayTrack();
            }
        }

        private void SetVideo(int idx)
        {
            if(!TryLoadFromWMV(videoFiles[idx], out currentVideo))
            {
                Monitor.Log($"Error loading video file {videoFiles[idx]}", LogLevel.Error);
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Config.PhoneApp)
            {
                if(api == null || !api.GetPhoneOpened() || api.GetRunningApp() != Helper.ModRegistry.ModID)
                    return;
            }

            if(e.Button == SButton.MouseLeft && (Config.PhoneApp || videoPlayer.State != MediaState.Stopped))
            {
                SetScreenPosAndSize();

                Point mousePos = Game1.getMousePosition();
                Vector2 xPos = screenPos + new Vector2(screenSize.X - 48, 16);
                Vector2 backPos = screenPos + new Vector2(screenSize.X / 2 - 96, screenSize.Y - 48);
                Vector2 stopPos = backPos + new Vector2(80,0);
                Vector2 fwdPos = stopPos + new Vector2(80, 0);
                if (Config.PhoneApp && new Rectangle((int)xPos.X, (int)xPos.Y, 32, 32).Contains(mousePos))
                {
                    api.SetAppRunning(false);
                }
                else if (new Rectangle((int)backPos.X, (int)backPos.Y, 32, 32).Contains(mousePos))
                {
                    SetTrack(false);
                }
                else if (new Rectangle((int)stopPos.X, (int)stopPos.Y, 32, 32).Contains(mousePos))
                {
                    StopTrack(true);
                }
                else if (new Rectangle((int)fwdPos.X, (int)fwdPos.Y, 32, 32).Contains(mousePos))
                {
                    SetTrack(true);
                }
                else if (new Rectangle((int)screenPos.X,(int)screenPos.Y,(int)screenSize.X,(int)screenSize.Y).Contains(mousePos))
                {
                    if (videoPlayer.State != MediaState.Playing)
                        PlayTrack();
                    else
                        StopTrack();
                }
                else
                {
                    return;
                }
                Helper.Input.Suppress(SButton.MouseLeft);
            }

            if (e.Button == SButton.NumPad4)
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

        private void SetScreenPosAndSize()
        {
            if (Config.PhoneApp)
            {
                screenPos = api.GetScreenPosition();
                screenSize = api.GetScreenSize();
            }
            else
            {
                int x = Config.RightSide ? Game1.viewport.Width - Config.Width - Config.XOffset : Config.XOffset;
                int y = Config.Bottom ? Game1.viewport.Height - Config.Height - Config.YOffset : Config.YOffset;
                screenPos = new Vector2(x, y);
                screenSize = new Vector2(Config.Width, Config.Height);
            }
        }

        private void StopTrack(bool forceStop = false)
        {
            if (videoPlayer.State == MediaState.Playing && !forceStop)
                videoPlayer.Pause();
            else
                videoPlayer.Stop();
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

            SetScreenPosAndSize();
            if (Config.PhoneApp)
            {
                if (!api.GetPhoneOpened() || api.GetRunningApp() != Helper.ModRegistry.ModID)
                {
                    StopTrack(true);
                    Helper.Events.Display.Rendered -= Display_Rendered;
                    return;
                }
                e.SpriteBatch.Draw(backgroundTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, (int)screenSize.Y), Color.White);
            }
            else if (videoPlayer.State == MediaState.Stopped)
                return;

            if (videoPlayer.State == MediaState.Playing)
            {
                lastTexture = videoPlayer.GetTexture();
            }
            if (lastTexture != null && videoPlayer.State != MediaState.Stopped)
            {
                Vector2 size;
                Vector2 pos = screenPos;
                float rs = screenSize.X / screenSize.Y;
                float rv = lastTexture.Width / (float)lastTexture.Height;
                if (rv > rs)
                {
                    size = new Vector2(screenSize.X, screenSize.X * lastTexture.Height / lastTexture.Width);
                    pos += new Vector2(0, (screenSize.Y - size.Y) / 2);
                }
                else if (rv > rs)
                {
                    size = new Vector2(screenSize.Y * lastTexture.Width / lastTexture.Height, screenSize.Y);
                    pos += new Vector2((screenSize.X - size.X) / 2, 0);
                }
                else
                {
                    size = screenSize;
                }
                Rectangle videoRect = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);

                e.SpriteBatch.Draw(lastTexture, videoRect, Color.White);

                if (new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, (int)screenSize.Y).Contains(Game1.getMousePosition()))
                {
                    int delay = 30;
                    if (uiTicks < 255 + delay)
                        uiTicks++;
                    if (uiTicks < 255 + delay)
                        uiTicks++;

                    int c = Math.Max(uiTicks - delay, 0);

                    Color color = new Color(c, c, c, c);
                    e.SpriteBatch.Draw(buttonsTexture, screenPos + new Vector2(screenSize.X / 2 - 96, screenSize.Y - 48), color);

                    if(Config.PhoneApp)
                        e.SpriteBatch.Draw(xTexture, screenPos + new Vector2(screenSize.X - 48, 16), color);
                }
                else
                    uiTicks = 0;

            }
            if (videoPlayer.State != MediaState.Playing)
            {
                e.SpriteBatch.Draw(playTexture, screenPos + new Vector2(screenSize.X / 2 - 32, screenSize.Y / 2 - 32), Color.White);
            }
        }
    }
}
