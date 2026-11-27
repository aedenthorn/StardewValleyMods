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

        private void ActivateSprinkler(Object obj, GameLocation currentLocation)
        {
            DeactiveateSprinkler(obj.TileLocation, currentLocation);
            obj.ApplySprinklerAnimation(Game1.currentLocation);
            activeSprinklers.Value.Add(obj, new ActiveSprinklerData()
            {
                location = Game1.currentLocation
            });
        }

        private static void DeactiveateSprinkler(Vector2 tileLocation, GameLocation currentLocation)
        {
            currentLocation.TemporarySprites.RemoveAll(s => s.id == tileLocation.X * 4000f + tileLocation.Y);
        }
    }
}