using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace DynamicMapTiles
{
    public partial class ModEntry
    {
        private static void DoStepOnActions(Farmer farmer, Point tilePos)
        {
            if (!farmer.currentLocation.isTileOnMap(tilePos.ToVector2()))
                return;

            var tile = farmer.currentLocation.Map.GetLayer("Back").Tiles[tilePos.X, tilePos.Y];

            if (tile is null)
                return;
            PropertyValue value;
            int number;
            if (tile.Properties.TryGetValue(changeIndexKey, out value))
            {
                if(int.TryParse(value, out number))
                {
                    if (tile.Layer.Tiles[tilePos.X, tilePos.Y] is null)
                    {
                        tile.Layer.Tiles[tilePos.X, tilePos.Y] = new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, number);
                    }
                    else
                    {
                        tile.Layer.Tiles[tilePos.X, tilePos.Y].TileIndex = number;
                    }
                }
                else
                {
                    var split = value.ToString().Split(',');
                    var split2 = split[0].Split(' ');
                    List<StaticTile> tiles = new List<StaticTile>();
                    foreach(var s in split2)
                    {
                        tiles.Add(new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, int.Parse(s)));
                    }
                    tile.Layer.Tiles[tilePos.X, tilePos.Y] = new AnimatedTile(tile.Layer, tiles.ToArray(), int.Parse(split[1]));
                }
            }
            if (tile.Properties.TryGetValue(changeMultipleIndexKey, out value))
            {
                var split = value.ToString().Split(',');
                foreach(var str in split)
                {
                    var ss = str.Split(' ');
                    if(ss.Length == 3)
                    {
                        tile.Layer.Tiles[int.Parse(ss[0]), int.Parse(ss[1])].TileIndex = int.Parse(ss[2]);
                    }
                    else if(ss.Length == 4)
                    {
                        farmer.currentLocation.Map.GetLayer(ss[0]).Tiles[int.Parse(ss[1]), int.Parse(ss[2])].TileIndex = int.Parse(ss[3]);
                    }
                }
            }
            if (tile.Properties.TryGetValue(changePropertiesKey, out value))
            {
                var props = value.ToString().Split('`');
                foreach(var prop in props)
                {
                    var kvp = prop.Split("=");
                    if(kvp.Length == 2)
                    {
                        tile.Properties[kvp[0]] = kvp[1];
                    }
                    else
                    {
                        tile.Properties.Remove(kvp[0]);
                    }
                }
            }
            if (tile.Properties.TryGetValue(changeMultiplePropertiesKey, out value))
            {
                var tiles = value.ToString().Split('|');
                foreach (var tileProp in tiles)
                {
                    var props = tileProp.Split('`');
                    foreach (var prop in props)
                    {
                        var kvp = prop.Split("=");
                        if (kvp.Length == 2)
                        {
                            var tileInfo = kvp[0].Split(",");
                            if (tileInfo.Length == 3)
                            {
                                tile.Layer.Tiles[int.Parse(tileInfo[0]), int.Parse(tileInfo[1])].Properties[tileInfo[2]] = kvp[1];
                            }
                            else if (tileInfo.Length == 4)
                            {
                                farmer.currentLocation.Map.GetLayer(tileInfo[0]).Tiles[int.Parse(tileInfo[1]), int.Parse(tileInfo[2])].Properties[tileInfo[3]] = kvp[1];
                            }
                        }

                    }
                }
            }
            if (tile.Properties.TryGetValue(soundKey, out value))
            {
                farmer.currentLocation.playSound(value);
            }
            if (tile.Properties.TryGetValue(soundOnceKey, out value))
            {
                tile.Properties.Remove(soundOnceKey);
                farmer.currentLocation.playSound(value);
            }
            if (tile.Properties.TryGetValue(messageKey, out value))
            {
                Game1.drawObjectDialogue(value);
            }
            if (tile.Properties.TryGetValue(messageOnceKey, out value))
            {
                tile.Properties.Remove(messageOnceKey);
                Game1.drawObjectDialogue(value);
            }
            if (tile.Properties.TryGetValue(eventKey, out value))
            {
                Game1.currentLocation.currentEvent = new Event(value, -1, null);
                Game1.currentLocation.checkForEvents();
            }
            if (tile.Properties.TryGetValue(eventOnceKey, out value))
            {
                tile.Properties.Remove(eventOnceKey);
                Game1.currentLocation.currentEvent = new Event(value, -1, null);
                Game1.currentLocation.checkForEvents();
            }
            if (tile.Properties.TryGetValue(musicKey, out value))
            {
                Game1.changeMusicTrack(value);
            }
            if (tile.Properties.TryGetValue(mailKey, out value))
            {
                tile.Properties.Remove(mailKey);
                if (!farmer.mailReceived.Contains(value))
                    farmer.mailReceived.Add(value);
            }
            if (tile.Properties.TryGetValue(teleportKey, out value))
            {
                var split = value.ToString().Split(' ');
                if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                    return;
                farmer.Position = new Vector2(x, y);
            }
            if (tile.Properties.TryGetValue(giveKey, out value))
            {
                tile.Properties.Remove(giveKey);
                Item item = null;
                if(int.TryParse(value, out number))
                {
                    item = new Object(number, 1);
                }
                else
                {
                    foreach(var kvp in Game1.objectInformation)
                    {
                        if(kvp.Value.StartsWith(value + "/"))
                        {
                            item = new Object(kvp.Key, 1);
                        }
                    }
                }
                if (item is null)
                    return;
                farmer.holdUpItemThenMessage(item, false);
                if (!Game1.player.addItemToInventoryBool(item, false))
                {
                    Game1.createItemDebris(item, Game1.player.getStandingPosition(), 1, null, -1);
                }
            }
            if (tile.Properties.TryGetValue(healthKey, out value) && int.TryParse(value, out number))
            {
                if (number < 0)
                {
                    Game1.player.takeDamage(Math.Abs(number), false, null);
                }
                else
                {
                    Game1.player.health = Math.Min(Game1.player.health + number, Game1.player.maxHealth);
                    Game1.player.currentLocation.debris.Add(new Debris(number, new Vector2((float)(Game1.player.getStandingX() + 8), (float)Game1.player.getStandingY()), Color.LimeGreen, 1f, Game1.player));
                }
            }
            if (tile.Properties.TryGetValue(staminaKey, out value) && int.TryParse(value, out number))
            {
                Game1.player.Stamina += number;
            }
            if (tile.Properties.TryGetValue(buffKey, out value) && int.TryParse(value, out number))
            {
                Game1.buffsDisplay.addOtherBuff(new Buff(number));
            }

        }

        private static void DoStepOffActions(Farmer farmer, Point tilePos)
        {
            if (!farmer.currentLocation.isTileOnMap(tilePos.ToVector2()))
                return;

            var tile = farmer.currentLocation.Map.GetLayer("Back").Tiles[tilePos.X, tilePos.Y];

            if (tile is null)
                return;

            if (tile.Properties.TryGetValue(changeIndexOffKey, out PropertyValue value) && int.TryParse(value, out int number))
            {
                tile.Layer.Tiles[tilePos.X, tilePos.Y].TileIndex = number;
            }
            if (tile.Properties.TryGetValue(changeMultipleIndexOffKey, out value))
            {
                var split = value.ToString().Split(',');
                foreach (var str in split)
                {
                    var ss = str.Split(' ');
                    if (ss.Length == 3)
                    {
                        tile.Layer.Tiles[int.Parse(ss[0]), int.Parse(ss[1])].TileIndex = int.Parse(ss[2]);
                    }
                    else if (ss.Length == 4)
                    {
                        farmer.currentLocation.Map.GetLayer(ss[0]).Tiles[int.Parse(ss[1]), int.Parse(ss[2])].TileIndex = int.Parse(ss[3]);
                    }
                }
            }
            if (tile.Properties.TryGetValue(soundOffKey, out value))
            {
                farmer.currentLocation.playSound(value);
            }
            if (tile.Properties.TryGetValue(soundOffOnceKey, out value))
            {
                tile.Properties.Remove(soundOffOnceKey);
                farmer.currentLocation.playSound(value);
            }
            if (tile.Properties.TryGetValue(changePropertiesOffKey, out value))
            {
                var split = value.ToString().Split('`');
                foreach (var str in split)
                {
                    var ss = str.Split("=");
                    if (ss.Length == 2)
                    {
                        tile.Properties[ss[0]] = ss[1];
                    }
                    else
                    {
                        tile.Properties.Remove(ss[0]);
                    }
                }
            }
            if (tile.Properties.TryGetValue(changeMultiplePropertiesOffKey, out value))
            {
                var tiles = value.ToString().Split('|');
                foreach (var tileProp in tiles)
                {
                    var props = tileProp.Split('`');
                    foreach (var prop in props)
                    {
                        var kvp = prop.Split("=");
                        if (kvp.Length == 2)
                        {
                            var tileInfo = kvp[0].Split(",");
                            if (tileInfo.Length == 3)
                            {
                                tile.Layer.Tiles[int.Parse(tileInfo[0]), int.Parse(tileInfo[1])].Properties[tileInfo[2]] = kvp[1];
                            }
                            else if (tileInfo.Length == 4)
                            {
                                farmer.currentLocation.Map.GetLayer(tileInfo[0]).Tiles[int.Parse(tileInfo[1]), int.Parse(tileInfo[2])].Properties[tileInfo[3]] = kvp[1];
                            }
                        }

                    }
                }

            }
        }


        public static void PushTile(GameLocation location, Tile tile, int dir, Point start, string sound)
        {
            var startTile = new Point(start.X / 64, start.Y / 64);

            if (!location.isTileOnMap(startTile.ToVector2()) || !location.isTileOnMap((startTile + GetNextTile(dir)).ToVector2()))
                return;
            if(!string.IsNullOrEmpty(sound))
            {
                location.playSound(sound);
            }

            if (!pushingDict.TryGetValue(location.Name, out List<PushedTile> tiles))
            {
                pushingDict.Add(location.Name, tiles = new List<PushedTile>());
            }

            tiles.Add(new PushedTile() { tile = tile, position = start, dir = dir });
            location.Map.GetLayer("Buildings").Tiles[new Location(start.X / 64, start.Y / 64)] = null;
        }
        public static Point GetNextTile(int dir)
        {
            switch (dir)
            {
                case 0:
                    return new Point(0, -1);
                case 1:
                    return new Point(1, 0);
                case 2:
                    return new Point(0, 1);
                default:
                    return new Point(-1, 0);
            }
        }
    }
}