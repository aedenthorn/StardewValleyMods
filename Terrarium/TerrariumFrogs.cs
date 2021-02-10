using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Terrarium
{
    public class TerrariumFrogs : SebsFrogs
    {
        public Vector2 tile;

        public TerrariumFrogs(Vector2 tile) : base()
        {
            this.tile = tile;
            layerDepth = 1;
        }

        public void doAction()
        {
            DelayedAction.playSoundAfterDelay("croak", Game1.random.Next(1000, 3000), null, -1);
        }
    }
}