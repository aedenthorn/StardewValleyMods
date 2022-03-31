using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace PlantAll
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static void Utility_tryToPlaceItem_Postfix(GameLocation location, Item item, int x, int y, bool __result)
        {
            if (!Config.EnableMod || !__result || (!SHelper.Input.IsDown(Config.ModButton) && !SHelper.Input.IsDown(Config.StraightModButton)) || !IsValidItem(item))
                return;
            SMonitor.Log($"Planting all; straight {SHelper.Input.IsDown(Config.StraightModButton)}, full {SHelper.Input.IsDown(Config.ModButton)}");

            List<Point> placeables = new List<Point>();
            GetPlaceable(x / 64, y / 64, x / 64, y / 64, placeables);
            SMonitor.Log($"Got {placeables.Count} placeable tiles");
            Vector2 start = new Vector2(x / 64, y / 64);
            placeables.Sort(delegate (Point p1, Point p2) { return Vector2.Distance(start, p1.ToVector2()).CompareTo(Vector2.Distance(start, p2.ToVector2())); });

            foreach(var p in placeables)
            {
                if(((Object)Game1.player.CurrentItem).placementAction(Game1.player.currentLocation, p.X * 64, p.Y * 64, Game1.player))              
                    Game1.player.reduceActiveItemByOne();
                if (!IsValidItem(Game1.player.CurrentItem) || item.ParentSheetIndex != Game1.player.CurrentItem.ParentSheetIndex)
                    return;
            }

        }

        private static bool IsValidItem(Item item)
        {
            return item != null && item.Stack > 0 && (item.Category == -74 || item.Category == -19) && !(item as Object).isSapling() && !Object.isWildTreeSeed(item.ParentSheetIndex);
        }

        private static void GetPlaceable(int ox, int oy, int x, int y, List<Point> placeables)
        {
            List<Point> tiles = new List<Point>();
            if (SHelper.Input.IsDown(Config.StraightModButton))
            {
                bool wide = SHelper.Input.IsDown(Config.ModButton);
                switch (Game1.player.FacingDirection)
                {
                    case 0:
                        if (x != ox)
                            break;
                        tiles.Add(new Point(x, y - 1));
                        if (wide)
                        {
                            tiles.Add(new Point(x - 1, y));
                            tiles.Add(new Point(x + 1, y));
                        }
                        break;
                    case 1:
                        if (y != oy)
                            break;
                        tiles.Add(new Point(x + 1, y));
                        if (wide)
                        {
                            tiles.Add(new Point(x, y - 1));
                            tiles.Add(new Point(x, y + 1));
                        }
                        break;
                    case 2:
                        if (x != ox)
                            break;
                        tiles.Add(new Point(x, y + 1));
                        if (wide)
                        {
                            tiles.Add(new Point(x - 1, y));
                            tiles.Add(new Point(x + 1, y));
                        }
                        break;
                    case 3:
                        if (y != oy)
                            break;
                        tiles.Add(new Point(x - 1, y));
                        if (wide)
                        {
                            tiles.Add(new Point(x, y - 1));
                            tiles.Add(new Point(x, y + 1));
                        }
                        break;
                }
            }
            else if (Config.AllowDiagonal)
            {
                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (i == x && j == y)
                            continue;
                        Point p = new Point(i, j);
                        if (placeables.Contains(p))
                            continue;
                        tiles.Add(p);
                    }
                }
            }
            else
            {
                if (!placeables.Contains(new Point(x - 1, y)))
                    tiles.Add(new Point(x - 1, y));
                if (!placeables.Contains(new Point(x + 1, y)))
                    tiles.Add(new Point(x + 1, y));
                if (!placeables.Contains(new Point(x, y - 1)))
                    tiles.Add(new Point(x, y - 1));
                if (!placeables.Contains(new Point(x, y + 1)))
                    tiles.Add(new Point(x, y + 1));
            }
            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                if (!Game1.player.CurrentItem.canBePlacedHere(Game1.player.currentLocation, new Vector2(tiles[i].X, tiles[i].Y)) || (!SHelper.Reflection.GetMethod(typeof(Utility), "itemCanBePlaced").Invoke<bool>(new object[] { Game1.player.currentLocation, new Vector2(tiles[i].X, tiles[i].Y), Game1.player.CurrentItem }) && !Utility.isViableSeedSpot(Game1.player.currentLocation, new Vector2(tiles[i].X, tiles[i].Y), Game1.player.CurrentItem)))
                {
                    tiles.RemoveAt(i);
                }
                else
                {
                    placeables.Add(tiles[i]);
                }
            }
            foreach (var v in tiles)
            {
                GetPlaceable(ox, oy, v.X, v.Y, placeables);
            }
        }

    }
}