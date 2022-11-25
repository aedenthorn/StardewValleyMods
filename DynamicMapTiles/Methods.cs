using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace DynamicMapTiles
{
    public partial class ModEntry
    {
        private static void DoStepOnActions(Farmer farmer, Point tilePos)
        {
            TriggerActions(stepOnKeys, new List<Layer>() { farmer.currentLocation.Map.GetLayer("Back") }, farmer, tilePos);
        }

        private static void DoStepOffActions(Farmer farmer, Point tilePos)
        {
            TriggerActions(stepOffKeys, new List<Layer>() { farmer.currentLocation.Map.GetLayer("Back") }, farmer, tilePos);
        }

        public static void TriggerActions(List<string> actions, List<Layer> layers, Farmer farmer, Point tilePos)
        {
            if (!farmer.currentLocation.isTileOnMap(tilePos.ToVector2()))
                return;

            foreach (var layer in layers)
            {
                var tile = layer.Tiles[tilePos.X, tilePos.Y];

                if (tile is null)
                    continue;
                foreach(var key in tile.Properties.Keys.ToArray())
                {
                    if (!actions.Contains(key) || !tile.Properties.TryGetValue(key, out PropertyValue v))
                        continue;
                    string value = v?.ToString();
                    try
                    {
                        int number;
                        if (key == changeIndexKey)
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                tile.Layer.Tiles[tilePos.X, tilePos.Y] = null;
                            }
                            else if (int.TryParse(value, out number))
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
                            else if (value.ToString().Contains(','))
                            {
                                var indexesDuration = value.ToString().Split(',');
                                var indexes = indexesDuration[0].Split(' ');
                                List<StaticTile> tiles = new List<StaticTile>();
                                foreach (var index in indexes)
                                {
                                    if (int.TryParse(index, out number))
                                    {
                                        tiles.Add(new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, number));
                                    }
                                    else if (index.ToString().Contains('/'))
                                    {
                                        var sheetIndex = index.ToString().Split('/');
                                        tiles.Add(new StaticTile(tile.Layer, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                                    }

                                }
                                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new AnimatedTile(tile.Layer, tiles.ToArray(), int.Parse(indexesDuration[1]));
                            }
                            else if (value.ToString().Contains('/'))
                            {
                                var sheetIndex = value.ToString().Split('/');
                                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new StaticTile(tile.Layer, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
                            }
                        }
                        else if (key == changeMultipleIndexKey)
                        {
                            var tileInfos = value.ToString().Split('|');
                            foreach (var tileInfo in tileInfos)
                            {
                                var pair = tileInfo.Split('=');
                                var layerXY = pair[0].Split(' ');
                                var l = farmer.currentLocation.Map.GetLayer(layerXY[0]);
                                if (string.IsNullOrEmpty(pair[1]))
                                {
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = null;
                                }
                                else if (int.TryParse(pair[1], out number))
                                {
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new StaticTile(l, tile.TileSheet, BlendMode.Alpha, number);
                                }
                                else if (pair[1].ToString().Contains(','))
                                {
                                    var indexesDuration = pair[1].ToString().Split(',');
                                    var indexes = indexesDuration[0].Split(' ');
                                    List<StaticTile> tiles = new List<StaticTile>();
                                    foreach (var index in indexes)
                                    {
                                        if (int.TryParse(index, out number))
                                        {
                                            tiles.Add(new StaticTile(l, tile.TileSheet, BlendMode.Alpha, number));
                                        }
                                        else if (index.ToString().Contains('/'))
                                        {
                                            var sheetIndex = index.ToString().Split('/');
                                            tiles.Add(new StaticTile(l, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                                        }

                                    }
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new AnimatedTile(l, tiles.ToArray(), int.Parse(indexesDuration[1]));
                                }
                                else if (pair[1].ToString().Contains('/'))
                                {
                                    var sheetIndex = pair[1].ToString().Split('/');
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new StaticTile(tile.Layer, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
                                }
                            }
                        }
                        else if (key == changePropertiesKey)
                        {
                            var props = value.ToString().Split('|');
                            foreach (var prop in props)
                            {
                                var pair = prop.Split('=');
                                if (pair.Length == 2)
                                {
                                    tile.Properties[pair[0]] = pair[1];
                                }
                                else
                                {
                                    tile.Properties.Remove(pair[0]);
                                }
                            }
                        }
                        else if (key == changeMultiplePropertiesKey)
                        {
                            var tiles = value.ToString().Split('|');
                            foreach (var prop in tiles)
                            {
                                var pair = prop.Split('=');
                                if (pair.Length == 2)
                                {
                                    var tileInfo = pair[0].Split(',');
                                    if (tileInfo.Length == 3)
                                    {
                                        tile.Layer.Tiles[int.Parse(tileInfo[0]), int.Parse(tileInfo[1])].Properties[tileInfo[2]] = pair[1];
                                    }
                                    else if (tileInfo.Length == 4)
                                    {
                                        farmer.currentLocation.Map.GetLayer(tileInfo[0]).Tiles[int.Parse(tileInfo[1]), int.Parse(tileInfo[2])].Properties[tileInfo[3]] = pair[1];
                                    }
                                }
                            }
                        }
                        else if (key == soundKey)
                        {
                            farmer.currentLocation.playSound(value);
                        }
                        else if (key == soundOnceKey)
                        {
                            tile.Properties.Remove(soundOnceKey);
                            farmer.currentLocation.playSound(value);
                        }
                        else if (key == messageKey)
                        {
                            Game1.drawObjectDialogue(value);
                        }
                        else if (key == messageOnceKey)
                        {
                            tile.Properties.Remove(messageOnceKey);
                            Game1.drawObjectDialogue(value);
                        }
                        else if (key == eventKey)
                        {
                            Game1.currentLocation.currentEvent = new Event(value, -1, null);
                            Game1.currentLocation.checkForEvents();
                        }
                        else if (key == eventOnceKey)
                        {
                            tile.Properties.Remove(eventOnceKey);
                            Game1.currentLocation.currentEvent = new Event(value, -1, null);
                            Game1.currentLocation.checkForEvents();
                        }
                        else if (key == musicKey)
                        {
                            Game1.changeMusicTrack(value);
                        }
                        else if (key == mailKey)
                        {
                            tile.Properties.Remove(mailKey);
                            if (!farmer.mailReceived.Contains(value))
                                farmer.mailReceived.Add(value);
                        }
                        else if (key == mailBoxKey)
                        {
                            tile.Properties.Remove(mailKey);
                            if (!farmer.mailbox.Contains(value))
                                farmer.mailbox.Add(value);
                        }
                        else if (key == invalidateKey)
                        {
                            var split = value.Split('|');
                            foreach(var str in split)
                            {
                                SHelper.GameContent.InvalidateCache(str);
                            }
                        }
                        else if (key == teleportKey)
                        {
                            var split = value.ToString().Split(' ');
                            if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                                return;
                            farmer.Position = new Vector2(x, y);
                        }
                        else if (key == teleportTileKey)
                        {
                            var split = value.ToString().Split(' ');
                            if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                                return;
                            farmer.Position = new Vector2(x * 64, y * 64);
                        }
                        else if (key == giveKey)
                        {
                            tile.Properties.Remove(giveKey);
                            Item item = null;
                            if (value.StartsWith("Money/"))
                            {
                                if (int.TryParse(value.Split('/')[1], out number))
                                    farmer.addUnearnedMoney(number);
                            }
                            else
                            {
                                item = GetItemFromString(value);
                            }
                            if (item is null)
                                return;
                            farmer.holdUpItemThenMessage(item, false);
                            if (!Game1.player.addItemToInventoryBool(item, false))
                            {
                                Game1.createItemDebris(item, Game1.player.getStandingPosition(), 1, null, -1);
                            }
                        }
                        else if (key == healthKey && int.TryParse(value, out number))
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
                        else if (key == staminaKey && int.TryParse(value, out number))
                        {
                            Game1.player.Stamina += number;
                        }
                        else if (key == buffKey && int.TryParse(value, out number))
                        {
                            Game1.buffsDisplay.addOtherBuff(new Buff(number));
                        }
                        else if ((key == emoteKey || key == emoteOnceKey) && int.TryParse(value, out number))
                        {
                            farmer.doEmote(number);
                            if (key == emoteOnceKey)
                            {
                                tile.Properties.Remove(key);
                            }
                        }
                        else if (key == explosionKey || key == explosionOnceKey)
                        {
                            var split = value.Split(' ');
                            farmer.currentLocation.playSound("explosion");
                            farmer.currentLocation.explode(new Vector2(int.Parse(split[0]), int.Parse(split[1])), int.Parse(split[2]), farmer, bool.Parse(split[3]), int.Parse(split[4]));
                            if (key == explosionOnceKey)
                            {
                                tile.Properties.Remove(key);
                            }
                        }
                        else if (key == chestKey)
                        {
                            var split = value.Split('=');
                            var split2 = split[0].Split(' ');
                            Vector2 p = new Vector2(int.Parse(split2[0]), int.Parse(split2[1]));
                            if (!farmer.currentLocation.objects.TryGetValue(p, out Object o) || o is not Chest)
                            {
                                int money = 0;
                                var items = new List<Item>();
                                split2 = split[1].Split(' ');
                                foreach (var str in split2)
                                {
                                    if (str.StartsWith("Money/"))
                                    {
                                        int.TryParse(str.Split('/')[1], out money);
                                    }
                                    else
                                    {
                                        var item = GetItemFromString(str);
                                        if (item is not null)
                                            items.Add(item);
                                    }
                                }
                                var chest = new Chest(true, p, 130);
                                chest.items.AddRange(items);
                                chest.coins.Value = money;
                                chest.Type = "interactive";
                                chest.bigCraftable.Value = false;
                                chest.CanBeSetDown = false;
                                farmer.currentLocation.overlayObjects[p] = chest;
                            }
                            tile.Properties.Remove(key);
                        }

                        // step off

                        else if (key == changeIndexOffKey && int.TryParse(value, out number))
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                tile.Layer.Tiles[tilePos.X, tilePos.Y] = null;
                            }
                            else if (int.TryParse(value, out number))
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
                            else if (value.ToString().Contains(','))
                            {
                                var indexesDuration = value.ToString().Split(',');
                                var indexes = indexesDuration[0].Split(' ');
                                List<StaticTile> tiles = new List<StaticTile>();
                                foreach (var index in indexes)
                                {
                                    if (int.TryParse(index, out number))
                                    {
                                        tiles.Add(new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, number));
                                    }
                                    else if (index.ToString().Contains('/'))
                                    {
                                        var sheetIndex = index.ToString().Split('/');
                                        tiles.Add(new StaticTile(tile.Layer, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                                    }

                                }
                                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new AnimatedTile(tile.Layer, tiles.ToArray(), int.Parse(indexesDuration[1]));
                            }
                            else if (value.ToString().Contains('/'))
                            {
                                var sheetIndex = value.ToString().Split('/');
                                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new StaticTile(tile.Layer, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
                            }
                        }
                        else if (key == changeMultipleIndexOffKey)
                        {
                            var tileInfos = value.ToString().Split('|');
                            foreach (var tileInfo in tileInfos)
                            {
                                var pair = tileInfo.Split('=');
                                var layerXY = pair[0].Split(' ');
                                var l = farmer.currentLocation.Map.GetLayer(layerXY[0]);
                                if (string.IsNullOrEmpty(pair[1]))
                                {
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = null;
                                }
                                else if (int.TryParse(pair[1], out number))
                                {
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])].TileIndex = number;
                                }
                                else if (pair[1].ToString().Contains(','))
                                {
                                    var indexesDuration = pair[1].ToString().Split(',');
                                    var indexes = indexesDuration[0].Split(' ');
                                    List<StaticTile> tiles = new List<StaticTile>();
                                    foreach (var index in indexes)
                                    {
                                        if (int.TryParse(index, out number))
                                        {
                                            tiles.Add(new StaticTile(l, tile.TileSheet, BlendMode.Alpha, number));
                                        }
                                        else if (index.ToString().Contains('/'))
                                        {
                                            var sheetIndex = index.ToString().Split('/');
                                            tiles.Add(new StaticTile(l, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                                        }

                                    }
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new AnimatedTile(l, tiles.ToArray(), int.Parse(indexesDuration[1]));
                                }
                                else if (pair[1].ToString().Contains('/'))
                                {
                                    var sheetIndex = pair[1].ToString().Split('/');
                                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new StaticTile(tile.Layer, farmer.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
                                }
                            }
                        }
                        else if (key == changePropertiesOffKey)
                        {
                            var props = value.ToString().Split('|');
                            foreach (var prop in props)
                            {
                                var pair = prop.Split('=');
                                if (pair.Length == 2)
                                {
                                    tile.Properties[pair[0]] = pair[1];
                                }
                                else
                                {
                                    tile.Properties.Remove(pair[0]);
                                }
                            }
                        }
                        else if (key == changeMultiplePropertiesOffKey)
                        {
                            var tiles = value.ToString().Split('|');
                            foreach (var tileProp in tiles)
                            {
                                var pair = tileProp.Split('=');
                                if (pair.Length == 2)
                                {
                                    var tileInfo = pair[0].Split(',');
                                    if (tileInfo.Length == 3)
                                    {
                                        tile.Layer.Tiles[int.Parse(tileInfo[0]), int.Parse(tileInfo[1])].Properties[tileInfo[2]] = pair[1];
                                    }
                                    else if (tileInfo.Length == 4)
                                    {
                                        farmer.currentLocation.Map.GetLayer(tileInfo[0]).Tiles[int.Parse(tileInfo[1]), int.Parse(tileInfo[2])].Properties[tileInfo[3]] = pair[1];
                                    }
                                }
                            }

                        }
                        else if (key == soundOffKey)
                        {
                            farmer.currentLocation.playSound(value);
                        }
                        else if (key == soundOffOnceKey)
                        {
                            tile.Properties.Remove(soundOffOnceKey);
                            farmer.currentLocation.playSound(value);
                        }
                    }
                    catch(Exception ex)
                    {
                        SMonitor.Log($"Error triggering {key} with value {value} at {tilePos}:\n\n{ex.ToString()}", StardewModdingAPI.LogLevel.Error);
                    }

                }
            }
        }

        private static Item GetItemFromString(string value)
        {
            Item item = null;
            int number;
            if (!value.ToString().Contains("/"))
            {
                if (int.TryParse(value, out number))
                {
                    item = new Object(number, 1);
                }
                else
                {
                    var amount = 1;
                    if (value.Contains(","))
                    {
                        var split = value.Split(',');
                        value = split[0];
                        amount = int.Parse(split[1]);
                    }
                    if (int.TryParse(value, out number))
                    {
                        item = new Object(number, amount);
                    }
                    else
                    {
                        foreach (var pair in Game1.objectInformation)
                        {
                            if (pair.Value.StartsWith(value + "/"))
                            {
                                item = new Object(pair.Key, amount);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                var split = value.ToString().Split('/');
                switch (split[0])
                {
                    case "Hat":
                        if (int.TryParse(split[1], out number))
                        {
                            item = new Hat(number);
                        }
                        else
                        {
                            Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\hats");
                            foreach (var pair in dictionary)
                            {
                                if (pair.Value.StartsWith(split[1] + "/"))
                                {
                                    item = new Hat(pair.Key);
                                    break;
                                }
                            }
                        }
                        break;
                    case "Clothing":
                        if (int.TryParse(split[1], out number))
                        {
                            item = new Clothing(number);
                        }
                        else
                        {
                            foreach (var pair in Game1.clothingInformation)
                            {
                                if (pair.Value.StartsWith(split[1] + "/"))
                                {
                                    item = new Clothing(pair.Key);
                                    break;
                                }
                            }
                        }
                        break;
                    case "Craftable":
                        if (int.TryParse(split[1], out number))
                        {
                            item = new Object(Vector2.Zero, number, false);
                        }
                        else
                        {
                            foreach (var pair in Game1.bigCraftablesInformation)
                            {
                                if (pair.Value.StartsWith(split[1] + "/"))
                                {
                                    item = new Object(Vector2.Zero, pair.Key, false);
                                    break;
                                }
                            }
                        }
                        break;
                    case "Furniture":
                        if (int.TryParse(split[1], out number))
                        {
                            item = new Furniture(number, Vector2.Zero);
                        }
                        else
                        {
                            foreach (var pair in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture"))
                            {
                                if (pair.Value.StartsWith(split[1] + "/"))
                                {
                                    item = new Furniture(pair.Key, Vector2.Zero);
                                    break;
                                }
                            }
                        }
                        break;
                    case "Weapon":
                        if (int.TryParse(split[1], out number))
                        {
                            item = new MeleeWeapon(number);
                        }
                        else
                        {
                            foreach (var pair in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
                            {
                                if (pair.Value.StartsWith(split[1] + "/"))
                                {
                                    item = new MeleeWeapon(pair.Key);
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
            return item;
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