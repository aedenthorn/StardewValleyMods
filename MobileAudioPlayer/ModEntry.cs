using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WMPLib;

namespace MapTeleport
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        public static ModConfig Config;
        private IMobilePhoneApi api;
        WindowsMediaPlayer Player;

        private Texture2D backgroundTexture;
        private Texture2D hightlightTexture;
        private Texture2D buttonBarTexture;
        private Texture2D buttonsTexture;
        private Texture2D volumeBarTexture;

        private string[] audio;
        private Vector2 screenPos;
        private Vector2 screenSize;
        private Point lastMousePosition;
        private bool dragging;
        private int offsetY;
        private int trackPlaying = 0;
        private int currentState;
        private bool ended;
        private bool clicking;
        private float currentMusicVolume;
        private float currentAmbientVolume;
        private bool opening;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            try
            {
                if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "audio")))
                    audio = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "audio"));
                else
                {
                    Directory.CreateDirectory(Path.Combine(Helper.DirectoryPath, "audio"));
                    Monitor.Log($"No audio files found. Please put audio files in {Path.Combine(Helper.DirectoryPath, "audio")}.", LogLevel.Warn);
                    return;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading audio files:\r\n{ex}", LogLevel.Error);
                return;
            }

            Player = new WindowsMediaPlayer();
            Player.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
            Player.MediaError += new _WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
            Player.settings.volume = Config.VolumeLevel;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
        }
        private void OpenAudioPlayer()
        {
            api.SetAppRunning(true);
            api.SetRunningApp(Helper.ModRegistry.ModID);
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            opening = true;
        }

        private void Player_MediaError(object pMediaObject)
        {
            Monitor.Log($"track error {audio[trackPlaying]}", LogLevel.Debug);
        }

        private void Player_PlayStateChange(int NewState)
        {
            Monitor.Log($"new state {(WMPPlayState)NewState}", LogLevel.Debug);
            if ((WMPPlayState)NewState == WMPPlayState.wmppsMediaEnded)
            {
                Monitor.Log($"track ended {audio[trackPlaying]}", LogLevel.Debug);
                if (!Config.PlayAll || (trackPlaying >= audio.Length - 1 && !Config.LoopPlaylist))
                {
                    MuteVolume(false);

                    return;
                }
                trackPlaying++;
                trackPlaying %= audio.Length;
                DelayedPlay(audio[trackPlaying]);
            }
            currentState = NewState;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (api.IsCallingNPC() || api.GetRunningApp() != Helper.ModRegistry.ModID)
                return;

            if(e.Button == SButton.MouseLeft)
            {
                Point mousePos = Game1.getMousePosition();
                if (!api.GetScreenRectangle().Contains(mousePos))
                {
                    return;
                }

                Helper.Input.Suppress(SButton.MouseLeft);

                Vector2 screenPos = api.GetScreenPosition();
                Vector2 screenSize = api.GetScreenSize();
                if (new Rectangle((int)screenPos.X, (int)(screenPos.Y + screenSize.Y - 32), (int)screenSize.X - Config.VolumeBarWidth - 2, 32).Contains(mousePos))
                {
                    int space = (int)Math.Round((screenSize.X - 168) / 8f);
                    int offset = mousePos.X - (int)screenPos.X - space / 2;
                    int idx = offset / (space + 24);
                    Monitor.Log($"button index: {idx}");
                    ClickButton(idx);
                    return;
                }
                else if (new Rectangle((int)(screenPos.X + screenSize.X - Config.VolumeBarWidth - 2), (int)(screenPos.Y + screenSize.Y - 31), Config.VolumeBarWidth + 2, 30).Contains(mousePos))
                {
                    SetVolume(screenPos.Y + screenSize.Y - mousePos.Y - 1);
                }

                clicking = true;
                lastMousePosition = mousePos;
            }
        }

        private void SetVolume(float v)
        {
            Monitor.Log($"Volume set to: {(int)(v * 100 / 30)}");
            Config.VolumeLevel = (int)(v * 100 / 30);
            Helper.WriteConfig(Config);
            Player.settings.volume = Config.VolumeLevel;
        }

        private void ClickButton(int idx)
        {
            switch (idx)
            {
                case 0:
                    StopTrack(true);
                    api.SetAppRunning(false);
                    api.SetRunningApp(null);
                    break;
                case 1:
                    SwitchTrack(false);
                    break;
                case 2:
                    SeekTrack(false);
                    break;
                case 3:
                    ToggleTrack();
                    break;
                case 4:
                    SeekTrack(true);
                    break;
                case 5:
                    SwitchTrack(true);
                    break;
                case 6:
                    api.SetAppRunning(false);
                    api.SetRunningApp(null);
                    break;

            }
        }

        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            opening = false;

            if (e.Button == SButton.MouseLeft)
            {
                if (api.GetRunningApp() != Helper.ModRegistry.ModID)
                    return;


            }
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                bool success = api.AddApp(Helper.ModRegistry.ModID, Helper.Translation.Get("app-name"), OpenAudioPlayer, appIcon);
                Monitor.Log($"loaded app successfully: {success}", LogLevel.Debug);
            }
            MakeTextures();
        }

        private void MakeTextures()
        {
            screenSize = api.GetScreenSize();
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, (int)screenSize.Y);
            Color[] data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.BackgroundColor;
            }
            texture.SetData(data);
            backgroundTexture = texture;
            texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, (int)(Game1.dialogueFont.LineSpacing * (Config.LineOneScale + Config.LineTwoScale)));
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.HighlightColor;
            }
            texture.SetData(data);
            hightlightTexture = texture;

            texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, 32);
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.ButtonBarColor;
            }
            texture.SetData(data);
            buttonBarTexture = texture;

            buttonsTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "buttons.png"));

            texture = new Texture2D(Game1.graphics.GraphicsDevice, Config.VolumeBarWidth, 30);
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.VolumeBarColor;
            }
            texture.SetData(data);
            volumeBarTexture = texture;
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {

            if (api.IsCallingNPC())
                return;

            float itemHeight = Game1.dialogueFont.LineSpacing * (Config.LineOneScale + Config.LineTwoScale);

            screenPos = api.GetScreenPosition();
            screenSize = api.GetScreenSize();
            if (!api.GetPhoneOpened() || !api.GetAppRunning() || api.GetRunningApp() != Helper.ModRegistry.ModID)
            {
                Monitor.Log($"Closing app: phone opened {api.GetPhoneOpened()} app running {api.GetAppRunning()} running app {api.GetRunningApp()}");
                Helper.Events.Display.RenderedWorld -= Display_RenderedWorld;

                if (api.GetRunningApp() == Helper.ModRegistry.ModID || !api.GetAppRunning())
                {
                    api.SetAppRunning(false);
                    api.SetRunningApp(null);
                }
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
                    offsetY = Math.Min(0, Math.Max(offsetY, (int)(screenSize.Y - (32 + Config.MarginY + (Config.MarginY + itemHeight) * audio.Length))));
                    lastMousePosition = mousePos;
                }
            }

            if(clicking && !Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                Monitor.Log($"unclicking; dragging = {dragging}");
                if (dragging)
                    dragging = false;
                else if (api.GetScreenRectangle().Contains(mousePos) && !new Rectangle((int)screenPos.X, (int)(screenPos.Y + screenSize.Y - 32), (int)screenSize.X, 32).Contains(mousePos))
                {
                    ClickTrack(mousePos);
                }
                clicking = false;
            }


            e.SpriteBatch.Draw(backgroundTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, (int)screenSize.Y), Color.White);
            for(int i = 0; i < audio.Length; i++)
            {
                string a = Path.GetFileName(audio[i]);
                string lineOne = Config.ListLineOne;
                string lineTwo = Config.ListLineTwo;
                MakeListString(a, ref lineOne, Config.LineOneScale);
                MakeListString(a, ref lineTwo, Config.LineTwoScale);
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
            Vector2 barPos = screenPos + new Vector2(0, screenSize.Y - 32);
            int space = (int)Math.Round((screenSize.X - 168 - Config.VolumeBarWidth - 2) / 8f);
            e.SpriteBatch.Draw(buttonBarTexture, new Rectangle((int)barPos.X, (int)barPos.Y, (int)screenSize.X, 32), Color.White);
            for (int i = 0; i < 7; i++)
            {
                int j = i;
                bool playing = true;
                try
                {
                    playing = (Player.playState == WMPPlayState.wmppsPlaying);
                }
                catch 
                { 
                }
                if (playing && i > 2 || i > 3)
                    j++;

                e.SpriteBatch.Draw(buttonsTexture, barPos + new Vector2(space + (space + 24) * i, 4), new Rectangle(24 * j, 0, 24, 24), Color.White);
            }
            e.SpriteBatch.Draw(volumeBarTexture, new Rectangle((int)(screenPos.X + screenSize.X - Config.VolumeBarWidth - 1), (int)barPos.Y + 31 - (Config.VolumeLevel * 30 / 100), Config.VolumeBarWidth, Config.VolumeLevel * 30 / 100), Color.White);
        }

        private void MakeListString(string a, ref string line, float scale)
        {
            a = new Regex(@"\.[^.]+$").Replace(a, "");
            string[] aa = a.Split('_');
            for(int i = 0; i < aa.Length; i++)
            {
                line = line.Replace("{"+ (i + 1) + "}", aa[i]);
            }
            float width = api.GetScreenSize().X - Config.MarginX * 2;
            int j = 0;
            string ow = line;
            while (Game1.dialogueFont.MeasureString(line).X * scale > width)
            {
                line = line.Substring(0, ow.Length - 3 - j++) + "...";
            }
        }

        private void ClickTrack(Point mousePos)
        {
            int idx = (int)((mousePos.Y - api.GetScreenPosition().Y - Config.MarginY - offsetY) / (Config.MarginY + Game1.dialogueFont.LineSpacing * (Config.LineOneScale + Config.LineTwoScale)));
            Monitor.Log($"clicked index: {idx}");
            if (idx < audio.Length && idx >= 0)
            {
                if (idx == trackPlaying)
                {
                    if(Player.playState == WMPPlayState.wmppsPlaying)
                    {
                        MuteVolume(false);
                        Player.controls.pause();
                    }
                    else if (Player.playState == WMPPlayState.wmppsPaused)
                    {
                        MuteVolume(true);
                        Player.controls.play();
                    }
                    else
                    {
                        PlayFile(audio[trackPlaying]);
                    }
                }
                else
                {
                    trackPlaying = idx;
                    PlayFile(audio[trackPlaying]);
                }
            }
        }
        private async void DelayedPlay(string url)
        {
            await Task.Delay(100);
            Monitor.Log($"Delayed play {url}");
            PlayFile(url);
        }

        private void PlayFile(String url)
        {
            MuteVolume(true);
            Monitor.Log($"playing file {audio[trackPlaying]}", LogLevel.Debug);
            Player.URL = url;
            Player.controls.play();
        }

        private void MuteVolume1(bool off)
        {
            if (off)
            {
                Game1.currentSong.Stop(AudioStopOptions.Immediate);
            }
            else
            {

                Game1.currentSong.Play();
            }
        }
        
        private void MuteVolume(bool off)
        {
            if (off)
            {
                if(Game1.musicPlayerVolume != 0)
                    currentMusicVolume = Game1.musicPlayerVolume;
                if (Game1.ambientPlayerVolume != 0)
                    currentAmbientVolume = Game1.ambientPlayerVolume;
                if (Config.MuteGameMusicWhilePlaying)
                {
                    Game1.currentSong.Stop(AudioStopOptions.Immediate);
                    Game1.musicPlayerVolume = 0;
                }
                if (Config.MuteAmbientSoundWhilePlaying)
                    Game1.ambientPlayerVolume = 0;

            }
            else
            {
                if (Config.MuteGameMusicWhilePlaying)
                {
                    try
                    {
                        if(Game1.currentSong != null && Game1.currentSong.IsPlaying)
                            Game1.currentSong.Stop(AudioStopOptions.Immediate);
                        Game1.musicPlayerVolume = currentMusicVolume;
                        Game1.player.currentLocation.checkForMusic(Game1.currentGameTime);
                        Game1.currentSong = Game1.soundBank.GetCue(Game1.currentSong.Name);
                        Game1.currentSong.Play();
                    }
                    catch
                    {
                        Monitor.Log($"Couldn't restart music track {Game1.currentSong?.Name}");
                    }
                }
                if (Config.MuteAmbientSoundWhilePlaying)
                    Game1.ambientPlayerVolume = currentAmbientVolume;
            }
            Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
            Game1.ambientCategory.SetVolume(Game1.ambientPlayerVolume);
        }

        private void StopTrack(bool force = false)
        {
            MuteVolume(false);

            Monitor.Log($"status on stopping: {Player.status}");
            if (force)
            {
                Player.controls.stop();
            }
            else
            {
                if(Player.playState == WMPPlayState.wmppsPlaying)
                    Player.controls.pause();
                else
                    Player.controls.stop();
            }
        }
        private void SwitchTrack(bool next)
        {
            Player.controls.stop();
            if (next)
            {
                if (trackPlaying < audio.Length - 1 || Config.LoopPlaylist)
                {
                    trackPlaying++;
                    trackPlaying %= audio.Length;
                    PlayFile(audio[trackPlaying]);
                }
            }
            else
            {
                if (trackPlaying > 0 || Config.LoopPlaylist)
                {
                    trackPlaying--;
                    if(trackPlaying < 0)
                        trackPlaying = audio.Length - 1;
                    PlayFile(audio[trackPlaying]);
                }
            }
        }
        private void ToggleTrack()
        {
            if (Player.playState == WMPPlayState.wmppsPlaying)
            {
                MuteVolume(false);
                Player.controls.pause();
            }
            else if (Player.playState == WMPPlayState.wmppsStopped)
            {
                PlayFile(audio[trackPlaying]);
            }
            else 
            { 
                MuteVolume(true);
                Player.controls.play();
            }
        }

        private void SeekTrack(bool forward)
        {
            if (forward)
                Player.controls.fastForward();
            else
                Player.controls.fastReverse();
        }

    }
}
