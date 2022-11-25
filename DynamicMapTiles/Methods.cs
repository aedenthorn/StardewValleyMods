using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
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
                foreach(var kvp in tile.Properties)
                {
                    if (!actions.Contains(kvp.Key))
                        continue;
                    string value = kvp.Value.ToString();
                    int number;
                    if (kvp.Key == changeIndexKey)
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
                    else if (kvp.Key == changeMultipleIndexKey)
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
                    else if (kvp.Key == changePropertiesKey)
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
                    else if (kvp.Key == changeMultiplePropertiesKey)
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
                    else if (kvp.Key == soundKey)
                    {
                        farmer.currentLocation.playSound(value);
                    }
                    else if (kvp.Key == soundOnceKey)
                    {
                        tile.Properties.Remove(soundOnceKey);
                        farmer.currentLocation.playSound(value);
                    }
                    else if (kvp.Key == messageKey)
                    {
                        Game1.drawObjectDialogue(value);
                    }
                    else if (kvp.Key == messageOnceKey)
                    {
                        tile.Properties.Remove(messageOnceKey);
                        Game1.drawObjectDialogue(value);
                    }
                    else if (kvp.Key == eventKey)
                    {
                        Game1.currentLocation.currentEvent = new Event(value, -1, null);
                        Game1.currentLocation.checkForEvents();
                    }
                    else if (kvp.Key == eventOnceKey)
                    {
                        tile.Properties.Remove(eventOnceKey);
                        Game1.currentLocation.currentEvent = new Event(value, -1, null);
                        Game1.currentLocation.checkForEvents();
                    }
                    else if (kvp.Key == musicKey)
                    {
                        Game1.changeMusicTrack(value);
                    }
                    else if (kvp.Key == mailKey)
                    {
                        tile.Properties.Remove(mailKey);
                        if (!farmer.mailReceived.Contains(value))
                            farmer.mailReceived.Add(value);
                    }
                    else if (kvp.Key == mailBoxKey)
                    {
                        tile.Properties.Remove(mailKey);
                        if (!farmer.mailbox.Contains(value))
                            farmer.mailbox.Add(value);
                    }
                    else if (kvp.Key == teleportKey)
                    {
                        var split = value.ToString().Split(' ');
                        if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                            return;
                        farmer.Position = new Vector2(x, y);
                    }
                    else if (kvp.Key == teleportTileKey)
                    {
                        var split = value.ToString().Split(' ');
                        if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                            return;
                        farmer.Position = new Vector2(x * 64, y * 64);
                    }
                    else if (kvp.Key == giveKey)
                    {
                        tile.Properties.Remove(giveKey);
                        Item item = null;
                        if (!value.ToString().Contains("/"))
                        {
                            if (int.TryParse(value, out number))
                            {
                                item = new Object(number, 1);
                            }
                            else
                            {
                                foreach (var pair in Game1.objectInformation)
                                {
                                    if (pair.Value.StartsWith(value + "/"))
                                    {
                                        item = new Object(pair.Key, 1);
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
                                            }
                                        }
                                    }
                                    break;
                                case "Money":
                                    if (int.TryParse(split[1], out number))
                                    {
                                        farmer.addUnearnedMoney(number);
                                    }
                                    break;
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
                    else if (kvp.Key == healthKey && int.TryParse(value, out number))
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
                    else if (kvp.Key == staminaKey && int.TryParse(value, out number))
                    {
                        Game1.player.Stamina += number;
                    }
                    else if (kvp.Key == buffKey && int.TryParse(value, out number))
                    {
                        Game1.buffsDisplay.addOtherBuff(new Buff(number));
                    }
                    else if (kvp.Key == emoteKey && int.TryParse(value, out number))
                    {
                        farmer.doEmote(number);
                    }


                    else if (kvp.Key == changeIndexOffKey && int.TryParse(value, out number))
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
                    else if (kvp.Key == changeMultipleIndexOffKey)
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
                    else if (kvp.Key == changePropertiesOffKey)
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
                    else if (kvp.Key == changeMultiplePropertiesOffKey)
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
                    else if (kvp.Key == soundOffKey)
                    {
                        farmer.currentLocation.playSound(value);
                    }
                    else if (kvp.Key == soundOffOnceKey)
                    {
                        tile.Properties.Remove(soundOffOnceKey);
                        farmer.currentLocation.playSound(value);
                    }

                    else if (kvp.Key == triggerKey || kvp.Key == triggerOnceKey)
                    {
                        var split = value.Split('|');
                        foreach(var s in split)
                        {
                            var split2 = s.Split('/');
                            var ls = new List<Layer>();
                            Point point;
                            if (split2.Length > 1)
                            {
                                var split3 = split2[1].Split(' ');
                                ls.Add(farmer.currentLocation.Map.GetLayer(split2[0]));
                                try
                                {
                                    point = new Point(int.Parse(split3[0]), int.Parse(split3[1]));
                                }
                                catch 
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                var split3 = split2[0].Split(' ');
                                try
                                {
                                    point = new Point(int.Parse(split3[0]), int.Parse(split3[1]));
                                }
                                catch
                                {
                                    continue;
                                }
                                ls.AddRange(farmer.currentLocation.Map.Layers);
                            }
                            TriggerActions(triggerKeys, ls, farmer, point);
                        }
                        if (kvp.Key == triggerOnceKey)
                        {
                            tile.Properties.Remove(triggerOnceKey);
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