using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Audio;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CloseDoors
{
    public partial class ModEntry
    {
        public static bool TryCloseDoor(GameLocation location, Point tilePoint)
        {
            foreach (var d in location.interiorDoors.Doors)
            {
                if (d.Position == tilePoint)
                {
                    if (!d.Value)
                        return false;
                    location.playSound("doorClose", Utility.PointToVector2(tilePoint), null, SoundContext.Default);
                    d.Sprite.paused = true;
                    location.interiorDoors[tilePoint] = false;

                    Location doorLocation = new(d.Position.X, d.Position.Y);
                    Map map = d.Location.Map;
                    if (map == null)
                    {
                        return false;
                    }
                    if (d.Tile == null)
                    {
                        return false;
                    }
                    map.GetLayer("Buildings").Tiles[doorLocation] = d.Tile;
                    d.Location.removeTileProperty(d.Position.X, d.Position.Y, "Back", "TemporaryBarrier");
                    doorLocation.Y--;
                    map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), d.Tile.TileSheet, BlendMode.Alpha, d.Tile.TileIndex - d.Tile.TileSheet.SheetWidth);
                    doorLocation.Y--;
                    map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), d.Tile.TileSheet, BlendMode.Alpha, d.Tile.TileIndex - d.Tile.TileSheet.SheetWidth * 2);

                    d.ResetLocalState();
                    return true;
                }
            }
            return false;
        }
        private static bool IsDoorOpen(GameLocation location, Point tilePoint)
        {

            foreach (var d in location.interiorDoors.Doors)
            {
                if (d.Position == tilePoint)
                {
                    return d.Value;
                }
            }
            return false;
        }

        public static bool IsRecentDoorPoint(Character character, Point tileLocation, Point point)
        {
            switch (character.FacingDirection)
            {
                case 0:
                    return point == tileLocation + new Point(0, 2);
                case 2:
                    return point == tileLocation + new Point(0, -2);
                case 1:
                    return point == tileLocation + new Point(-1, -1);
                case 3:
                    return point == tileLocation + new Point(1, -1);
            }
            return false;
        }
        public static Point GetMovingTile(int facing, Rectangle tile)
        {
            switch (facing)
            {
                case 0:
                    return new Point(tile.Center.X / 64, tile.Top / 64);
                case 2:
                    return new Point(tile.Center.X / 64, tile.Bottom / 64);
                case 1:
                    return new Point(tile.Right / 64, tile.Center.Y / 64);
                case 3:
                    return new Point(tile.Left / 64, tile.Center.Y / 64);
            }
            return Point.Zero;
        }
    }
}