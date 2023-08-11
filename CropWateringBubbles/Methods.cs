using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Quests;
using StardewValley.SDKs;
using StardewValley.TerrainFeatures;
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
        
        public static bool CanBecomeGiant(HoeDirt instance)
        {
            if (!Config.IncludeGiantable)
                return false;
            int indexOfHarvest = instance.crop.indexOfHarvest.Value;

            if (indexOfHarvest != 276 && indexOfHarvest != 190 && indexOfHarvest != 254)
                return false;

            for(int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    if (!IsAdjacentToSame(instance, x, y))
                        return false;
                }
            }
            return true;

            bool l = IsAdjacentToSame(instance, -1, 0);
            bool u = IsAdjacentToSame(instance, 0, -1);
            bool r = IsAdjacentToSame(instance, 1, 0);
            bool d = IsAdjacentToSame(instance, 0, 1);
            if (!(l && u) && !(l && d) && !(r && u) && !(r && d))
                return false;
            bool ul = IsAdjacentToSame(instance, -1, -1);
            bool ur = IsAdjacentToSame(instance, 1, -1);
            bool dr = IsAdjacentToSame(instance, 1, 1);
            bool dl = IsAdjacentToSame(instance, -1, 1);

            if(d && l && u && r && dr && dl && ur && ul)
            {
                return true;
            }

            if (u && ul && l)
            {
                if (
                    IsAdjacentToSame(instance, -2, 0) &&
                    IsAdjacentToSame(instance, -2, -1) &&
                    IsAdjacentToSame(instance, -2, -2) &&
                    IsAdjacentToSame(instance, -1, -2) &&
                    IsAdjacentToSame(instance, 0, -2)
                )
                {
                    return true;
                }
            }
            if (u && ur && r)
            {
                if (
                    IsAdjacentToSame(instance, 2, 0) &&
                    IsAdjacentToSame(instance, 2, -1) &&
                    IsAdjacentToSame(instance, 2, -2) &&
                    IsAdjacentToSame(instance, 1, -2) &&
                    IsAdjacentToSame(instance, 0, -2)
                )
                {
                    return true;
                }
            }
            if (d && dl && l)
            {
                if (
                    IsAdjacentToSame(instance, -2, 0) &&
                    IsAdjacentToSame(instance, -2, 1) &&
                    IsAdjacentToSame(instance, -2, 2) &&
                    IsAdjacentToSame(instance, -1, 2) &&
                    IsAdjacentToSame(instance, 0, 2)
                )
                {
                    return true;
                }
            }
            if (d && dr && r)
            {
                if (
                    IsAdjacentToSame(instance, 2, 0) &&
                    IsAdjacentToSame(instance, 2, 1) &&
                    IsAdjacentToSame(instance, 2, 2) &&
                    IsAdjacentToSame(instance, 1, 2) &&
                    IsAdjacentToSame(instance, 0, 2)
                )
                {
                    return true;
                }
            }
            if(u && l && r && ul && ur)
            {
                if (
                    IsAdjacentToSame(instance, -1, -2) &&
                    IsAdjacentToSame(instance, 0, -2) &&
                    IsAdjacentToSame(instance, 1, -2)
                )
                {
                    return true;
                }

            }
            if(d && l && r && dl && dr)
            {
                if (
                    IsAdjacentToSame(instance, -1, 2) &&
                    IsAdjacentToSame(instance, 0, 2) &&
                    IsAdjacentToSame(instance, 1, 2)
                )
                {
                    return true;
                }

            }
            if(u && d & l && dl && ul)
            {
                if (
                    IsAdjacentToSame(instance, -2, -1) &&
                    IsAdjacentToSame(instance, -2, 0) &&
                    IsAdjacentToSame(instance, -2, 1)
                )
                {
                    return true;
                }

            }
            if(u && d & r && dr && ur)
            {
                if (
                    IsAdjacentToSame(instance, 2, -1) &&
                    IsAdjacentToSame(instance, 2, 0) &&
                    IsAdjacentToSame(instance, 2, 1)
                )
                {
                    return true;
                }

            }
            return false;
        }

        public static bool IsAdjacentToSame(HoeDirt instance, int v1, int v2)
        {
            return instance.currentLocation.terrainFeatures.TryGetValue(instance.currentTileLocation + new Vector2(v1, v2), out var tf) && tf is HoeDirt && (tf as HoeDirt).crop?.indexOfHarvest?.Value == instance.crop.indexOfHarvest.Value && (tf as HoeDirt).crop.currentPhase.Value >= (tf as HoeDirt).crop.phaseDays.Count - 1 && (!instance.crop.fullyGrown.Value || (tf as HoeDirt).crop.dayOfCurrentPhase.Value <= 0);
        }
    }
}