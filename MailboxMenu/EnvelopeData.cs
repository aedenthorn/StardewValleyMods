using Microsoft.Xna.Framework.Graphics;

namespace MailboxMenu
{
    public class EnvelopeData
    {
        public string texturePath;
        public string sender;
        public string title;
        public float scale = 1;
        public int frames = 1;
        public int frameWidth;
        public float frameSeconds;
        [JsonIgnore]
        public Texture2D texture;
    }
}