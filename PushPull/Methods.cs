using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PushPull
{
    public partial class ModEntry
    {
        public static Vector2 GetNextTile(int dir)
        {
            return dir switch
            {
                0 => new(0, -1),
                1 => new(1, 0),
                2 => new(0, 1),
                3 => new(-1, 0),
                _ => new(0) //This will never be hit, but I don't like my IDE complaining with colorfull squiqly lines
            };
        }



        internal static bool CheckPull()
        {

            if (Config.Key.GetState() != SButtonState.Held)
                return false;

            var f = Game1.player;
            var tilePos = f.TilePoint.ToVector2();
            var startTile = Game1.currentCursorTile;
            var xDiff = startTile.X - tilePos.X;
            var yDiff = startTile.Y - tilePos.Y;
            if (xDiff == 0)
            {
                switch (yDiff)
                {
                    case 1:
                        f.FacingDirection = 2;
                        break;
                    case -1:
                        f.FacingDirection = 0;
                        break;
                    default:
                        return false;
                }
            }
            else if (yDiff == 0)
            {
                switch (xDiff)
                {
                    case 1:
                        f.FacingDirection = 1;
                        break;
                    case -1:
                        f.FacingDirection = 3;
                        break;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }

            var destination = f.Tile;
            
            if (!f.currentLocation.objects.TryGetValue(startTile, out var obj) || movingObjects.ContainsKey(obj) || !IsAllowed(f.currentLocation, obj, destination, false) || !IsAllowed(f.currentLocation, obj, destination - new Vector2(xDiff, yDiff), false))
                return false;

            if (PullingTile.Value != startTile)
            {
                PullingTile.Value = startTile;
                PullingTicks.Value = 0;
            }
            if (f.movementDirections.Any())
            {
                if (f.FacingDirection == 1 && f.movementDirections.Contains(3))
                {
                    f.Position = new((PullingTile.Value.X - 0.9f) * 64, f.Position.Y);
                }
                else if (f.FacingDirection == 3 && f.movementDirections.Contains(1))
                {
                    f.Position = new((PullingTile.Value.X + 0.9f) * 64, f.Position.Y);
                }
                else if (f.FacingDirection == 2 && f.movementDirections.Contains(0))
                {
                    f.Position = new(f.Position.X, (PullingTile.Value.Y - 0.5f) * 64);
                }
                else if (f.FacingDirection == 0 && f.movementDirections.Contains(2))
                {
                    f.Position = new(f.Position.X, (PullingTile.Value.Y + 0.9f) * 64);
                }
                else
                {
                    return false;
                }
                f.movementDirections.Clear();
                PullingFace.Value = f.FacingDirection;
                if (PullingTicks.Value++ >= Config.Delay)
                {
                    PullingTicks.Value = 0;
                    var dir = f.Tile - PullingTile.Value;
                    MoveObject(obj, new MovementData()
                    {
                        position = 0,
                        destination = f.Tile,
                        location = f.currentLocation,
                        direction = dir
                    });
                }
            }
            return true;
        }

        internal static void MoveObject(Object obj, MovementData movementData)
        {
            Game1.playSound(Config.Sound);
            movingObjects[obj] = movementData;
        }
    }
}