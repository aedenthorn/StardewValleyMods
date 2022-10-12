using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using System;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PersonalTravellingCart
{
    public partial class ModEntry
    {
        private static bool IsMouseInBoundingBox(Character c, PersonalCartData data)
        {
            if (!Config.ModEnabled)
                return false;
            DirectionData ddata = GetDirectionData(data, c.FacingDirection);
            Rectangle box = new Rectangle(Utility.Vector2ToPoint(c.Position + ddata.cartOffset) + new Point(ddata.clickRect.Location.X * 4, ddata.clickRect.Location.Y * 4), new Point(ddata.clickRect.Width * 4, ddata.clickRect.Height * 4));
            return box.Contains(Game1.viewport.X + Game1.getMouseX(), Game1.viewport.Y + Game1.getMouseY());
        }
        private static PersonalCartData GetCartData(string whichCart)
        {
            PersonalCartData data;
            if (!cartDict.TryGetValue(whichCart, out data))
                data = cartDict[defaultKey];
            return data;
        }
        private static DirectionData GetDirectionData(PersonalCartData data, int facingDirection)
        {

            switch (facingDirection)
            {
                case 0:
                    return data.up;
                case 1:
                    return data.right;
                case 2:
                    return data.down;
                default:
                    return data.left;
            }
        }

        private static void DrawLayer(Layer layer, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool v1, int pixelZoom)
        {
            int tileWidth = pixelZoom * 16;
            int tileHeight = pixelZoom * 16;
            Location tileInternalOffset = new Location(Wrap(mapViewport.X, tileWidth), Wrap(mapViewport.Y, tileHeight));
            int tileXMin = (mapViewport.X >= 0) ? (mapViewport.X / tileWidth) : ((mapViewport.X - tileWidth + 1) / tileWidth);
            int tileYMin = (mapViewport.Y >= 0) ? (mapViewport.Y / tileHeight) : ((mapViewport.Y - tileHeight + 1) / tileHeight);
            if (tileXMin < 0)
            {
                displayOffset.X -= tileXMin * tileWidth;
                tileXMin = 0;
            }
            if (tileYMin < 0)
            {
                displayOffset.Y -= tileYMin * tileHeight;
                tileYMin = 0;
            }
            int tileColumns = 1 + (mapViewport.Size.Width - 1) / tileWidth;
            int tileRows = 1 + (mapViewport.Size.Height - 1) / tileHeight;
            if (tileInternalOffset.X != 0)
            {
                tileColumns++;
            }
            if (tileInternalOffset.Y != 0)
            {
                tileRows++;
            }
            Location tileLocation = displayOffset - tileInternalOffset;
            int offset = 0;
            tileLocation.Y = displayOffset.Y - tileInternalOffset.Y - tileYMin * 64;
            for (int tileY = 0; tileY < layer.LayerSize.Height; tileY++)
            {
                tileLocation.X = displayOffset.X - tileInternalOffset.X - tileXMin * 64;
                for (int tileX = 0; tileX < layer.LayerSize.Width; tileX++)
                {
                    Tile tile = layer.Tiles[tileX, tileY];
                    if (tile != null)
                    {
                        displayDevice.DrawTile(tile, tileLocation, (tileY * (16 * pixelZoom) + 16 * pixelZoom + offset) / 10000f);
                    }
                    tileLocation.X += tileWidth;
                }
                tileLocation.Y += tileHeight;
            }
        }

        private static int Wrap(int value, int span)
        {
            value %= span;
            if (value < 0)
            {
                value += span;
            }
            return value;
        }
    }
}