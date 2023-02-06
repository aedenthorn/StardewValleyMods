using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SprinklerMod
{
    public partial class ModEntry
    {

        private async void ActivateSprinkler(Object obj, GameLocation currentLocation)
        {
            DeactiveateSprinkler(obj.TileLocation, currentLocation);
            obj.ApplySprinklerAnimation(Game1.currentLocation);
            await Task.Delay(1000);
            foreach (Vector2 v2 in obj.GetSprinklerTiles())
            {
                obj.ApplySprinkler(Game1.currentLocation, v2);
            }
        }

        private static void DeactiveateSprinkler(Vector2 tileLocation, GameLocation currentLocation)
        {
            currentLocation.TemporarySprites.RemoveAll(s => s.id == tileLocation.X * 4000f + tileLocation.Y);
        }
    }
}