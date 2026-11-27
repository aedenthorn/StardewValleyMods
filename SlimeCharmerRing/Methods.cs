using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace SlimeCharmerRing
{
    public partial class ModEntry
    {
        public static bool Attracts(Ring ring, int which)
        {
            if (ring is null)
                return true;
            if(ring is CombinedRing cr)
            {
                foreach (var r in cr.combinedRings)
                {
                    if (!Attracts(r, which))
                        return false;
                }
                return true;
            }
            else if(ring.ItemId != "520")
            {
                return true;
            }
            int w = GetWhich(ring);
            string effect = GetEffect(ring);
            return effect switch
            {
                "Repulse" => w != which,
                "Attract" => w == which,
                _ => true
            };
        }

        public static int GetWhich(Ring ring)
        {
            if(!ring.modData.TryGetValue(whichKey, out var str) || !int.TryParse(str, out var which))
            {
                which = 0;
            }
            return which;
        }
        public static string GetEffect(Ring ring)
        {
            if(!ring.modData.TryGetValue(effectKey, out var str))
            {
                str = "";
            }
            return str;
        }

        public static bool InColorRange(Color slime, Color color)
        {
            return IsInRange(slime.R, color.R) && IsInRange(slime.G, color.G) && IsInRange(slime.B, color.B);
        }

        public static bool IsInRange(byte s, byte c)
        {
            return s >= (c - 20) * (7 / 10f) && s <= (c + 20) * (11 / 10f);
        }
    }
}