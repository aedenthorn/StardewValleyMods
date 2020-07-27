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
        public string name;

        public CallableNPC(string name, NPC npc, Texture2D portrait, Rectangle sourceRect)
        {
            this.name = name;
            this.npc = npc;
            this.portrait = portrait;
            this.sourceRect = sourceRect;
            this.nameSize = Game1.dialogueFont.MeasureString(name);
        }
    }
}