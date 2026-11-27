using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using HarmonyLib;

namespace MultipleSpouses
{
    public static class Maps
    {
        private static IMonitor Monitor;
        private static ModConfig config;
        private static IModHelper Helper;
        public static Dictionary<string, Map> tmxSpouseRooms = new Dictionary<string, Map>();
        public static Dictionary<string, int> roomIndexes = new Dictionary<string, int>{
            { "Abigail", 0 },
            { "Penny", 1 },
            { "Leah", 2 },
            { "Haley", 3 },
            { "Maru", 4 },
            { "Sebastian", 5 },
            { "Alex", 6 },
            { "Harvey", 7 },
            { "Elliott", 8 },
            { "Sam", 9 },
            { "Shane", 10 },
            { "Emily", 11 },
            { "Krobus", 12 },
        };

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
            config = ModEntry.config;
            Helper = ModEntry.PHelper;
        }

        public static void BuildSpouseRooms(FarmHouse farmHouse)
        {
            if (config.DisableCustomSpousesRooms)
                return;

            try
            {
                if(farmHouse is Cabin)
                {
                    Monitor.Log("BuildSpouseRooms for Cabin");
                }

                Farmer f = farmHouse.owner;
                if (f == null)
                    return;
                Misc.ResetSpouses(f);
                Monitor.Log("Building all spouse rooms");
                if (Misc.GetSpouses(f, 1).Count == 0 || farmHouse.upgradeLevel > 3)
                {
                    ModEntry.PMonitor.Log("No spouses");
                    farmHouse.showSpouseRoom();
                    return;
                }

                List<string> spousesWithRooms = new List<string>();

                foreach (string spouse in Misc.GetSpouses(f, 1).Keys)
                {
                    Monitor.Log($"checking {spouse} for spouse room");
                    if (roomIndexes.ContainsKey(spouse) || tmxSpouseRooms.ContainsKey(spouse))
                    {
                        Monitor.Log($"Adding {spouse} to list for spouse rooms");
                        spousesWithRooms.Add(spouse);
                    }
                }

                if (spousesWithRooms.Count == 0)
                {
                    ModEntry.PMonitor.Log("No spouses with rooms");
                    return;
                }

                spousesWithRooms = new List<string>(Misc.ReorderSpousesForRooms(spousesWithRooms));

                if (!spousesWithRooms.Any())
                    return;

                if (!ModEntry.config.BuildAllSpousesRooms)
                {
                    if (f.spouse != null && !f.friendshipData[f.spouse].IsEngaged() && (roomIndexes.ContainsKey(f.spouse) || tmxSpouseRooms.ContainsKey(f.spouse)))
                    {
                        Monitor.Log($"Building spouse room for official spouse {f.spouse}");
                        BuildOneSpouseRoom(farmHouse, f.spouse, -1);
                    }
                    else
                    {
                        Monitor.Log($"No spouse room for official spouse {f.spouse}, placing for {spousesWithRooms[0]} instead.");
                        BuildOneSpouseRoom(farmHouse, spousesWithRooms[0], -1);
                        spousesWithRooms = new List<string>(spousesWithRooms.Skip(1));
                    }
                    return;
                }

                Monitor.Log($"Building {spousesWithRooms.Count} additional spouse rooms");

                List<string> sheets = new List<string>();
                for (int i = 0; i < farmHouse.map.TileSheets.Count; i++)
                {
                    sheets.Add(farmHouse.map.TileSheets[i].Id);
                }
                int untitled = sheets.IndexOf("untitled tile sheet");
                int floorsheet = sheets.IndexOf("walls_and_floors");
                int indoor = sheets.IndexOf("indoor");

                Monitor.Log($"Map has sheets: {string.Join(", ", sheets)}");

                int startx = 29;

                int ox = ModEntry.config.ExistingSpouseRoomOffsetX;
                int oy = ModEntry.config.ExistingSpouseRoomOffsetY;
                if (farmHouse.upgradeLevel > 1)
                {
                    ox += 6;
                    oy += 9;
                }

                Monitor.Log($"Preliminary adjustments");

                for (int i = 0; i < 7; i++)
                {
                    farmHouse.setMapTileIndex(ox + startx + i, oy + 11, 0, "Buildings", indoor);
                    farmHouse.removeTile(ox + startx + i, oy + 9, "Front");
                    farmHouse.removeTile(ox + startx + i, oy + 10, "Buildings");
                    farmHouse.setMapTileIndex(ox + startx - 1 + i, oy + 10, 165, "Front", indoor);
                    farmHouse.removeTile(ox + startx + i, oy + 10, "Back");
                }
                for (int i = 0; i < 8; i++)
                {
                    farmHouse.setMapTileIndex(ox + startx - 1 + i, oy + 10, 165, "Front", indoor);
                }
                for (int i = 0; i < 10; i++)
                {
                    farmHouse.removeTile(ox + startx + 6, oy + 0 + i, "Buildings");
                    farmHouse.removeTile(ox + startx + 6, oy + 0 + i, "Front");
                }
                for (int i = 0; i < 7; i++)
                {
                    // horiz hall
                    farmHouse.setMapTileIndex(ox + startx + i, oy + 10, (i % 2 == 0 ? 352: 336), "Back", floorsheet);
                }


                for (int i = 0; i < 7; i++)
                {
                    //farmHouse.removeTile(ox + startx - 1, oy + 4 + i, "Back");
                    //farmHouse.setMapTileIndex(ox + 28, oy + 4 + i, (i % 2 == 0 ? 352 : ModEntry.config.HallTileEven), "Back", 0);
                }


                farmHouse.removeTile(ox + startx - 1, oy + 9, "Front");
                farmHouse.removeTile(ox + startx - 1, oy + 10, "Buildings");
                
                if(farmHouse.upgradeLevel > 1) 
                    farmHouse.setMapTileIndex(ox + startx - 1, oy + 10, 163, "Front", indoor);
                farmHouse.removeTile(ox + startx + 6, oy + 0, "Front");
                farmHouse.removeTile(ox + startx + 6, oy + 0, "Buildings");



                int count = -1;

                ExtendMap(farmHouse, ox + startx + 8 + (7* spousesWithRooms.Count));

                // remove and rebuild spouse rooms
                for (int j = 0; j < spousesWithRooms.Count; j++)
                {
                    farmHouse.removeTile(ox + startx + 6 + (7 * count), oy + 0, "Buildings");
                    for (int i = 0; i < 10; i++)
                    {
                        farmHouse.removeTile(ox + startx + 6 + (7 * count), oy + 1 + i, "Buildings");
                    }
                    BuildOneSpouseRoom(farmHouse, spousesWithRooms[j], count++);
                }

                Monitor.Log($"Building far wall");

                // far wall
                farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 0, 11, "Buildings", indoor);
                for (int i = 0; i < 10; i++)
                {
                    farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 1 + i, 68, "Buildings", indoor);
                }
                farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 10, 130, "Front", indoor);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(BuildSpouseRooms)}:\n{ex}", LogLevel.Error);
            }
            farmHouse.getWalls();
            farmHouse.getFloors();
        }

        public static void BuildOneSpouseRoom(FarmHouse farmHouse, string name, int count)
        {

            NPC spouse = Game1.getCharacterFromName(name);
            Layer back = null;
            Layer buildings = null;
            Layer front = null;
            Layer alwaysFront = null;
            if (spouse != null || name == "")
            {
                if (farmHouse.owner.friendshipData[spouse.Name] != null && farmHouse.owner.friendshipData[spouse.Name].IsEngaged())
                    name = "";
                Map refurbishedMap;
                if (name == "")
                {
                    refurbishedMap = Helper.Content.Load<Map>("Maps\\" + farmHouse.Name + ((farmHouse.upgradeLevel == 0) ? "" : ((farmHouse.upgradeLevel == 3) ? "2" : string.Concat(farmHouse.upgradeLevel))) + "_marriage", ContentSource.GameContent);
                }
                else
                {
                    refurbishedMap = Helper.Content.Load<Map>("Maps\\spouseRooms", ContentSource.GameContent);
                }
                int indexInSpouseMapSheet = -1;

                if (roomIndexes.ContainsKey(name))
                {
                    back = refurbishedMap.GetLayer("Back");
                    buildings = refurbishedMap.GetLayer("Buildings");
                    front = refurbishedMap.GetLayer("Front");

                    indexInSpouseMapSheet = roomIndexes[name];
                    if(name == "Emily")
                    {
                        farmHouse.temporarySprites.RemoveAll((s) => s is EmilysParrot);

                        int offset = (1 + count) * 7 * 64;
                        Vector2 parrotSpot = new Vector2(2064f + offset, 160f);
                        int upgradeLevel = farmHouse.upgradeLevel;
                        if (upgradeLevel > 1)
                        {
                            parrotSpot = new Vector2(2448f + offset, 736f);
                        }
                        ModEntry.PMonitor.Log($"Building Emily's parrot at {parrotSpot}, spouse room count {count}, upgrade level {upgradeLevel}");
                        farmHouse.temporarySprites.Add(new EmilysParrot(parrotSpot));
                    }
                }
                else if (tmxSpouseRooms.ContainsKey(name))
                {

                    refurbishedMap = tmxSpouseRooms[name];
                    if (refurbishedMap == null)
                    {
                        ModEntry.PMonitor.Log($"Couldn't load TMX spouse room for spouse {name}", LogLevel.Error);
                        return;
                    }
                    try 
                    {
                        foreach(Layer l in refurbishedMap.Layers)
                        {
                            ModEntry.PMonitor.Log($"layer ID: {l.Id}");

                            if(l.Id != "Back" && l.Id.StartsWith("Back")){
                                back = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for back in TMX spouse room layers for spouse {name}.");
                            }
                            else if(back == null && l.Id == "Back"){
                                back = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for back in TMX spouse room layers for spouse {name}.");
                            }
                            else if(l.Id != "Buildings" && l.Id.StartsWith("Buildings"))
                            {
                                buildings = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for buildings in TMX spouse room layers for spouse {name}.");
                            }
                            else if(buildings == null && l.Id == "Buildings")
                            {
                                buildings = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for buildings in TMX spouse room layers for spouse {name}.");
                            }
                            else if (l.Id != "Front" && l.Id.StartsWith("Front"))
                            {
                                front = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for front in TMX spouse room layers for spouse {name}.");
                            }
                            else if(front == null && l.Id == "Front")
                            {
                                front = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for front in TMX spouse room layers for spouse {name}.");
                            }
                            else if (l.Id != "AlwaysFront" && l.Id.StartsWith("AlwaysFront"))
                            {
                                alwaysFront = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for alwaysFront in TMX spouse room layers for spouse {name}.");
                            }
                            else if(alwaysFront == null && l.Id == "AlwaysFront")
                            {
                                alwaysFront = l;
                                ModEntry.PMonitor.Log($"Using {l.Id} for alwaysFront in TMX spouse room layers for spouse {name}.");
                            }
                        }
                        if(back == null)
                            back = refurbishedMap.Layers[0];
                        if (buildings == null)
                            buildings = refurbishedMap.Layers[1];
                        if (front == null)
                            front = refurbishedMap.Layers[2];
                        if(alwaysFront == null && refurbishedMap.Layers.Count > 3)
                            alwaysFront = refurbishedMap.Layers[3]; 
                    }
                    catch(Exception ex)
                    {
                        ModEntry.PMonitor.Log($"Couldn't load TMX spouse room layers for spouse {name}. Exception: {ex}", LogLevel.Error);
                    }
                    ModEntry.PMonitor.Log($"Loaded TMX spouse room layers for spouse {name}.");

                    indexInSpouseMapSheet = 0;
                }
                else if(name != "")
                {
                    return;
                }


                Monitor.Log($"Building {name}'s room", LogLevel.Debug);

                int startx = 29;
                int ox = ModEntry.config.ExistingSpouseRoomOffsetX;
                int oy = ModEntry.config.ExistingSpouseRoomOffsetY;
                if (farmHouse.upgradeLevel > 1)
                {
                    ox += 6;
                    oy += 9;
                }

                Microsoft.Xna.Framework.Rectangle areaToRefurbish = new Microsoft.Xna.Framework.Rectangle(startx + 7 + ox + (7 * count), 1 + oy, 6, 9);

                Point mapReader;
                if (name == "")
                {
                    mapReader = new Point(areaToRefurbish.X, areaToRefurbish.Y);
                }
                else
                {
                    mapReader = new Point(indexInSpouseMapSheet % 5 * 6, indexInSpouseMapSheet / 5 * 9);
                }
                farmHouse.map.Properties.Remove("DayTiles");
                farmHouse.map.Properties.Remove("NightTiles");


                List<string> sheetNames = new List<string>();
                for (int i = 0; i < farmHouse.map.TileSheets.Count; i++)
                {
                    sheetNames.Add(farmHouse.map.TileSheets[i].Id);
                    //Monitor.Log($"tilesheet id: {farmHouse.map.TileSheets[i].Id}");
                }
                int untitled = sheetNames.IndexOf("untitled tile sheet");
                int floorsheet = sheetNames.IndexOf("walls_and_floors");
                int indoor = sheetNames.IndexOf("indoor");

                if (ModEntry.config.BuildAllSpousesRooms)
                {



                    for (int i = 0; i < 7; i++)
                    {
                        // bottom wall
                        farmHouse.setMapTileIndex(ox + startx + 7 + i + (count * 7), oy + 10, 165, "Front", indoor);
                        farmHouse.setMapTileIndex(ox + startx + 7 + i + (count * 7), oy + 11, 0, "Buildings", indoor);


                        farmHouse.removeTile(ox + startx + 6 + (7 * count), oy + 4 + i, "Back");

                        if (count % 2 == 0)
                        {
                            // vert hall
                            farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 4 + i, (i % 2 == 0 ? 352 : 336), "Back", floorsheet);
                            // horiz hall
                            farmHouse.setMapTileIndex(ox + startx + 7 + i + (count * 7), oy + 10, (i % 2 == 0 ? 336 : 352), "Back", floorsheet);
                        }
                        else
                        {
                            // vert hall
                            
                            farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 4 + i, (i % 2 == 0 ? 336 : 352), "Back", floorsheet);
                            // horiz hall
                            farmHouse.setMapTileIndex(ox + startx + 7 + i + (count * 7), oy + 10, (i % 2 == 0 ? 352 : 336), "Back", floorsheet);
                        }
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        // top wall
                        farmHouse.setMapTileIndex(ox + startx + 7 + i + (count * 7), oy + 0, 2, "Buildings", indoor);
                    }

                    // vert wall
                    farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 0, 87, "Buildings", untitled);
                    farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 1, 99, "Buildings", untitled);
                    farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 2, 111, "Buildings", untitled);
                    farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 3, 123, "Buildings", untitled);
                    farmHouse.setMapTileIndex(ox + startx + 6 + (7 * count), oy + 4, 135, "Buildings", untitled);

                }
                for (int x = 0; x < areaToRefurbish.Width; x++)
                {
                    for (int y = 0; y < areaToRefurbish.Height; y++)
                    {
                        //PMonitor.Log($"x {x}, y {y}", LogLevel.Debug);
                        if (back?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
                        {
                            farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Back"), farmHouse.map.GetTileSheet(back.Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, back.Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
                            foreach (KeyValuePair<string, PropertyValue> prop in back.Tiles[mapReader.X + x, mapReader.Y + y].Properties)
                            {
                                farmHouse.setTileProperty(areaToRefurbish.X + x, areaToRefurbish.Y + y, "Back", prop.Key, prop.Value);
                            }
                        }
                        if (buildings?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
                        {
                            farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Buildings"), farmHouse.map.GetTileSheet(buildings.Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, buildings.Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);

                            foreach(KeyValuePair<string, PropertyValue> prop in buildings.Tiles[mapReader.X + x, mapReader.Y + y].Properties)
                            {
                                farmHouse.setTileProperty(areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings", prop.Key, prop.Value);
                            }

                            typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { buildings.Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings" });
                        }
                        else
                        {
                            farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
                        }
                        if (y < areaToRefurbish.Height - 1 && front?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
                        {
                            farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Front"), farmHouse.map.GetTileSheet(front.Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, front.Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
                            foreach (KeyValuePair<string, PropertyValue> prop in front.Tiles[mapReader.X + x, mapReader.Y + y].Properties)
                            {
                                farmHouse.setTileProperty(areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front", prop.Key, prop.Value);
                            }
                            typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { front.Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front" });
                        }
                        else if (y < areaToRefurbish.Height - 1)
                        {
                            farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
                        }
                        if (y < areaToRefurbish.Height - 2 && alwaysFront?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
                        {
                            if(farmHouse.map.GetLayer("AlwaysFront") == null)
                            {
                                farmHouse.map.AddLayer(new Layer("AlwaysFront", farmHouse.map, farmHouse.map.GetLayer("Front").LayerSize, farmHouse.map.GetLayer("Front").TileSize));

                            }
                            farmHouse.map.GetLayer("AlwaysFront").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("AlwaysFront"), farmHouse.map.GetTileSheet(alwaysFront.Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, alwaysFront.Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
                            foreach (KeyValuePair<string, PropertyValue> prop in alwaysFront.Tiles[mapReader.X + x, mapReader.Y + y].Properties)
                            {
                                farmHouse.setTileProperty(areaToRefurbish.X + x, areaToRefurbish.Y + y, "AlwaysFront", prop.Key, prop.Value);
                            }
                            typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { alwaysFront.Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "AlwaysFront" });
                        }
                        else if (y < areaToRefurbish.Height - 2 && farmHouse.map.GetLayer("AlwaysFront") != null)
                        {
                            farmHouse.map.GetLayer("AlwaysFront").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
                        }
                        if (x == 4 && y == 4)
                        {
                            try
                            {
                                farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y].Properties["NoFurniture"] = new PropertyValue("T");
                            }
                            catch (Exception ex)
                            {
                                Monitor.Log(ex.ToString());
                            }
                        }
                    }
                }

                if(name == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrogReal"))
                {
                    Monitor.Log("building Sebastian's terrarium");
                    Vector2 spot = new Vector2(startx + 8 + (7 * count) + ox, 7 + oy);
                    farmHouse.removeTile((int)spot.X, (int)spot.Y - 1, "Front");
                    farmHouse.removeTile((int)spot.X + 1, (int)spot.Y - 1, "Front");
                    farmHouse.removeTile((int)spot.X + 2, (int)spot.Y - 1, "Front");
                    farmHouse.temporarySprites.Add(new TemporaryAnimatedSprite
                    {
                        texture = Game1.mouseCursors,
                        sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
                        animationLength = 1,
                        sourceRectStartingPos = new Vector2(641f, 1534f),
                        interval = 5000f,
                        totalNumberOfLoops = 9999,
                        position = spot * 64f + new Vector2(0f, -5f) * 4f,
                        scale = 4f,
                        layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
                    });
                    if (Game1.random.NextDouble() < 0.85)
                    {
                        Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
                        farmHouse.TemporarySprites.Add(new SebsFrogs
                        {
                            texture = crittersText2,
                            sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 224, 16, 16),
                            animationLength = 1,
                            sourceRectStartingPos = new Vector2(64f, 224f),
                            interval = 100f,
                            totalNumberOfLoops = 9999,
                            position = spot * 64f + new Vector2((float)((Game1.random.NextDouble() < 0.5) ? 22 : 25), (float)((Game1.random.NextDouble() < 0.5) ? 2 : 1)) * 4f,
                            scale = 4f,
                            flipped = (Game1.random.NextDouble() < 0.5),
                            layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
                            Parent = farmHouse
                        });
                    }
                    if (!Game1.player.activeDialogueEvents.ContainsKey("sebastianFrog2") && Game1.random.NextDouble() < 0.5)
                    {
                        Texture2D crittersText3 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
                        farmHouse.TemporarySprites.Add(new SebsFrogs
                        {
                            texture = crittersText3,
                            sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 240, 16, 16),
                            animationLength = 1,
                            sourceRectStartingPos = new Vector2(64f, 240f),
                            interval = 150f,
                            totalNumberOfLoops = 9999,
                            position = spot * 64f + new Vector2(8f, 3f) * 4f,
                            scale = 4f,
                            layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
                            flipped = (Game1.random.NextDouble() < 0.5),
                            pingPong = false,
                            Parent = farmHouse
                        });
                    }
                }

                /*
                List<string> sheets = new List<string>();
                for (int i = 0; i < farmHouse.map.TileSheets.Count; i++)
                {
                    sheets.Add(farmHouse.map.TileSheets[i].Id);
                }
                for (int i = 0; i < 7; i++)
                {

                    // vert floor ext
                    farmHouse.removeTile(ox + 42 + (7 * count), oy + 4 + i, "Back");
                    farmHouse.setMapTileIndex(ox + 42 + (7 * count), oy + 4 + i, farmHouse.getTileIndexAt(ox + 41 + (7 * count), oy + 4 + i, "Back"), "Back", sheets.IndexOf(farmHouse.getTileSheetIDAt(ox + 41 + (7 * count), oy + 4 + i, "Back")));
                }
                */
            }
        }

        private static void ExtendMap(FarmHouse farmHouse, int v)
        {
            ModEntry.PMonitor.Log($"Extending map width to {v}");
            List<Layer> layers = AccessTools.Field(typeof(Map), "m_layers").GetValue(farmHouse.map) as List<Layer>;
            for (int i = 0; i < layers.Count; i++)
            {
                Tile[,] tiles = AccessTools.Field(typeof(Layer), "m_tiles").GetValue(layers[i]) as Tile[,];
                Size size = (Size)AccessTools.Field(typeof(Layer), "m_layerSize").GetValue(layers[i]);
                if (size.Width >= v)
                    continue;
                size = new Size(v, size.Height);
                AccessTools.Field(typeof(Layer), "m_layerSize").SetValue(layers[i], size);
                AccessTools.Field(typeof(Map), "m_layers").SetValue(farmHouse.map, layers);

                Tile[,] newTiles = new Tile[v, tiles.GetLength(1)];

                for (int k = 0; k < tiles.GetLength(0); k++)
                {
                    for (int l = 0; l < tiles.GetLength(1); l++)
                    {
                        newTiles[k, l] = tiles[k, l];
                    }
                }
                AccessTools.Field(typeof(Layer), "m_tiles").SetValue(layers[i], newTiles);
                AccessTools.Field(typeof(Layer), "m_tileArray").SetValue(layers[i], new TileArray(layers[i], newTiles));

            }
            AccessTools.Field(typeof(Map), "m_layers").SetValue(farmHouse.map, layers);
        }


        private static void setupTile(int posX, int posY, int indexX, int indexY, FarmHouse farmHouse, List<int> indexes, List<int> sheets, int indexWidth, int layerNo)
        {
            string[] layerNames = {
                "Front",
                "Buildings",
                "Back"
            };
            int idx = indexY * indexWidth + indexX;
            string layer = layerNames[layerNo];
            try
            {
                if(farmHouse.map.GetLayer(layer)?.Tiles[posX, posY] != null)
                    farmHouse.removeTile(posX, posY, layer);
                farmHouse.setMapTileIndex(posX, posY, indexes[idx], layer, sheets[idx]);
            }
            catch(Exception ex)
            {
                Monitor.Log("x1: "+posX);
                Monitor.Log("y1: "+posY);
                Monitor.Log("x: "+indexX);
                Monitor.Log("y: "+indexY);
                Monitor.Log("sheet: "+layerNo);
                Monitor.Log("index: "+ idx);
                Monitor.Log("Exception: "+ex, LogLevel.Error);
            }
        }
        internal static void ExpandKidsRoom(FarmHouse farmHouse)
        {
            ModEntry.PMonitor.Log("Expanding kids room");

            //int extraWidth = Math.Max(ModEntry.config.ExtraCribs,0) * 3 + Math.Max(ModEntry.config.ExtraKidsRoomWidth, 0) + Math.Max(ModEntry.config.ExtraKidsBeds, 0) * 4;
            int extraWidth = Math.Max(ModEntry.config.ExtraKidsRoomWidth, 0);
            int roomWidth = 14;
            int height = 9;
            int startx = 15;
            int starty = 0;
            //int ox = ModEntry.config.ExistingKidsRoomOffsetX;
            int ox = 0;
            //int oy = ModEntry.config.ExistingKidsRoomOffsetY;
            int oy = 0;

            Map map = Helper.Content.Load<Map>("Maps\\" + farmHouse.Name + "2"+ (Misc.GetSpouses(farmHouse.owner,1).Count > 0? "_marriage":""), ContentSource.GameContent);

            List<string> sheets = new List<string>();
            for (int i = 0; i < map.TileSheets.Count; i++)
            {
                sheets.Add(map.TileSheets[i].Id);
            }

            List<int> backIndexes = new List<int>();
            List<int> frontIndexes = new List<int>();
            List<int> buildIndexes = new List<int>();
            List<int> backSheets = new List<int>();
            List<int> frontSheets = new List<int>();
            List<int> buildSheets = new List<int>();


            for (int i = 0; i < roomWidth * height; i++)
            {
                backIndexes.Add(getTileIndexAt(map, ox + startx + (i % roomWidth), oy + starty + (i / roomWidth), "Back"));
                backSheets.Add(sheets.IndexOf(getTileSheetIDAt(map, ox + startx + (i % roomWidth), oy + starty + (i / roomWidth), "Back")));
                frontIndexes.Add(getTileIndexAt(map, ox + startx + (i % roomWidth), oy + starty + (i / roomWidth), "Front"));
                frontSheets.Add(sheets.IndexOf(getTileSheetIDAt(map, ox + startx + (i % roomWidth), oy + starty + (i / roomWidth), "Front")));
                buildIndexes.Add(getTileIndexAt(map, ox + startx + (i % roomWidth), oy + starty + (i / roomWidth), "Buildings"));
                buildSheets.Add(sheets.IndexOf(getTileSheetIDAt(map, ox + startx + (i % roomWidth), oy + starty + (i / roomWidth), "Buildings")));
            }

            if(extraWidth > 0)
            {
                Monitor.Log("total width: " + (29 + ox + extraWidth));
                ExtendMap(farmHouse, 29 + ox + extraWidth);
            }

            int k = 0;

            if (ModEntry.config.ExtraKidsRoomWidth > 0)
            {
                for (int j = 0; j < ModEntry.config.ExtraKidsRoomWidth - 1; j++)
                {
                    k %= 3;
                    for (int i = 0; i < height; i++)
                    {
                        int x = roomWidth + j + ox + startx - 2;
                        int y = oy + starty + i; 
                        int xt = 4 + k;
                        int yt = i;

                        setupTile(x, y, xt, yt, farmHouse, frontIndexes, frontSheets, roomWidth, 0);
                        setupTile(x, y, xt, yt, farmHouse, buildIndexes, buildSheets, roomWidth, 1);
                        setupTile(x, y, xt, yt, farmHouse, backIndexes, backSheets, roomWidth, 2);
                    }
                    k++;
                }
                for (int i = 0; i < height; i++)
                {
                    int x = startx + roomWidth + ox + extraWidth - 3;
                    int y = oy + starty + i;
                    int xt = roomWidth - 2;
                    int yt = i;

                    setupTile(x, y, xt, yt, farmHouse, frontIndexes, frontSheets, roomWidth, 0);
                    setupTile(x, y, xt, yt, farmHouse, buildIndexes, buildSheets, roomWidth, 1);
                    setupTile(x, y, xt, yt, farmHouse, backIndexes, backSheets, roomWidth, 2);
                }
            }



            // far wall
            for (int i = 0; i < height; i++)
            {
                int x = startx + roomWidth + ox + extraWidth - 2;
                int y = oy + starty + i;
                int xt = roomWidth - 1;
                int yt = i;

                setupTile(x, y, xt, yt, farmHouse, frontIndexes, frontSheets, roomWidth, 0);
                setupTile(x, y, xt, yt, farmHouse, buildIndexes, buildSheets, roomWidth, 1);
                setupTile(x, y, xt, yt, farmHouse, backIndexes, backSheets, roomWidth, 2);
            }

            // bottom barrier
            for (int i = 0; i < extraWidth; i++)
            {
                int x = startx + roomWidth + ox + i;
                Tile tile = farmHouse.map.GetLayer("Buildings").PickTile(new Location(x * Game1.tileSize, (9 + oy) * Game1.tileSize), Game1.viewport.Size);
                if (tile == null || tile.TileIndex == -1)
                {
                    Monitor.Log($"Adding building tile at {startx + roomWidth + ox + i},{ 9 + oy}");
                    farmHouse.setMapTileIndex(x, 9 + oy, 0, "Buildings");
                }
            }

            Microsoft.Xna.Framework.Rectangle? crib_location = farmHouse.GetCribBounds();
            if(crib_location != null)
            {
                Monitor.Log($"Adding {Misc.GetExtraCribs()} cribs");
                for (int i = 1; i <= Misc.GetExtraCribs(); i++)
                {
                    Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(crib_location.Value.X + i * 3, crib_location.Value.Y, crib_location.Value.Width, crib_location.Value.Height); 
                    Monitor.Log($"Adding crib at {rect}");
                    Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\FarmHouse_Crib_" + farmHouse.cribStyle.Value);
                    HashSet<string> amo = Helper.Reflection.GetField<HashSet<string>>(farmHouse, "_appliedMapOverrides").GetValue();
                    amo.Remove($"crib{i + 1}");
                    Helper.Reflection.GetField<HashSet<string>>(farmHouse, "_appliedMapOverrides").SetValue(amo);
                    farmHouse.ApplyMapOverride(override_map, $"crib{i+1}", null, rect);
                }
            }
        }

        public static string getTileSheetIDAt(Map map, int x, int y, string layer)
        {
            if (map.GetLayer(layer) == null)
            {
                return "";
            }
            Tile tmp = map.GetLayer(layer).Tiles[x, y];
            if (tmp != null)
            {
                return tmp.TileSheet.Id;
            }
            return "";
        }

        public static int getTileIndexAt(Map map, int x, int y, string layer)
        {
            if (map.GetLayer(layer) == null)
            {
                return -1;
            }
            Tile tmp = map.GetLayer(layer).Tiles[x, y];
            if (tmp != null)
            {
                return tmp.TileIndex;
            }
            return -1;
        }

        public static void setMapTileIndex(ref Map map, int tileX, int tileY, int index, string layer, int whichTileSheet = 0)
        {
            try
            {
                if (map.GetLayer(layer).Tiles[tileX, tileY] != null)
                {
                    if (index == -1)
                    {
                        map.GetLayer(layer).Tiles[tileX, tileY] = null;
                    }
                    else
                    {
                        map.GetLayer(layer).Tiles[tileX, tileY].TileIndex = index;
                    }
                }
                else
                {
                    map.GetLayer(layer).Tiles[tileX, tileY] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, index);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}