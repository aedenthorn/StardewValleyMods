using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PlaygroundFurniture
{
    public partial class ModEntry
    {
        public static bool CanBePlaygrounding(Farmer who)
        {
            return (who.IsSitting() && who.sittingFurniture is Furniture f && f.ItemId.StartsWith(furniturePrefix));
        }

    }
}