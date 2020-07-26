using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace SocialNetwork
{
    internal class SocialPost
    {
        public NPC npc;
        public Texture2D portrait;
        public string text;
        public Rectangle sourceRect;

        public SocialPost(NPC npc, Texture2D portrait, Rectangle sourceRect, string text)
        {
            this.npc = npc;
            this.portrait = portrait;
            this.sourceRect = sourceRect;
            this.text = text;
        }
    }
}