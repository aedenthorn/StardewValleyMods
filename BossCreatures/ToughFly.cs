using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;

namespace BossCreatures
{
    internal class ToughFly : Fly
    {

        public ToughFly(Vector2 position, float difficulty) : base(position, true)
        {
            Health = (int)Math.Round(Health * difficulty);
        }
        public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            base.MovePosition(time, viewport, currentLocation);
            base.MovePosition(time, viewport, currentLocation);

        }

    }
}