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
            if (!Config.EnableMod || !__result || !SHelper.Input.IsDown(Config.ModButton) || !(item is Object) || (((Object)item).Category != -74 && ((Object)item).Category != -19) || item.Stack <= 0)
                return;
            SMonitor.Log($"Planting all");

            List<Point> placeables = new List<Point>();
            GetPlaceable(x / 64, y / 64, placeables);
            SMonitor.Log($"Got {placeables.Count} placeable tiles");
            Vector2 start = new Vector2(x / 64, y / 64);
            placeables.Sort(delegate (Point p1, Point p2) { return Vector2.Distance(start, p1.ToVector2()).CompareTo(Vector2.Distance(start, p2.ToVector2())); });

            foreach(var p in placeables)
            {
                if(((Object)Game1.player.CurrentItem).placementAction(Game1.player.currentLocation, p.X * 64, p.Y * 64, Game1.player))              
                    Game1.player.reduceActiveItemByOne();
                if (Game1.player.CurrentItem == null || (Game1.player.CurrentItem.Category != -74 && Game1.player.CurrentItem.Category != -19) || Game1.player.CurrentItem.Stack <= 0)
                    return;
            }

        }

        private static void GetPlaceable(int x, int y, List<Point> placeables)
        {
            List<Point> tiles = new List<Point>();

            if (Config.AllowDiagonal)
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
                GetPlaceable(v.X, v.Y, placeables);
            }
        }

    }
}