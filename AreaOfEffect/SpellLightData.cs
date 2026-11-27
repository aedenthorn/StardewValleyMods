using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace AreaOfEffect
{
    public class SpellLightData
    {
        public double timeLeft;
        public double totalTime;
        public float radius;
        public string id;
        public GameLocation location;
        public Character target;

        public bool Update(GameTime currentGameTime)
        {
            if(target is not null)
            {
                Vector2 offset = Vector2.Zero;
                if (target.shouldShadowBeOffset)
                {
                    offset += target.drawOffset;
                }
                location.repositionLightSource(id, new Vector2(target.Position.X + 21f, target.Position.Y) + offset);
            }
            timeLeft -= currentGameTime.ElapsedGameTime.Milliseconds;
            location.sharedLights[id].radius.Value = (float)(radius * timeLeft / totalTime);
            if(timeLeft <= 0)
            {
                location.removeLightSource(id);
                return true;
            }
            return false;
        }
    }
}