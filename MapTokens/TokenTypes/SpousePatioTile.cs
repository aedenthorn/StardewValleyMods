using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace MapTokens
{

    internal class SpousePatioTile : PropertyTile
    {
        private Point position = new Point(69, 6);

        public SpousePatioTile(int _which = 0, string _s = " ") : base(_which, _s)
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
            position = (Game1.getLocationFromName("Farm") as Farm)?.GetSpouseOutdoorAreaCorner().ToPoint() ?? new Point(69, 6); // tokens may update while the save is still being loaded
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