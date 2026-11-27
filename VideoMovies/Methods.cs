extern alias xnavid;
extern alias xnagame;
using AsfMojo.Parsing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;
using System.Reflection;
using Video = xnavid::Microsoft.Xna.Framework.Media.Video;
using MediaState = xnagame::Microsoft.Xna.Framework.Media.MediaState;

namespace VideoMovies
{

    public partial class ModEntry : Mod
    {

        public static bool TryLoadFromWMV(string filePath, out Video video)
        {
            video = null;
            SMonitor.Log($"Loading wmv: {filePath}");
            
            using (AsfMojo.File.AsfFile asfFile = new AsfMojo.File.AsfFile(filePath))
            {
                int duration = (int)asfFile.PacketConfiguration.Duration * 1000, width = asfFile.PacketConfiguration.ImageWidth, height = asfFile.PacketConfiguration.ImageHeight;
                SMonitor.Log($"Duration: {duration}");
                if (asfFile.GetAsfObjectByType(AsfGuid.ASF_Metadata_Object).FirstOrDefault() is AsfMetadataObject metadataObject)
                {
                    foreach (AsfMetadataObject o in asfFile.GetAsfObjectByType(AsfGuid.ASF_Metadata_Object))
                    {
                        SMonitor.Log($"object: {o.Name}");
                        foreach (AsfProperty p in o.DescriptionRecords)
                        {
                            SMonitor.Log($"property: {p.Name}");
                        }
                    }
                    ConstructorInfo videoConstructor = typeof(Video).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(GraphicsDevice), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(VideoSoundtrackType) }, null);
                    SMonitor.Log($"Constructor: {videoConstructor != null}");
                    if (videoConstructor?.Invoke(new object[] { Game1.graphics.GraphicsDevice, filePath, duration, width, height, -1, VideoSoundtrackType.MusicAndDialog }) is Video v)
                    {
                        video = v;
                        SMonitor.Log($"loaded video: {video != null}");
                    }
                }
            }

            return video is Video;
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (videoPlayer.State == MediaState.Stopped)
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

                    if (Config.PhoneApp)
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
