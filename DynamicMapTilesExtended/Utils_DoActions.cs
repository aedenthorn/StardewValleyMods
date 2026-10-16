using DMT.Data;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Reflection;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using static HarmonyLib.Code;
using static System.Net.WebRequestMethods;
using Object = StardewValley.Object;

namespace DMT
{
    public static partial class Utils
    {
        public static readonly Dictionary<string, Action<Farmer, string, Tile, Point>> ModActions = [];

        public static void DoAction(Farmer? who, string value)
        {
            if (who?.currentLocation == null || string.IsNullOrEmpty(value))
                return;

            var split = value.Split(',');
            if (split.Length != 2 || !int.TryParse(split[0], out var x) || !int.TryParse(split[1], out var y))
                return;
            xLocation tileLocation = tileLocation = new(x, y);

            Tile tile = who.currentLocation.map.RequireLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), Game1.viewport.Size);
            string action;
            if (tile == null || !tile.Properties.TryGetValue("Action", out action))
            {
                action = who.currentLocation.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings", false);
                if (action != null)
                {
                    who.currentLocation.performAction(action, who, tileLocation);
                }
            }
        }
        public static void DoAddLayer(GameLocation location, string value) => AddLayer(location.map, value);

        public static void DoAddTileSheet(GameLocation location, string value)
        {
            var split = value.Split(',');
            if (split.Length == 2)
                AddTileSheet(location.map, split[0], split[1]);
        }
        public static void DoChangeIndex(GameLocation location, string value, Tile tile, Point tilePos)
        {
            if (string.IsNullOrEmpty(value))
            {
                tile.Layer.Tiles[tilePos.X, tilePos.Y] = null;
                return;
            }
            if (int.TryParse(value, out int num))
            {
                if (tile.Layer.Tiles[tilePos.X, tilePos.Y] is null)
                    tile.Layer.Tiles[tilePos.X, tilePos.Y] = new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, num);
                else
                    tile.Layer.Tiles[tilePos.X, tilePos.Y].TileIndex = num;
                return;
            }
            if (value.Contains(','))
            {
                var indecesDuration = value.Split(',');
                var indeces = indecesDuration[0].Split(' ');
                List<StaticTile> tiles = [];
                foreach (var index in indeces)
                {
                    if (int.TryParse(index, out num))
                        tiles.Add(new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, num));
                    else if (index.ToString().Contains('/'))
                    {
                        var sheetIndex = index.ToString().Split('/');
                        tiles.Add(new StaticTile(tile.Layer, location.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                    }

                }
                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new AnimatedTile(tile.Layer, tiles.ToArray(), int.Parse(indecesDuration[1]));
                return;
            }
            if (value.Contains('/'))
            {
                var sheetIndex = value.Split('/');
                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new StaticTile(tile.Layer, location.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
            }
        }

        public static void DoChangeMultipleIndexes(GameLocation location, string value, Tile tile, Point tilePos)
        {
            var tileInfos = value.Split('|');
            foreach (var tileInfo in tileInfos)
            {
                var pair = tileInfo.Split('=');
                if (pair.Length != 2)
                    continue;
                var layerXY = pair[0].Split(' ');
                var l = location.Map.GetLayer(layerXY[0]);
                l ??= AddLayer(location.Map, layerXY[0]);
                if (string.IsNullOrEmpty(pair[1]))
                {
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = null;
                    continue;
                }
                if (int.TryParse(pair[1], out int num))
                {
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new StaticTile(l, tile.TileSheet, BlendMode.Alpha, num);
                    continue;
                }
                if (pair[1].Contains(','))
                {
                    var indexesDuration = pair[1].Split(',');
                    var indexes = indexesDuration[0].Split(' ');
                    List<StaticTile> tiles = [];
                    foreach (var index in indexes)
                    {
                        if (int.TryParse(index, out num))
                            tiles.Add(new StaticTile(l, tile.TileSheet, BlendMode.Alpha, num));
                        else if (index.Contains('/'))
                        {
                            var sheetIndex = index.Split('/');
                            tiles.Add(new StaticTile(l, location.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                        }

                    }
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new AnimatedTile(l, [.. tiles], int.Parse(indexesDuration[1]));
                    continue;
                }
                if (pair[1].Contains('/'))
                {
                    var sheetIndex = pair[1].Split('/');
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new StaticTile(tile.Layer, location.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
                    continue;
                }
            }
        }

        public static void DoChangeProperties(string value, Tile tile)
        {
            var props = value.Split('|');
            foreach (var prop in props)
            {
                var pair = prop.Split('=');
                if (pair.Length == 2)
                    tile.Properties[pair[0]] = pair[1];
                else if (pair.Length == 1)
                    tile.Properties.Remove(pair[0]);
            }
        }


        public static void DoChangeMultipleProperties(GameLocation location, string value, Tile tile)
        {
            var tiles = value.Split('|');
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
                        var l = location.Map.GetLayer(tileInfo[0]) ?? AddLayer(location.Map, tileInfo[0]);
                        l.Tiles[int.Parse(tileInfo[1]), int.Parse(tileInfo[2])].Properties[tileInfo[3]] = pair[1];
                    }
                }
            }
        }
        public static void DoSetModData(GameLocation l, string value)
        {

            var split = value.Split('|');
            foreach(var str in split)
            {
                Vector2 tile = new(int.Parse(split[0]), int.Parse(split[1]));
                Object obj = l.getObjectAtTile((int)tile.X, (int)tile.Y);
                if (obj == null)
                    return;
                obj.modData[split[2]] = split[3];
            }
        }

        public static void DoPlaySound(GameLocation location, string value)
        {
            var split = value.Split('|');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Contains(','))
                {
                    var split2 = split[i].Split(',');
                    if (int.TryParse(split2[1], out int delay))
                        DelayedAction.playSoundAfterDelay(split2[0], delay, location);
                    continue;
                }
                location.playSound(split[i]);
            }
        }

        public static void DoShowMessage(string value)
        {
            if (value.Contains('|'))
            {
                var split = value.Split('|');
                var letter = bool.Parse(split[0].ToLower());
                value = split[1];
                if (letter)
                {
                    Game1.drawLetterMessage(value);
                    return;
                }
            }

            Game1.drawObjectDialogue(value);
        }

        public static void DoSpawnMonster(GameLocation location, string value)
        {
            if (location == null)
            {
                SMonitor.Log($"[{nameof(DoSpawnMonster)}] Location is null", LogLevel.Warn);
                return;
            }
            var cmapi = SHelper.ModRegistry.GetApi<ICustomMonstersAPI>("aedenthorn.CustomMonsters");
            foreach (var item in value.Split('|'))
            {
                var split = item.Split(',');
                if (split.Length == 1)
                {
                    split = item.Split(' ');
                }
                if(split.Length < 3)
                {
                    SMonitor.Log($"[{nameof(DoSpawnMonster)}] Missing parameters in {item}", LogLevel.Warn);
                }
                Vector2 pos = new(int.Parse(split[1]), int.Parse(split[2]));
                if(cmapi != null)
                {
                    var monster = cmapi.CreateMonster(split[0], pos);
                    if (monster != null)
                    {
                        location.characters.Add(monster);
                        continue;
                    }
                }
                if (split[0] == "Monster")
                {
                    if(split.Length == 4)
                    {
                        location.characters.Add(new Monster(split[3], pos));
                    }
                    else if(split.Length == 5 && int.TryParse(split[4], out var facing))
                    {
                        location.characters.Add(new Monster(split[3], pos, facing));
                    }
                }
                else
                {
                    foreach(var t in typeof(Monster).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Monster))))
                    {
                        if (t.Name == split[0])
                        {
                            foreach (var c in t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                            {
                                var p = c.GetParameters();
                                if (split.Length == 3 && p.Length == 1 && p[0].ParameterType == typeof(Vector2))
                                {
                                    location.characters.Add((Monster)c.Invoke([pos]));
                                    break;
                                }
                                else if (split.Length == 4 && p.Length == 2)
                                {
                                    if(p[0].ParameterType == typeof(Vector2))
                                    {
                                        if (TryGetParameter(split[3], p[1], out var p1))
                                        {
                                            location.characters.Add((Monster)c.Invoke([pos, p1]));
                                            break;
                                        }
                                    }
                                    else if (p[1].ParameterType == typeof(Vector2))
                                    {
                                        if (TryGetParameter(split[3], p[0], out var p0))
                                        {
                                            location.characters.Add((Monster)c.Invoke([p0, pos]));
                                            break;
                                        }
                                    }
                                }
                                else if (split.Length == 5 && p.Length == 3 && p[0].ParameterType == typeof(Vector2))
                                {
                                    if (TryGetParameter(split[3], p[1], out var p1) && TryGetParameter(split[4], p[2], out var p2))
                                    {
                                        location.characters.Add((Monster)c.Invoke([pos, p1, p2]));
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    
                }
            }
        }

        public static void DoPlayEvent(string value)
        {
            if (!value.Contains('|'))
            {
                Game1.currentLocation.currentEvent = new(value, null, null);
                Game1.currentLocation.checkForEvents();
                return;
            }
            var split = value.Split('|');
            var eventStr = split[0];
            var assetName = split[1];
            string assetKey = "";
            if (assetName.Contains('#'))
            {
                var split2 = assetName.Split('#');
                assetName = split2[0];
                assetKey = split2[1];
            }
            var eventId = split[2];

            if (!string.IsNullOrWhiteSpace(eventStr) && context.Helper.GameContent.Load<Dictionary<string, string>>(assetName).TryGetValue(assetKey, out eventStr))
            {
                Game1.currentLocation.currentEvent = new(eventStr, assetName, eventId);
                Game1.currentLocation.checkForEvents();
            }
        }

        public static void DoPlayMusic(string value) => Game1.changeMusicTrack(Game1.random.Choose(value.Split('|')));

        public static void DoAddMailflag(Farmer? who, string value) 
        {
            if (who is null)
                return;
            foreach(var item in value.Split('|'))
            {
                who.mailReceived.Add(item);
            }
        }

        public static void DoRemoveMailflag(Farmer? who, string value)
        {
            if (who is null)
                return;
            foreach (var item in value.Split('|'))
            {
                who.mailReceived.Remove(item);
            }
        }

        public static void DoAddMailForTomorrow(Farmer? who, string value)
        {
            if (who == null)
                return;
            foreach (var item in value.Split('|'))
            {
                if (!who.hasOrWillReceiveMail(item))
                    who.mailbox.Add(item);
            }
        }

        public static void DoAddQuest(Farmer? who, string value)
        {
            if (who == null)
                return;
            foreach (var item in value.Split('|'))
            {
                who.addQuest(item);
            }
        }

        public static void DoRemoveQuest(Farmer? who, string value)
        {
            if (who == null)
                return; 
            foreach (var item in value.Split('|'))
            {
                who.removeQuest(item);
            }
        }

        public static void DoChangeAppearance(Farmer? who, string value)
        {
            if (who == null)
                return;
            var changes = value.Split("|");
            foreach (var item in changes)
            {
                var split = item.Split(",");
                if(split.Length != 2) continue;
                bool isNum = int.TryParse(split[1], out var i);
                switch (split[0].ToLower())
                {
                    case "hairstyle":
                        if (isNum)
                        {
                            who.changeHairStyle(i);
                        }
                        break;
                    case "haircolor":
                        Color? c = Utility.StringToColor(split[1]);
                        if (c is not null)
                            who.changeHairColor(c.Value);
                        break;
                    case "eyecolor":
                        Color? c2 = Utility.StringToColor(split[1]);
                        if (c2 is not null)
                            who.changeEyeColor(c2.Value);
                        break;
                    case "accessory":
                        if (isNum)
                        {
                            who.changeAccessory(i);
                        }
                        break;
                    case "skincolor":
                        if (isNum)
                        {
                            who.changeSkinColor(i);
                        }
                        break;
                    case "gender":
                        if (isNum)
                        {
                            who.changeGender(split[1].ToLower() == "male");
                        }
                        break;
                }
            }
        }
        public static void DoMakeover(Farmer? who, string value, bool gendered, bool required, bool replace)
        {
            if (who == null)
                return;
            string[] outfits = value.Split("|");
            List<MakeoverOutfit?> makeoverOutfits = outfits.Select(o => DataLoader.MakeoverOutfits(Game1.content).FirstOrDefault(m => m.Id == o)).ToList();
            if (makeoverOutfits.Count != outfits.Length)
            {
                SMonitor.Log($"[{nameof(Actions)}.{nameof(DoMakeover)}] Makeover outfits don't match; requested: {value}, got {string.Join("|", makeoverOutfits.Select(o => o.Id))}", LogLevel.Warn);
                return;
            }
            MakeoverOutfit? selectedOutfit;
            if (gendered)
            {
                selectedOutfit = makeoverOutfits[(int)Game1.player.Gender];
                if(selectedOutfit.Gender is Gender g && g != Game1.player.Gender)
                {
                    SMonitor.Log($"[{nameof(DoMakeover)}] Makeover outfit {makeoverOutfits.ElementAt((int)Game1.player.Gender).Id} is not valid for the player's gender", LogLevel.Warn);
                    return;
                }
            }
            else
            {
                for (int i = makeoverOutfits.Count -1; i >= 0; i--)
                {
                    var outfit = makeoverOutfits[i];
                    if (outfit.Gender is Gender g && g != Game1.player.Gender)
                    {
                        makeoverOutfits.RemoveAt(i);
                        continue;
                    }
                    foreach (MakeoverItem outfitPart in outfit.OutfitParts)
                    {
                        if (!outfitPart.MatchesGender(Game1.player.Gender))
                        {
                            makeoverOutfits.RemoveAt(i);
                            break;
                        }
                    }
                }
                if (makeoverOutfits.Count == 0)
                {
                    SMonitor.Log($"[{nameof(DoMakeover)}] No valid makeover outfits found for player gender", LogLevel.Warn);
                    return;
                }
                selectedOutfit = Game1.random.ChooseFrom(makeoverOutfits);
            }
            if (selectedOutfit == null)
            {
                SMonitor.Log($"[{nameof(DoMakeover)}] Selected makeover outfit is invalid", LogLevel.Warn);
                return;
            }
            Farmer player = Game1.player;

            if (selectedOutfit.OutfitParts == null)
                return;

            SMonitor.Log($"[{nameof(DoMakeover)}] Applying makeover outfit {selectedOutfit.Id}");

            bool appliedHat = false;
            bool appliedShirt = false;
            bool appliedPants = false;
            foreach (MakeoverItem part in selectedOutfit.OutfitParts)
            {
                if (part.MatchesGender(Game1.player.Gender))
                {
                    Item? item = required ? Game1.player.Items.GetById(part.ItemId).FirstOrDefault() : ItemRegistry.Create(part.ItemId, 1, 0, false);
                    if (item == null)
                        continue;
                    if (required)
                    {
                        Game1.player.removeItemFromInventory(item);
                    }
                    if (item is Hat hat)
                    {
                        if (!appliedHat)
                        {
                            var oldHat = player.Equip<Hat>(hat, player.hat);
                            if (!replace)
                                ReturnClothes(oldHat);
                            appliedHat = true;
                        }
                    }
                    else if (item is Clothing clothing)
                    {
                        Color? color = Utility.StringToColor(part.Color);
                        if (color != null)
                        {
                            clothing.clothesColor.Value = color.Value;
                        }
                        Clothing.ClothesType type = clothing.clothesType.Value;
                        if (type == Clothing.ClothesType.PANTS && !appliedPants)
                        {
                            var oldPants = player.Equip<Clothing>(clothing, player.pantsItem);
                            if (!replace)
                                ReturnClothes(oldPants);
                            appliedPants = true;
                        }
                        else if (type == Clothing.ClothesType.SHIRT && !appliedShirt)
                        {
                            var oldShirt = player.Equip<Clothing>(clothing, player.shirtItem);
                            if (!replace)
                                ReturnClothes(oldShirt);
                            appliedShirt = true;
                        }
                    }
                }
            }
        }

        private static void ReturnClothes(Item oldItem)
        {
            Item clothes = Utility.PerformSpecialItemGrabReplacement(oldItem);
            if (clothes != null && Game1.player.addItemToInventory(clothes) != null)
            {
                Game1.player.team.returnedDonations.Add(clothes);
                Game1.player.team.newLostAndFoundItems.Value = true;
            }
        }

        public static void DoInvalidateAsset(string value)
        {
            foreach (var asset in value.Split('|'))
                context.Helper.GameContent.InvalidateCache(asset);
        }

        public static void DoTeleport(Farmer? who, string value)
        {
            if (who == null)
                return;

            var split = value.Split(',');
            if (split.Length == 1)
                split = value.Split(' ');
            if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                return;
            who.Position = new Vector2(x, y);
        }

        public static void DoTeleportTile(Farmer? who, string value)
        {
            if (who == null)
                return;
            var split = value.Split(',');
            if (split.Length == 1)
                split = value.Split(' ');
            if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                return;
            who.Position = new Vector2(x * 64, y * 64);
        }

        public static void DoGive(Farmer? who, string value)
        {
            if(who == null) 
                return;
            Item? item = null;
            if (value.StartsWith("Money/"))
            {
                if (int.TryParse(value.Split('/')[1], out int number))
                    who.addUnearnedMoney(number);
            }
            else
                item = ParseItemString(value);
            if (item is null)
                return;
            who.holdUpItemThenMessage(item, false);
            if (!who.addItemToInventoryBool(item, false))
                Game1.createItemDebris(item, who.getStandingPosition(), who.FacingDirection);
        }

        public static void DoTake(Farmer? who, string value)
        {
            if (who == null)
                return;

            Item? item = null;
            if (value.StartsWith("Money/"))
            {
                if (int.TryParse(value.Split('/')[1], out int number))
                    who.Money -= number;
            }
            else
                item = ParseItemString(value);
            if (item is null || !who.Items.ContainsId(item.ItemId))
                return;
            who.Items.ReduceId(item.ItemId, item.Stack);
        }

        public static void DoSpawnChest(GameLocation location, string value)
        {
            var split = value.Split('=');
            var split2 = split[0].Split(' ');
            Vector2 tilePos = new(int.Parse(split2[0]), int.Parse(split2[1]));
            if (location.Objects.TryGetValue(tilePos, out var obj) && obj is Chest)
                return;
            int coins = 0;
            string? chestId = null;
            List<Item> items = [];
            if (split[1].Contains('|'))
            {
                split = split[1].Split('|');
                chestId = split[0];
            }
            split2 = split[1].Split(' ');
            foreach (var str in split2)
            {
                if (str.StartsWith("Money/"))
                    _ = int.TryParse(str.Split('/')[1], out coins);
                else
                {
                    var item = ParseItemString(str);
                    if (item is not null)
                        items.Add(item);
                }
            }
            if (coins > 0)
                items.Add(ItemRegistry.Create("(O)GoldCoin", coins));
            Chest chest = new(items, tilePos);

            if (!string.IsNullOrWhiteSpace(chestId))
            {
                var itemData = ItemRegistry.GetData("(BC)" + chestId);
                chest.SetBigCraftableSpriteIndex(itemData.SpriteIndex, lid_frame_count: 5);
            }

            /*Chest chest = new(true, tilePos, chestId)
            {
                Type = "interactive",
                CanBeSetDown = true
            };
            chest.bigCraftable.Value = true;
            chest.GetItemsForPlayer(who.UniqueMultiplayerID).AddRange(items);
            if (coins > 0)
                chest.GetItemsForPlayer(who.UniqueMultiplayerID).Add(ItemRegistry.Create("(O)GoldCoin", coins));*/
            location.overlayObjects[tilePos] = chest;
        }

        public static void DoSpawnObject(GameLocation l, string value)
        {

            var split = value.Split('|');
            foreach(var str in split)
            {
                var split2 = str.Split(',');
                Vector2 tilePos = new(int.Parse(split2[0]), int.Parse(split2[1]));
                var id = split2[2];
                if (string.IsNullOrEmpty(id))
                {
                    l.objects.Remove(tilePos);
                    continue;
                }
                if (!ItemRegistry.IsQualifiedItemId(id))
                    return;
                bool move = split2.Length > 3 && bool.TryParse(split2[3], out var m) ? m : true;
                Item item = ItemRegistry.Create(id);
                if(item is Furniture f)
                {
                    f.InitializeAtTile(tilePos);
                    f.IsOn = true;
                    int rot = split2.Length > 4 && int.TryParse(split2[4], out var r) ? r : 0;
                    for (int rotation = 0; rotation < rot; rotation++)
                    {
                        f.rotate();
                    }
                    if (!move)
                    {
                        f.modData[moveKey] = "false";
                    }
                    Furniture targetFurniture = l.GetFurnitureAt(tilePos);
                    if (targetFurniture != null)
                    {
                        targetFurniture.heldObject.Value = f;
                    }
                    else
                    {
                        l.furniture.Add(f);
                    }
                }
                else if(item is Object obj)
                {
                    if (!move)
                    {
                        obj.modData[moveKey] = "false";
                    }
                    obj.TileLocation = tilePos;
                    l.Objects[tilePos] = obj;
                }
            }
        }

        public static void DoSpawnTerraiFeature(GameLocation l, string value)
        {

            var split = value.Split('|');
            foreach(var str in split)
            {
                var split2 = str.Split(',');
                if (split2.Length < 3)
                    continue;
                Vector2 tilePos = new(int.Parse(split2[0]), int.Parse(split2[1]));
                var type = split2[2];
                if (string.IsNullOrEmpty(type))
                {
                    l.terrainFeatures.Remove(tilePos);
                    continue;
                }
                TerrainFeature? tf = type switch
                {
                    "Grass" => split2.Length < 5 ? new Grass() : new Grass(int.Parse(split2[3]), int.Parse(split2[4])),
                    "HoeDirt" => new HoeDirt((l.IsRainingHere() && l.IsOutdoors) ? 1 : 0, l),
                    "Tree" => split2.Length switch
                    {
                        5 => new Tree(split2[3], int.Parse(split2[4])),
                        4 => new Tree(split2[3]),
                        _ => new Tree()
                    },
                    "FruitTree" => split2.Length switch
                    {
                        5 => new FruitTree(split2[3], int.Parse(split2[4])),
                        _ => new FruitTree()
                    },
                    _ => null
                };
                if (tf is null)
                    continue;
                l.terrainFeatures.TryAdd(tilePos, tf);
            }
        }

        public static void DoUpdateHealth(Farmer? who, string value)
        {
            if (who == null)
                return;

            if (!int.TryParse(value, out int number))
                return;
            if (number > 0)
            {
                who.health = Math.Min(who.health + number, who.maxHealth);
                who.currentLocation.debris.Add(new(number, new(who.getStandingPosition().X + 8, who.getStandingPosition().Y), Color.LimeGreen, 1f, who));
                return;
            }
            who.takeDamage(Math.Abs(number), false, null);
        }

        public static void DoUpdateHealthPerSecond(Farmer? who, string value)
        {
            if (who == null)
                return;

            if (!value.Contains(',') && !value.Contains('|'))
                return;
            var split = value.Split(',');
            if(split.Length == 1)
                split = value.Split('|');
            if (!int.TryParse(split[0], out int loops) || !int.TryParse(split[1], out int number))
                return;
            context.SecondUpdateFiredLoops.Value.Add(new() { Loops = loops, Value = number, type = SecondUpdateData.SecondUpdateType.Health, Who = who });
        }
        

        public static void DoUpdateHealthPerSecondCont(Farmer? who, string value)
        {
            if (!int.TryParse(value, out int number))
                return;
            context.SecondUpdateContinuousLoops.Value.Add(new() { Tile = who.Tile, Location = who.currentLocation, Value = number, type = SecondUpdateData.SecondUpdateType.Health, Who = who });
        }

        public static void DoUpdateStamina(Farmer? who, string value)
        {
            if (who == null)
                return;

            if (!float.TryParse(value, out var number))
                return;
            who.Stamina += number;
        }

        public static void DoUpdateStaminaPerSecond(Farmer? who, string value)
        {
            if (who == null)
                return;

            if (!value.Contains(',') && !value.Contains('|'))
                return;
            var split = value.Split(',');
            if (split.Length == 1)
                split = value.Split('|');
            if (!int.TryParse(split[0], out int loops) || !float.TryParse(split[1], out var number))
                return;
            context.SecondUpdateFiredLoops.Value.Add(new() { Loops = loops, Value = number, type = SecondUpdateData.SecondUpdateType.Stamina, Who = who });
        }
        

        public static void DoUpdateStaminaPerSecondCont(Farmer? who, string value)
        {
            if (who == null)
                return;

            if (!int.TryParse(value, out int number))
                return;
            context.SecondUpdateContinuousLoops.Value.Add(new() { Tile = who.Tile, Location = who.currentLocation, Value = number, type = SecondUpdateData.SecondUpdateType.Stamina, Who = who });
        }

        public static void DoAddBuff(Farmer? who, string value)
        {
            if (who == null)
                return;

            foreach (var item in value.Split('|'))
            {
                if (!item.Contains(','))
                {
                    who.applyBuff(item);
                    continue;
                }
                var opt = item.Split(',');
                who.applyBuff(new Buff(opt[0], displaySource: opt[1]));
            }
        }

        public static void DoEmote(Farmer? who, string value)
        {
            if (who == null)
                return;

            if (!int.TryParse(value, out int id))
                return;
            who.doEmote(id);
        }

        public static void DoExplode(Farmer? who, GameLocation location, string value, Point tilePos)
        {
            var split = value.Split(',');
            if (split.Length == 1)
            {
                split = value.Split(' ');
            }
            string explodeSound = "explosion";
            bool damagesFarmer = true;
            int damageRadius = 1;
            bool destroyObjects = true;
            int radius = 1;
            Vector2 pos = new(tilePos.X, tilePos.Y);

            if (split.Length == 7 && !string.IsNullOrWhiteSpace(split[6]))
                explodeSound = split[6];
            if (split.Length >= 6 && !string.IsNullOrWhiteSpace(split[5]))
                destroyObjects = bool.Parse(split[5]);
            if (split.Length >= 5 && !string.IsNullOrWhiteSpace(split[4]))
                damageRadius = int.Parse(split[4]);
            if (split.Length >= 4 && !string.IsNullOrWhiteSpace(split[3]))
                damagesFarmer = bool.Parse(split[3]);
            if (split.Length >= 3 && !string.IsNullOrWhiteSpace(split[2]))
                radius = int.Parse(split[2]);
            if (split.Length > 1 && !string.IsNullOrWhiteSpace(split[0]) && !string.IsNullOrWhiteSpace(split[1]))
                pos = new(int.Parse(split[0]), int.Parse(split[1]));


            location.playSound(explodeSound);
            location.explode(pos, radius, who, damagesFarmer, damageRadius, destroyObjects);
        }

        public static void DoAnimate(GameLocation location, string value, bool off)
        {
            if (!value.Contains(','))
                return;
            List<Animation> anims = new();
            var opt = value.Split(',');
            if (opt.Length == 2)
            {
                var dict = context.AnimationsDict;
                if (!context.AnimationsDict.TryGetValue(opt[0], out var animSet))
                    return;
                var names = opt[1].Split('|');
                foreach(var name in names)
                {
                    anims.AddRange(animSet.FindAll(x => x.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true || x.Group?.Equals(name, StringComparison.OrdinalIgnoreCase) == true));
                }
            }
            else
                anims = JsonConvert.DeserializeObject<List<Animation>>(value) ?? new();
            foreach (var anim in anims)
            {
                if (anim?.ToSAnim() is not TemporaryAnimatedSprite sprite)
                    continue;

                location.removeTemporarySpritesWithIDLocal(sprite.id);
                if (!off)
                {
                    location.TemporarySprites.Add(sprite);
                }
            }
        }

        public static void DoPushTiles(Farmer? who, Tile tile, Point tilePos) => PushTilesWithOthers(who, tile, tilePos);

        public static void DoPushOtherTiles(Farmer? who, string value, Tile tile, Point tilePos)
        {
            if (who == null)
                return;

            var split = value.Split(',');
            foreach (var item in split)
            {
                Layer l = tile.Layer;
                var pushed = item.Split(' ');
                if (pushed.Length == 3)
                {
                    l = who.currentLocation.Map.GetLayer(pushed[0]);
                    PushTilesWithOthers(who, l.Tiles[int.Parse(pushed[1]), int.Parse(pushed[2])], tilePos);
                    continue;
                }
                PushTilesWithOthers(who, l.Tiles[int.Parse(pushed[0]), int.Parse(pushed[1])], tilePos);
            }
        }

        public static void DoWarp(Farmer? who, string value)
        {
            if (who == null)
                return;

            var split = value.Split(',');
            int x, y;
            if (split.Length == 2 || string.IsNullOrWhiteSpace(split[0]))
            {
                x = int.Parse(split[0]);
                y = int.Parse(split[1]);
                who.warpFarmer(new(who.TilePoint.X, who.TilePoint.Y, who.currentLocation.Name, x, y, false));
                return;
            }
            string location = split[0];
            if (Game1.getLocationFromName(location) is null)
            {
                context.Monitor.Log($"[{nameof(Actions)}.{nameof(DoWarp)}] Location {location} could not be found", LogLevel.Error);
                return;
            }
            x = int.Parse(split[1]);
            y = int.Parse(split[2]);

            who.warpFarmer(new(who.TilePoint.X, who.TilePoint.Y, location, x, y, false));
        }

        public static void DoFriendshipChange(Farmer? who, string value)
        {
            if (who == null)
                return;
            try
            {
                var split = value.Split('|');
                foreach (var item in split)
                {
                    var kv = item.Split(',');
                    string name = kv[0].Trim();
                    if (!int.TryParse(kv[1].Trim(), out int amount))
                        continue;
                    var npc = Game1.getCharacterFromName<NPC>(name, false);
                    if (npc is not null)
                    {
                        who.changeFriendship(amount, npc);
                        continue;
                    }
                    var farm = Game1.RequireLocation<Farm>("Farm");
                    foreach (var animal in farm.Animals.Values)
                        if (animal.type.Value == name)
                            animal.friendshipTowardFarmer.Add(amount);
                }
            }
            catch
            {
                var split = value.Split(',');
                foreach (var item in split)
                {
                    var kv = item.Split('|');
                    string name = kv[0].Trim();
                    if (!int.TryParse(kv[1].Trim(), out int amount))
                        continue;
                    var npc = Game1.getCharacterFromName<NPC>(name, false);
                    if (npc is not null)
                    {
                        who.changeFriendship(amount, npc);
                        continue;
                    }
                    var farm = Game1.RequireLocation<Farm>("Farm");
                    foreach (var animal in farm.Animals.Values)
                        if (animal.type.Value == name)
                            animal.friendshipTowardFarmer.Add(amount);
                }
            }

        }

        public static void DoFertilize(GameLocation location, string value)
        {

            var split = value.Split('|');
            foreach (var item in split)
            {
                var kv = item.Split('=');
                if (kv.Length != 2)
                {
                    context.Monitor.Log($"[{nameof(DoFertilize)}] Missing argument for {item}");
                    continue;
                }
                var xy = kv[0].Split(',');
                if (xy.Length != 2)
                {
                    context.Monitor.Log($"[{nameof(DoFertilize)}] Missing argument for {item}");
                    continue;
                }
                string itemId = kv[1].Trim();
                if (!int.TryParse(xy[0].Trim(), out int x) || !int.TryParse(xy[1].Trim(), out int y) || !location.terrainFeatures.TryGetValue(new Vector2(x, y), out var tf) || tf is not HoeDirt dirt)
                    continue;
                dirt.fertilizer.Value = string.IsNullOrEmpty(itemId) ? null : ItemRegistry.QualifyItemId(itemId) ?? itemId;
            }
        }

        public static void DoKillCrop(GameLocation location, string value)
        {

            var split = value.Split('|');
            foreach (var item in split)
            {
                var xy = item.Split(',');
                if (xy.Length != 2)
                {
                    context.Monitor.Log($"[{nameof(DoKillCrop)}] Error in argument for {item}");
                    continue;
                }
                if (!int.TryParse(xy[0].Trim(), out int x) || !int.TryParse(xy[1].Trim(), out int y) || !location.terrainFeatures.TryGetValue(new Vector2(x, y), out var tf) || tf is not HoeDirt dirt || dirt.crop == null)
                    continue;
                dirt.destroyCrop(true);
            }
        }

        public static void DoGrowCrop(GameLocation location, string value)
        {

            var split = value.Split('|');
            foreach (var item in split)
            {
                var xy = item.Split(',');
                if (xy.Length != 2)
                {
                    context.Monitor.Log($"[{nameof(DoKillCrop)}] Error in argument for {item}");
                    continue;
                }
                if (!int.TryParse(xy[0].Trim(), out int x) || !int.TryParse(xy[1].Trim(), out int y) || !location.terrainFeatures.TryGetValue(new Vector2(x, y), out var tf) || tf is not HoeDirt dirt || dirt.crop == null)
                    continue;
                dirt.crop.growCompletely();
            }
        }

        public static void DoSetCrop(GameLocation location, string value)
        {

            var split = value.Split('|');
            foreach (var item in split)
            {
                string[] split2;
                var temp = item.Split('=');
                if (temp.Length == 2)
                {
                    split2 = temp[0].Split(",").Concat(temp[1].Split(",")).ToArray();
                }
                else
                {
                    split2 = item.Split(",");
                }
                if (split2.Length < 3)
                {
                    context.Monitor.Log($"[{nameof(DoSetCrop)}] Error in argument for {item}");
                    continue;
                }
                if (!int.TryParse(split2[0].Trim(), out int x) || !int.TryParse(split2[1].Trim(), out int y))
                    continue;
                if(!location.terrainFeatures.TryGetValue(new Vector2(x, y), out var tf))
                {
                    location.makeHoeDirt(new Vector2(x, y), true);
                    location.terrainFeatures.TryGetValue(new Vector2(x, y), out tf);
                }
                if(tf is not HoeDirt dirt)
                {
                    continue;
                }

                dirt.crop = new Crop(split2[2].Trim(), x, y, location);
                if (split2.Length > 3 && int.TryParse(split2[3].Trim(), out int phase))
                {
                    dirt.crop.currentPhase.Value = phase < 0 ? dirt.crop.phaseDays.Count - 1 : Math.Min(phase, dirt.crop.phaseDays.Count - 1);
                    dirt.crop.dayOfCurrentPhase.Value = 0;
                    if (dirt.crop.currentPhase.Value == dirt.crop.phaseDays.Count - 1 && dirt.crop.RegrowsAfterHarvest())
                        dirt.crop.fullyGrown.Value = true;
                    dirt.crop.updateDrawMath(dirt.Tile);
                }
            }
        }
    }
}
