using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace MailboxMenu
{
    public class MailData
    {
        public string texturePath;
        public string sender;
        public string title;
        public float scale = 4;
        public int frames = 1;
        public int frameWidth;
        public float frameSeconds;
        [JsonIgnore]
        public Texture2D texture;
    }
}