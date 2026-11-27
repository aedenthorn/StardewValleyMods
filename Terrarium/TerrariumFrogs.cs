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
        }

        public void doAction()
        {
            if(ModEntry.Config.PlaySound != null && ModEntry.Config.PlaySound.Length > 0)
               DelayedAction.playSoundAfterDelay(ModEntry.Config.PlaySound, Game1.random.Next(1000, 3000), null, -1);
        }
    }
}