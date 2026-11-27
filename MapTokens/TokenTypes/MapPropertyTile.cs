using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Xml;

namespace MapTokens
{
    internal class MapPropertyTile : PropertyTile
    {
        /*********
        ** Fields
        *********/

        public MapPropertyTile(int _which = 0, string _s = " ") : base(_which, _s)
        {
        }

        public override bool AllowsInput()
        {
            return true;
        }

        public override bool UpdateContext()
        {
            return true;
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
            string output = "null";
            if (input != null)
            {
                string[] args = input.Split(':');
                if(args.Length == 2)
                {
                    var loc = Game1.getLocationFromName(args[0]);
                    if (loc != null )
                    {
                        var val = loc.GetMapPropertySplitBySpaces(args[1]);
                        if (ArgUtility.TryGetPoint(val, 0, out Point parsed, out _, "parsed"))
                        {
                            output = GetString(parsed);
                        }
                    }
                }
            }
            yield return output;
        }
    }
}