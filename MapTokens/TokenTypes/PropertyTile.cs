using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MapTokens
{
    public abstract class PropertyTile
    {
        private int which;
        private string separator;

        public PropertyTile(int _which = 0, string _s = " ")
        {
            which = _which;
            separator = _s;
        }

        internal string GetString(Point point)
        {
            string output = "";
            if (which != 2)
            {
                output += point.X + "";
            }
            if (which == 0)
            {
                output += separator;
            }
            if (which != 1)
            {
                output += point.Y + "";
            }
            return output;
        }

        /*********
        ** Public methods
        *********/
        /****
        ** Metadata
        ****/
        /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
        public virtual bool AllowsInput()
        {
            return false;
        }

        /// <summary>Whether the token may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public virtual bool CanHaveMultipleValues(string input = null)
        {
            return false;
        }

        /****
        ** State
        ****/
        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public virtual bool UpdateContext()
        {
            return false;
        }


        /// <summary>Get whether the token is available for use.</summary>
        public virtual bool IsReady()
        {
            return true;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public abstract IEnumerable<string> GetValues(string input);
    }
}