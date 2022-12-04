using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using static StardewValley.Minigames.TargetGame;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DynamicMapTiles
{
    public partial class ModEntry
    {
        private static void DoStepOnActions(Farmer farmer, Point tilePos)
        {
            TriggerActions(new List<Layer>() { farmer.currentLocation.Map.GetLayer("Back") }, farmer, tilePos, new List<string>() { "On" });
        }

        private static void DoStepOffActions(Farmer farmer, Point tilePos)
        {
            TriggerActions(new List<Layer>() { farmer.currentLocation.Map.GetLayer("Back") }, farmer, tilePos, new List<string>() { "Off" });
        }

        public static bool TriggerActions(List<Layer> layers, Farmer farmer, Point tilePos, List<string> postfixes)
        {
            if (!farmer.currentLocation.isTileOnMap(tilePos.ToVector2()))
                return false;
            bool triggered = false;
            foreach (var layer in layers)
            {
                var tile = layer.Tiles[tilePos.X, tilePos.Y];

                if (tile is null)
                    continue;
                foreach(var k in tile.Properties.Keys.ToArray())
                {
                    string key = k;
                    if (postfixes is not null && postfixes.Any())
                    {
                        bool found = false;
                        foreach(var postfix in postfixes)
                        {
                            if (key.EndsWith(postfix))
                            {
                                key = key.Substring(0, key.Length - postfix.Length);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            continue;
                    }
                    bool remove = false;
                    if (key.EndsWith("Once"))
                    {
                        remove = true;
                        key = key.Substring(0, key.Length - 4);
                    }
                    if (!actionKeys.Contains(key) || !tile.Properties.TryGetValue(k, out PropertyValue v))
                        continue;
                    triggered = true;
                    SMonitor.Log($"Triggering property {key}");
                    string value = v?.ToString();
                    if (remove)
                    {
                        SMonitor.Log($"Removing property from tile");
                        tile.Properties.Remove(k);
                    }
                    try
                    {
                        int number;
                        if (key == addLayerKey)
                        {
                            AddLayer(farmer.currentLocation.map, value);
                        }
                        else if (key == addTilesheetKey)
                        {
                            var split = value.Split(',');
                            if (split.Length == 2)
                            {
                                AddTilesheet(farmer.currentLocation.map, split[0], split[1]);
                            }
                        }
                        else if (key == changeIndexKey)
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
                                if(l is null)
                                {
                                    l = AddLayer(farmer.currentLocation.Map, layerXY[0]);
                                }
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
                                        var l = farmer.currentLocation.Map.GetLayer(tileInfo[0]);
                                        if (l is null)
                                        {
                                            l = AddLayer(farmer.currentLocation.Map, tileInfo[0]);
                                        }
                                        l.Tiles[int.Parse(tileInfo[1]), int.Parse(tileInfo[2])].Properties[tileInfo[3]] = pair[1];
                                    }
                                }
                            }
                        }
                        else if (key == soundKey)
                        {
                            var split = value.Split('|');
                            for(int i = 0; i < split.Length; i++)
                            {
                                if (split[i].Contains(","))
                                {
                                    var split2 = split[i].Split(',');
                                    if (int.TryParse(split2[1], out int delay))
                                    {
                                        DelayedAction.playSoundAfterDelay(split2[0], delay, farmer.currentLocation, -1);
                                    }
                                }
                                else
                                {
                                    if(i == 0)
                                    {
                                        farmer.currentLocation.playSound(split[i]);
                                    }
                                    else
                                    {
                                        DelayedAction.playSoundAfterDelay(split[i], i * 300, farmer.currentLocation, -1);
                                    }
                                }
                            }
                        }
                        else if (key == messageKey)
                        {
                            Game1.drawObjectDialogue(value);
                        }
                        else if (key == eventKey)
                        {
                            Game1.currentLocation.currentEvent = new Event(value, -1, null);
                            Game1.currentLocation.checkForEvents();
                        }
                        else if (key == musicKey)
                        {
                            Game1.changeMusicTrack(value);
                        }
                        else if (key == mailKey)
                        {
                            if (!farmer.mailReceived.Contains(value))
                                farmer.mailReceived.Add(value);
                        }
                        else if (key == mailRemoveKey)
                        {
                            if (farmer.mailReceived.Contains(value))
                                farmer.mailReceived.Remove(value);
                        }
                        else if (key == mailBoxKey)
                        {
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
                                continue;
                            farmer.Position = new Vector2(x, y);
                        }
                        else if (key == teleportTileKey)
                        {
                            var split = value.ToString().Split(' ');
                            if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                                continue;
                            farmer.Position = new Vector2(x * 64, y * 64);
                        }
                        else if (key == giveKey)
                        {
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
                                continue;
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
                        else if (key == emoteKey && int.TryParse(value, out number))
                        {
                            farmer.doEmote(number);
                        }
                        else if (key == explosionKey)
                        {
                            var split = value.Split(' ');
                            farmer.currentLocation.playSound("explosion");
                            farmer.currentLocation.explode(new Vector2(int.Parse(split[0]), int.Parse(split[1])), int.Parse(split[2]), farmer, bool.Parse(split[3]), int.Parse(split[4]));
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
                        }
                        else if (key == animationKey)
                        {
                            var split = value.Split('|');
                            foreach(var str in split)
                            {
                                var split2 = str.Split(',');
                                TemporaryAnimatedSprite sprite = null;
                                if (int.TryParse(split2[0], out int index))
                                {
                                    sprite = new TemporaryAnimatedSprite(index, new Vector2(int.Parse(split2[1]), int.Parse(split2[2])), new Color(byte.Parse(split2[3]), byte.Parse(split2[4]), byte.Parse(split2[5]), byte.Parse(split2[6])), int.Parse(split2[7]), bool.Parse(split2[8]), float.Parse(split2[9], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(split2[10]), int.Parse(split2[11]), float.Parse(split2[12], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(split2[13]), int.Parse(split2[14]))
                                    {
                                        layerDepth = float.Parse(split2[12], NumberStyles.Any, CultureInfo.InvariantCulture),
                                        id = int.Parse(split2[15])
                                    };
                                }
                                else
                                {
                                    // string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, float layerDepth, float alphaFade, Color color, float scale, float scaleChange, float rotation, float rotationChange, bool local

                                    sprite = new TemporaryAnimatedSprite(split2[0], new Rectangle(int.Parse(split2[1]), int.Parse(split2[2]), int.Parse(split2[3]), int.Parse(split2[4])), float.Parse(split2[5], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(split2[6]), int.Parse(split2[7]), new Vector2(int.Parse(split2[8]), int.Parse(split2[9])), bool.Parse(split2[10]), bool.Parse(split2[11]), float.Parse(split2[12], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(split2[13], NumberStyles.Any, CultureInfo.InvariantCulture), new Color(byte.Parse(split2[14]), byte.Parse(split2[15]), byte.Parse(split2[16]), byte.Parse(split2[17])), float.Parse(split2[18], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(split2[19], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(split2[20], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(split2[21], NumberStyles.Any, CultureInfo.InvariantCulture), bool.Parse(split2[22]))
                                    {
                                        layerDepth = float.Parse(split2[12], NumberStyles.Any, CultureInfo.InvariantCulture),
                                        motion = new Vector2(float.Parse(split2[23], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(split2[24], NumberStyles.Any, CultureInfo.InvariantCulture)),
                                        acceleration = new Vector2(float.Parse(split2[25], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(split2[26], NumberStyles.Any, CultureInfo.InvariantCulture)),
                                        delayBeforeAnimationStart = int.Parse(split2[27]),
                                        id = int.Parse(split2[28])
                                    };
                                }
                                if(sprite is not null)
                                {
                                    farmer.currentLocation.removeTemporarySpritesWithIDLocal(sprite.id);
                                    farmer.currentLocation.TemporarySprites.Add(sprite);
                                }
                            }
                        }
                        else if (key == pushKey)
                        {
                            PushTileWithOthers(farmer, tile, tilePos);
                        }
                        else if (key == takeKey && farmer.ActiveObject is not null && (string.IsNullOrEmpty(value) || farmer.ActiveObject.ParentSheetIndex+"" == value || farmer.ActiveObject.Name == value))
                        {
                            farmer.reduceActiveItemByOne();
                        }
                        else if (key == pushOthersKey)
                        {
                            var split = value.Split(',');
                            foreach(var str in split)
                            {
                                Layer l = tile.Layer;
                                var split2 = str.Split(' ');
                                if (split2.Length == 3)
                                {
                                    l = farmer.currentLocation.Map.GetLayer(split2[0]);
                                    PushTileWithOthers(farmer, l.Tiles[int.Parse(split2[1]), int.Parse(split2[2])], tilePos);
                                }
                                else
                                {
                                    PushTileWithOthers(farmer, l.Tiles[int.Parse(split2[0]), int.Parse(split2[1])], tilePos);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        SMonitor.Log($"Error triggering {key} with value {value} at {tilePos}:\n\n{ex.ToString()}", StardewModdingAPI.LogLevel.Error);
                    }

                }
            }
            return triggered;
        }

        private static Layer AddLayer(Map map, string id)
        {
            var layer = new Layer(id, map, map.Layers[0].LayerSize, Layer.m_tileSize);
            map.AddLayer(layer);
            return layer;
        }
        private static TileSheet AddTilesheet(Map map, string id, string texturePath)
        {
            var texture = SHelper.GameContent.Load<Texture2D>(texturePath);
            if (texture == null)
                return null;
            var tilesheet = new TileSheet(id, map, texturePath, new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));
            map.AddTileSheet(tilesheet);
            return tilesheet;
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


        private static void PushTileWithOthers(Farmer farmer, Tile tile, Point startTile)
        {
            List<(Point, Tile)> tileList = new() { (startTile, tile) };
            if (tile.Properties.TryGetValue(pushAlsoKey, out PropertyValue others))
            {
                var split = others.ToString().Split(',');
                foreach (var t in split)
                {
                    try
                    {
                        var split2 = t.Split(' ');
                        if (split2.Length == 3 && int.TryParse(split2[1], out int x2) && int.TryParse(split2[2], out int y2))
                        {
                            x2 += startTile.X;
                            y2 += startTile.Y;
                            var layer = farmer.currentLocation.Map.GetLayer(split2[0]);
                            if (layer is not null && farmer.currentLocation.isTileOnMap(x2, y2) && layer.Tiles[x2, y2] is not null)
                            {
                                tileList.Add((new Point(x2, y2), layer.Tiles[x2, y2]));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SMonitor.Log(ex.ToString(), StardewModdingAPI.LogLevel.Error);
                    }
                }
            }
            PushTiles(farmer.currentLocation, tileList, farmer.FacingDirection, farmer);
        }

        public static bool PushTiles(GameLocation location, List<(Point, Tile)> tileList, int dir, Farmer farmer)
        {
            for(int i = 0; i < tileList.Count; i++)
            {
                Point destTile = tileList[i].Item1 + GetNextTile(dir);
                if (!location.isTileOnMap(tileList[i].Item1.ToVector2()) || !location.isTileOnMap(destTile.ToVector2()) || 
                    (
                        tileList[i].Item2.Layer.Tiles[destTile.X, destTile.Y] is not null &&
                        (tileList[i].Item2.Layer.Tiles[destTile.X, destTile.Y].Properties.ContainsKey(pushKey) || tileList[i].Item2.Layer.Tiles[destTile.X, destTile.Y].Properties.ContainsKey(pushableKey)) &&
                        !tileList.Where(t => t.Item1 == destTile && t.Item2.Layer.Id == tileList[i].Item2.Layer.Id).Any()
                    )
                )
                    return false;
            }
            if (!pushingDict.TryGetValue(location.Name, out List<PushedTile> pushedList))
            {
                pushingDict.Add(location.Name, pushedList = new List<PushedTile>());
            }
            for (int i = 0; i < tileList.Count; i++)
            {
                if (tileList[i].Item2.Layer.Id == "Buildings")
                {
                    TriggerActions(new List<Layer>() { tileList[i].Item2.Layer }, farmer, tileList[i].Item1, new List<string>() { "Push" });
                }
                pushedList.Add(new PushedTile() { tile = tileList[i].Item2, position = new Point(tileList[i].Item1.X * 64, tileList[i].Item1.Y * 64), dir = dir, farmer = farmer });
                tileList[i].Item2.Layer.Tiles[new Location(tileList[i].Item1.X, tileList[i].Item1.Y)] = null;
            }
            return true;
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