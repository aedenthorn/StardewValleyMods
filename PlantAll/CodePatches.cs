using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace PlantAll
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Utility), nameof(Utility.tryToPlaceItem))]
        private static class UtilitytryToPlaceItem_Patch
        {
            private static void Postfix(GameLocation location, Item item, int x, int y, bool __result)
            {
                if (!Config.EnableMod || !__result || (!SHelper.Input.IsDown(Config.ModButton) && !SHelper.Input.IsDown(Config.StraightModButton) && !SHelper.Input.IsDown(Config.SprinklerModButton)) || !IsValidItem(item))
                    return;
                PlantAll(item, location, x / 64, y / 64);
            }
        }

        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.performObjectDropInAction))]
        private static class IndoorPot_performObjectDropInAction_Patch
        {
            private static void Postfix(Object __instance, Item dropInItem, bool probe, Farmer who, bool __result)
            {
                if (!Config.EnableMod || probe || !__result || (!SHelper.Input.IsDown(Config.ModButton) && !SHelper.Input.IsDown(Config.StraightModButton) && !SHelper.Input.IsDown(Config.SprinklerModButton)) || !IsValidItem(dropInItem))
                    return;
                PlantAll(dropInItem, who.currentLocation, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y);
            }
        }

        private static void PlantAll(Item item, GameLocation location, int x, int y)
        {
            SMonitor.Log($"Planting all; straight {SHelper.Input.IsDown(Config.StraightModButton)}, sprinkler {SHelper.Input.IsDown(Config.SprinklerModButton)}, full {SHelper.Input.IsDown(Config.ModButton)}");

            List<Point> placeables = new List<Point>();
            GetPlaceable(item, location, x, y, x, y, placeables);
            SMonitor.Log($"Got {placeables.Count} placeable tiles");
            Vector2 start = new Vector2(x, y);
            placeables.Sort(delegate (Point p1, Point p2) { return Vector2.Distance(start, p1.ToVector2()).CompareTo(Vector2.Distance(start, p2.ToVector2())); });

            foreach (var p in placeables)
            {
                if(location.objects.TryGetValue(p.ToVector2(), out Object o) && o is IndoorPot)
                {
                    if (o.performObjectDropInAction(item, false, Game1.player))
                        Game1.player.reduceActiveItemByOne();
                }
                else if (location.terrainFeatures.TryGetValue(p.ToVector2(), out TerrainFeature f) && f is HoeDirt)
                {
                    if (Game1.player.ActiveObject.placementAction(Game1.player.currentLocation, p.X * 64, p.Y * 64, Game1.player))
                        Game1.player.reduceActiveItemByOne();
                }
                if (!IsValidItem(Game1.player.ActiveObject) || item.ParentSheetIndex != Game1.player.ActiveObject.ParentSheetIndex)
                    return;
            }
        }

        private static bool IsValidItem(Item item)
        {
            return item != null && item.Stack > 0 && (item.Category == -74 || item.Category == -19) && !(item as Object).isSapling() && !Object.isWildTreeSeed(item.ParentSheetIndex);
        }

        private static void GetPlaceable(Item item, GameLocation location, int ox, int oy, int x, int y, List<Point> placeables)
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
            else if (SHelper.Input.IsDown(Config.SprinklerModButton))
            {
                var playerTile = Game1.player.getTileLocationPoint();
                int size = SHelper.Input.IsDown(Config.ModButton) ? 2 : 1;
                for (int i = playerTile.X - size; i <= playerTile.X + size; i++)
                {
                    for (int j = playerTile.Y - size; j <= playerTile.Y + size; j++)
                    {
                        if (i == playerTile.X && j == playerTile.Y)
                            continue;
                        tiles.Add(new Point(i, j));
                    }
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

                if (
                    (
                        item.canBePlacedHere(location, new Vector2(tiles[i].X, tiles[i].Y)) &&
                        (SHelper.Reflection.GetMethod(typeof(Utility), "itemCanBePlaced").Invoke<bool>(new object[] { location, new Vector2(tiles[i].X, tiles[i].Y), item })
                            || Utility.isViableSeedSpot(location, new Vector2(tiles[i].X, tiles[i].Y), item))
                    )
                    || (location.objects.TryGetValue(tiles[i].ToVector2(), out Object o) && o is IndoorPot && (o as IndoorPot).hoeDirt.Value.crop is null)
                )
                {
                    placeables.Add(tiles[i]);
                }
                else
                {
                    tiles.RemoveAt(i);
                }
            }
            if (SHelper.Input.IsDown(Config.SprinklerModButton))
                return;
            foreach (var v in tiles)
            {
                GetPlaceable(item, location, ox, oy, v.X, v.Y, placeables);
            }
        }

    }
}