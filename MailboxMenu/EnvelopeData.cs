using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

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