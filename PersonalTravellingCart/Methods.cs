using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersonalTravellingCart
{
    public partial class ModEntry
    {
        private static bool IsMouseInBoundingBox()
        {
            Rectangle box = new Rectangle();
            if(Game1.player.FacingDirection == 3)
            {
                box = new Rectangle((int)Game1.player.position.X + 120, (int)Game1.player.position.Y - 196, 93 * 4, 71 * 4);
            }
            else if(Game1.player.FacingDirection == 1)
            {
                box = new Rectangle((int)Game1.player.position.X - 382, (int)Game1.player.position.Y - 196, 93 * 4, 71 * 4);
            }
            return box.Contains(Game1.viewport.X + Game1.getMouseX(), Game1.viewport.Y + Game1.getMouseY());
        }
    }
}