using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Quests;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using static StardewValley.LocationRequest;
using Object = StardewValley.Object;

namespace CropWateringBubbles
{
    public partial class ModEntry
    {
        public void updateEmote()
        {
            GameTime time = Game1.currentGameTime;
            if (isEmoting)
            {
                emoteInterval += (float)time.ElapsedGameTime.Milliseconds;
                if (emoteFading && emoteInterval > 20f)
                {
                    emoteInterval = 0f;
                    currentEmoteFrame--;
                    if (currentEmoteFrame < 0)
                    {
                        emoteFading = false;
                        isEmoting = false;
                    }
                }
                else if (!emoteFading && emoteInterval > 20f && currentEmoteFrame <= 3)
                {
                    emoteInterval = 0f;
                    currentEmoteFrame++;
                    if (currentEmoteFrame == 4)
                    {
                        currentEmoteFrame = 28;
                        return;
                    }
                }
                else if (!emoteFading && emoteInterval > 250f)
                {
                    emoteInterval = 0f;
                    currentEmoteFrame++;
                    if (currentEmoteFrame >= 28 + 4)
                    {
                        emoteFading = true;
                        currentEmoteFrame = 3;
                    }
                }
            }
        }
    }
}