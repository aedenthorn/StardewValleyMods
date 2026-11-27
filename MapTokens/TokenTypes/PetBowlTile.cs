using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace MapTokens
{

    internal class PetBowlTile : PropertyTile
    {
        private Point position = new Point(53, 7);

        public PetBowlTile(int _which = 0, string _s = " ") : base(_which, _s)
        {
        }


        /****
        ** State
        ****/
        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public override bool UpdateContext()
        {
            Point oldPos = position;
            if ((Game1.getLocationFromName("Farm") as Farm)?.TryGetMapPropertyAs("PetBowlLocation", out position) != true)
            {
                position = new Point(53, 7);
            }
            return oldPos != position;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public override bool IsReady()
        {
            return true;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public override IEnumerable<string> GetValues(string input)
        {
            yield return GetString(position);
        }
    }
}