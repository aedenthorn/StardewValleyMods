using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MobilePhone
{
    public class CallableNPC
    {
        public NPC npc;
        public Texture2D portrait;
        public Rectangle sourceRect;
        public Vector2 nameSize;

        public CallableNPC(NPC npc, Texture2D portrait, Rectangle sourceRect)
        {
            this.npc = npc;
            this.portrait = portrait;
            this.sourceRect = sourceRect;
            this.nameSize = Game1.dialogueFont.MeasureString(npc.displayName);
        }
    }
}