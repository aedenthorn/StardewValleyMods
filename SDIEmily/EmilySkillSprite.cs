using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace SDIEmily
{
    public class EmilySkillSprite : TemporaryAnimatedSprite
    {
        private Vector2 center;

        public EmilySkillSprite(Vector2 center) : base("LooseSprites\\parrots", new Rectangle(48, 0, 24, 24), center, false, 1f, Color.White)
        {
            this.center = center;
            endBehavior endBehavior = new endBehavior(NewSprite);
        }

        private void NewSprite(int extraInfo)
        {
        }
    }
}