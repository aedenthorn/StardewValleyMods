using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapTokens
{
    internal class WarpTiles : PropertyTile
    {
        /*********
        ** Fields
        *********/
        public static Dictionary<string, List<Warp>> locationDict = new();

        public WarpTiles(int _which = 0, string _s = " ") : base(_which, _s)
        {
        }
        public override bool CanHaveMultipleValues(string input = null)
        {
            return true;
        }
        public override bool AllowsInput()
        {
            return true;
        }

        public override bool UpdateContext()
        {
            if (Game1.locations?.Count <= 0)
                return false;
            bool changed = false;
            Dictionary<string, List<Warp>> newLocDict = new();
            foreach (var l in Game1.locations)
            {
                List<Warp> dict = null;
                if (!changed && !locationDict.TryGetValue(l.NameOrUniqueName, out dict))
                {
                    changed = true;
                }
                List<Warp> newDict = new();
                foreach (var p in l.warps)
                {
                    newDict.Add(p);
                    if (!changed && dict?.Contains(p) != true)
                    {
                        changed = true;
                    }
                }
                if (!changed && dict?.Count != newDict.Count)
                    changed = true;
                if (changed)
                    newLocDict[l.NameOrUniqueName] = newDict;
            }
            if (changed)
            {
                locationDict = newLocDict;
            }
            return changed;
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
            if (input == null)
                return new List<string>();
            string[] args = input.Split(':');
            if (!locationDict.TryGetValue(args[0].Trim(), out var dict) || dict == null)
                return new List<string>();
            return dict.Where(w => args.Length < 2 || w.TargetName == args[1]).Select(w => GetString(new Point(w.X, w.Y)));
        }
    }
}