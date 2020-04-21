using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace BossCreatures
{
    internal class ToughFly : Fly
    {

        public ToughFly(Vector2 position) : base(position, true)
        {
            Health *= 2;
        }
        public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            base.MovePosition(time, viewport, currentLocation);
            base.MovePosition(time, viewport, currentLocation);

        }

    }
}