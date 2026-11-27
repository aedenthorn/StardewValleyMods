using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Display;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace SGJigsaw
{
    public class JigsawGameMenu : IClickableMenu
    {
        public static JigsawGameMenu instance;

        public SpriteFont font;
        public DropDown musicDropdown;
        public DropDown mapDropdown;
        public DropDown pieceSizeDropdown;
        public DropDown seasonDropdown;

        public ClickableTextureComponent stopButton;
        public ClickableTextureComponent playButton;
        public ClickableTextureComponent nextButton;
        public ClickableTextureComponent backButton;

        public Point puzzleSize = new Point();
        public Point puzzleSizeMismatch = new Point();
        public List<int> shorter = new List<int>();
        public List<int> skinnier = new List<int>();
        public PuzzlePiece[,] pieces = null;
        

        public List<PuzzlePiece> sortedList = new List<PuzzlePiece>();
        public TileList[,] tileDict;

        public Dictionary<string, LocationData> mapsDataDict = new Dictionary<string, LocationData>();
        public List<string> maps = new List<string>();
        public List<string> pieceSizes = new List<string>();
        public List<string> music;
        
        public Object box;
        public LocalizedContentManager loader;
        public Map map;
        public LocationData mapData;
        public int pieceSize = 8;
        public int border = 4000;
        public string mapPath = "Maps\\Town";
        public float zoom = 1;
        
        public Vector2 offset;

        public Vector2 dragged;
        public Vector2 draggedStart;
        public PuzzlePiece draggingPiece;
        public HashSet<PuzzlePiece> draggingPieces = new HashSet<PuzzlePiece>();

        public bool held;
        private bool draggingScreen;
        public bool solved;
        public bool solving;

        public WaterTiles waterTiles;
        public int waterAnimationIndex;
        public int waterAnimationTimer;
        public float waterPosition;
        public bool waterTileFlip;
        public bool isFarm;
        public int season = 0;
        public Color waterColor;

        public JigsawGameMenu(Object box) : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height)
        {
            instance = this;
            if (box is not null)
            {
                season = (int)Game1.season;
            }

            loader = new LocalizedContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
            var mapsData = DataLoader.Locations(Game1.content).Values.Where(l => l.CreateOnLoad != null);
            maps = mapsData.Select(l => l.CreateOnLoad.MapPath).ToList();
            maps.Sort();
            mapsDataDict.Clear();
            for (int i = 0; i < maps.Count; i++)
            {
                mapsDataDict[maps[i]] = mapsData.ElementAt(i);
            }

            if (ModEntry.Config.ShuffleMusic)
            {
                music = new List<string>();
                music.AddRange(ModEntry.Config.Music);
                music.Shuffle();
            }
            else
            {
                music = ModEntry.Config.Music;
            }
            if (box != null)
            {
                this.box = box;
                behaviorBeforeCleanup = CleanupBeforeExit;
                PuzzleBoxInfo info = new PuzzleBoxInfo();
                if (box.modData.TryGetValue(ModEntry.infoKey, out var dataStr))
                {
                    info = JsonConvert.DeserializeObject<PuzzleBoxInfo>(dataStr);
                    mapPath = info.mapPath;
                    LoadMap();
                    mapData = mapsDataDict[mapPath];
                    sortedList = info.pieces;
                    puzzleSize = info.puzzleSize;
                    pieceSize = info.pieceSize;
                    pieces = new PuzzlePiece[puzzleSize.X, puzzleSize.Y];
                    foreach (var piece in sortedList)
                    {
                        pieces[piece.index.X, piece.index.Y] = piece;
                    }
                    foreach (var piece in sortedList)
                    {
                        piece.Initialize();
                    }
                    zoom = info.zoom;
                    offset = info.offset;
                    season = info.season;
                }
                else
                {
                    ReloadMap();
                }
            }
            else
            {
                ReloadMap();
            }
            RebuildElements();
            ChangeMusic(ModEntry.Config.CurrentMusic);
            if(!ModEntry.Config.PlayingMusic)
                ChangeMusic(null);
        }

        private void LoadMap()
        {
            solved = false;
            solving = false;

            map = ModEntry.SHelper.GameContent.Load<Map>(mapPath);
            tileDict = new TileList[map.Layers[0].LayerWidth, map.Layers[0].LayerHeight];
            for (int x = 0; x < map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < map.Layers[0].LayerHeight; y++)
                {
                    List<Tile> list = new List<Tile>();
                    foreach (var layer in map.Layers)
                    {
                        if (!layer.Id.StartsWith("Paths"))
                            list.Add(layer.Tiles[x, y]);
                    }
                    tileDict[x, y] = new TileList(x, y, list);
                }
            }
            UpdateSeasonalTileSheets();
        }

        private void CleanupBeforeExit(IClickableMenu menu)
        {
            var info = new PuzzleBoxInfo()
            {
                mapPath = mapPath,
                zoom = zoom,
                offset = offset,
                pieces = sortedList,
                pieceSize = pieceSize,
                puzzleSize = puzzleSize,
            };
            (menu as JigsawGameMenu).box.modData[ModEntry.infoKey] = JsonConvert.SerializeObject(info);
        }

        public void Solve()
        {
            solving = true;
        }

        private void RebuildElements()
        {
            font = Game1.smallFont;
            musicDropdown = new DropDown(music, (int)font.MeasureString(ModEntry.SHelper.Translation.Get("music")).X + 64, 12, 100, 44, 2);
            musicDropdown.SetCurrentItem(ModEntry.Config.CurrentMusic);

            stopButton = new ClickableTextureComponent("stop", new Rectangle(new Point(musicDropdown.bounds.Right, 22), new Point(28, 28)), "", "Stop", Game1.staminaRect, new Rectangle(0,0,1,1), 24);

            playButton = new ClickableTextureComponent("play", new Rectangle(new Point(musicDropdown.bounds.Right, 18), new Point(32, 32)), "", "Play", Game1.mouseCursors, new Rectangle(448, 96, 32, 32), 1);

            nextButton = new ClickableTextureComponent("next", new Rectangle(new Point(playButton.bounds.Right, 6), new Point(44, 44)), "", "Next", Game1.mouseCursors, new Rectangle(364, 493, 14, 14), 4);

            mapDropdown = new DropDown(maps, Game1.uiViewport.Width / 2, 12, 100, 44, 2);
            mapDropdown.bounds.Offset(-mapDropdown.bounds.Width / 2, 0);
            mapDropdown.SetCurrentItem(mapPath);

            var seasonList = Enumerable.Range(0, 4).Select(s => Utility.getSeasonNameFromNumber(s)).ToList();
            seasonDropdown = new DropDown(seasonList, Game1.uiViewport.Width * 3 / 4, 12, 100, 44, 2);
            seasonDropdown.bounds.Offset(-seasonDropdown.bounds.Width / 2, 0);
            seasonDropdown.SetCurrentItem(seasonList[season]);
            

            pieceSizes = Enumerable.Range(2, 29).Select(i => i.ToString()).ToList();
            pieceSizeDropdown = new DropDown(pieceSizes, Game1.uiViewport.Width - 64, 12, 100, 44, 2);
            pieceSizeDropdown.bounds.Offset(-pieceSizeDropdown.bounds.Width, 0);
            pieceSizeDropdown.SetCurrentItem(pieceSize.ToString());

            backButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11739"), new Rectangle(width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, this.height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), null, "", Game1.content.Load<Texture2D>("Minigames\\TitleButtons"), new Rectangle(296, 252, 66, 27), (float)TitleMenu.pixelZoom, false)
            {
                myID = 81114
            };
        }

        private void ReloadMap()
        {
            skinnier.Clear();
            shorter.Clear();

            LoadMap();
            mapData = mapsDataDict[mapPath];
            isFarm = map.Properties.ContainsKey("IsFarm");
            //if (mapData.CreateOnLoad.Type != "MineShaft")
            //{
            //    switch (season)
            //    {
            //        case Season.Spring:
            //            waterColor = new Color(120, 200, 255) * 0.5f;
            //            break;
            //        case Season.Summer:
            //            waterColor = new Color(60, 240, 255) * 0.5f;
            //            break;
            //        case Season.Fall:
            //            waterColor = new Color(255, 130, 200) * 0.5f;
            //            break;
            //        case Season.Winter:
            //            waterColor = new Color(130, 80, 255) * 0.5f;
            //            break;
            //    }
            //}
            float wdiff = (float)Game1.viewport.Width / map.DisplayWidth;
            float hdiff = (float)Game1.viewport.Height / map.DisplayHeight;

            zoom = Math.Clamp((wdiff < hdiff ? wdiff : hdiff) / 2, 0.15f, 10f);
            offset = ClampToRect(new Vector2(-(Game1.uiViewport.Width / 2 - map.DisplayWidth / 2 * zoom), -(Game1.uiViewport.Height / 2 - map.DisplayHeight / 2 * zoom)) / zoom);

            int x = (int)Math.Ceiling(map.DisplayWidth / 64 / (float)pieceSize);
            int y = (int)Math.Ceiling(map.DisplayHeight / 64 / (float)pieceSize);
            int xx = pieceSize - (map.DisplayWidth / 64 % pieceSize);
            int yy = pieceSize - (map.DisplayHeight / 64 % pieceSize);
            if (xx == pieceSize)
                xx = 0;
            if (yy == pieceSize)
                yy = 0;
            while (xx > x || yy > y)
            {
                pieceSize--;
                x = (int)Math.Ceiling(map.DisplayWidth / 64 / (float)pieceSize);
                y = (int)Math.Ceiling(map.DisplayHeight / 64 / (float)pieceSize);
                xx = pieceSize - (map.DisplayWidth / 64 % pieceSize);
                yy = pieceSize - (map.DisplayHeight / 64 % pieceSize);
                if (xx == pieceSize)
                    xx = 0;
                if (yy == pieceSize)
                    yy = 0;
            }

            puzzleSize = new Point(x, y);

            for (int i = 0; i < xx; i++)
            {
                var s = Game1.random.Next(x);
                while (skinnier.Contains(s))
                {
                    s = Game1.random.Next(x);
                }
                skinnier.Add(s);
            }
            for (int i = 0; i < yy; i++)
            {
                var s = Game1.random.Next(y);
                while (shorter.Contains(s))
                {
                    s = Game1.random.Next(y);
                }
                shorter.Add(s);
            }
            MakePuzzle();
            //MakeWater();
        }

        private void MakeWater()
        {

            waterTiles = new WaterTiles(this.map.Layers[0].LayerWidth, this.map.Layers[0].LayerHeight);
            bool foundAnyWater = false;
            var layer = map.GetLayer("Back");
            if (layer is null)
            {
                waterTiles = null;
                return;
            }
            for (int x = 0; x < map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < map.Layers[0].LayerHeight; y++)
                {
                    Tile tile = layer.Tiles[x, y];
                    if (tile is null)
                        continue;
                    var props = tile.Properties;
                    var props2 = tile.TileIndexProperties;
                    var tr = props.TryGetValue("Water", out var prop) || props2.TryGetValue("Water", out prop);
                    if (tr)
                    {
                        string water_property = prop;
                        if (water_property != null)
                        {
                            foundAnyWater = true;
                            if (water_property == "I")
                            {
                                waterTiles.waterTiles[x, y] = new WaterTiles.WaterTileData(true, false);
                            }
                            else
                            {
                                waterTiles[x, y] = true;
                            }
                        }
                    }
                }
            }
            if (!foundAnyWater)
            {
                waterTiles = null;
            }
        }

        private void MakePuzzle()
        {
            sortedList.Clear();
            pieces = new PuzzlePiece[puzzleSize.X, puzzleSize.Y];
            for (int x = 0; x < pieces.GetLength(0); x++)
            {
                for (int y = 0; y < pieces.GetLength(1); y++)
                {
                    bool empty = true;
                    var tiles = new TileList[skinnier.Contains(x) ? pieceSize - 1 : pieceSize, shorter.Contains(y) ? pieceSize - 1 : pieceSize];
                    var tilePoints = new Point?[tiles.GetLength(0), tiles.GetLength(1)];
                    int xStart = 0;
                    for (int i = 0; i < x; i++)
                    {
                        xStart += skinnier.Contains(i) ? pieceSize - 1 : pieceSize;
                    }
                    int yStart = 0;
                    for (int i = 0; i < y; i++)
                    {
                        yStart += shorter.Contains(i) ? pieceSize - 1 : pieceSize;
                    }
                    for (int x1 = 0; x1 < tiles.GetLength(0); x1++)
                    {
                        for (int y1 = 0; y1 < tiles.GetLength(1); y1++)
                        {
                            var list = new List<Tile>();
                            foreach(var l in map.Layers)
                            {
                                if (l.Id == "Paths")
                                    continue;
                                list.Add(l.Tiles.Array[xStart + x1, yStart + y1]);
                            }

                            if (list.Exists(t => t is not null))
                            {
                                empty = false;
                                tiles[x1, y1] = new TileList(xStart + x1, yStart + y1, list);
                                tilePoints[x1, y1] = tiles[x1, y1].index;
                            }
                        }
                    }
                    if (!empty)
                    {
                        pieces[x, y] = new PuzzlePiece()
                        {
                            _tiles = tiles,
                            tiles = tilePoints,
                            position = Clamp(new Vector2(Game1.random.Next(-border / 8, map.DisplayWidth + border / 8), Game1.random.Next(-border / 8, map.DisplayHeight + border / 8))),
                            index = new Point(x, y),
                            properPosition = new Vector2(xStart, yStart) * 64
                        };
                        sortedList.Add(pieces[x, y]);
                    }
                }
            }
            for (int x = 0; x < pieces.GetLength(0); x++)
            {
                for (int y = 0; y < pieces.GetLength(1); y++)
                {
                    SwapTiles(x, y);
                }
            }
            sortedList.Shuffle();
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].layerDepth = i;
            }
        }

        private void SwapTiles(int x, int y)
        {
            var piece = pieces[x, y];
            if(x < puzzleSize.X - 1)
            {
                SwapTilesH(piece, pieces[x + 1, y]);
            }
            if(y < puzzleSize.Y - 1)
            {
                SwapTilesV(piece, pieces[x, y + 1]);
            }
        }

        private void SwapTilesH(PuzzlePiece left, PuzzlePiece right)
        {
            if (left is null || right is null)
                return;
            int length = left.tiles.GetLength(1);
            int number = 0;
            int last = 0;
            double reassurance = 0;
            while (number == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    var d = Game1.random.NextDouble() - reassurance;
                    if (d < 0.33 && last != 1) // swap right
                    {

                        right.SetExtraTile(3, i, length, left._tiles[left.tiles.GetLength(0) - 1, i]);
                        left.SetTile(left.tiles.GetLength(0) - 1, i, null);
                        number++;
                        last = -1;
                    }
                    else if (d < 0.66 && last != -1) // swap left
                    {
                        left.SetExtraTile(1, i, length, right._tiles[0, i]);
                        right.SetTile(0, i, null);
                        number++;
                        last = 1;
                    }
                    else
                    {
                        last = 0;
                    }
                    if (number >= length - 1)
                        break;
                }
                reassurance += 0.1;
            }
        }

        private void SwapTilesV(PuzzlePiece above, PuzzlePiece below)
        {
            if (above is null || below is null)
                return;
            int length = above.tiles.GetLength(0);
            int number = 0;
            int last = 0;
            double reassurance = 0;
            while(number == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    var d = Game1.random.NextDouble() - reassurance;
                    if (d < 0.33 && last != 1) // swap down
                    {
                        below.SetExtraTile(0, i, length, above._tiles[i, above.tiles.GetLength(1) - 1]);
                        above.SetTile(i, above._tiles.GetLength(1) - 1, null);
                        number++;
                        last = -1;
                    }
                    else if (d < 0.66 && last != -1) // swap up
                    {
                        above.SetExtraTile(2, i, length, below._tiles[i, 0]);
                        below.SetTile(i, 0, null);
                        number++;
                        last = 1;
                    }
                    else
                    {
                        last = 0;
                    }
                    if (number >= length - 1)
                        break;
                }
                reassurance += 0.1;
            }
        }

        private void AddDraggingPieces(PuzzlePiece piece)
        {
            if(!ModEntry.Config.FuseConnected) 
                return;
            for (int i = 0; i < piece._connected.Length; i++)
            {
                if (piece._connected[i] != null && piece._connected[i] != draggingPiece && !draggingPieces.Contains(piece._connected[i]))
                {
                    draggingPieces.Add(piece._connected[i]);
                    var pc = piece._connected[i];
                    sortedList.Remove(piece._connected[i]);
                    sortedList.Add(pc);
                    AddDraggingPieces(piece._connected[i]);
                }
            }
        }

        public List<PuzzlePiece> GetConnected(PuzzlePiece piece, List<PuzzlePiece> pieces = null)
        {
            if(pieces is null)
            {
                pieces = new List<PuzzlePiece>();
            }
            if (!pieces.Contains(piece))
            {
                pieces.Add(piece);
            }
            foreach (var p in piece._connected)
            {
                if (p != null && !pieces.Contains(p))
                {
                    GetConnected(p, pieces);
                }
            }
            return pieces;
        }
        private bool ConnectPieces(PuzzlePiece piece, List<PuzzlePiece> list)
        {
            bool connect = false;
            for (int i = 0; i < 4; i++)
            {
                if (piece._connected[i] != null)
                {
                    if (!list.Contains(piece._connected[i]))
                    {
                        list.Add(piece._connected[i]);
                        if(ConnectPieces(piece._connected[i], list))
                            connect = true;
                    } 
                }
                else
                {
                    Point o;
                    switch (i)
                    {
                        case 0:
                            o = new Point(0, -1);
                            break;
                        case 1:
                            o = new Point(1, 0);
                            break;
                        case 2:
                            o = new Point(0, 1);
                            break;
                        default:
                            o = new Point(-1, 0);
                            break;
                    }
                    if (piece.index.X + o.X < 0 || piece.index.X + o.X >= pieces.GetLength(0) || piece.index.Y + o.Y < 0 || piece.index.Y + o.Y >= pieces.GetLength(1))
                        continue;
                    PuzzlePiece offPiece = pieces[piece.index.X + o.X, piece.index.Y + o.Y];
                    if (offPiece == null)
                        continue;
                    Vector2 offset2;
                    switch (i)
                    {
                        case 0:
                            offset2 = new Vector2(0, -offPiece.tiles.GetLength(1));
                            break;
                        case 1:
                            offset2 = new Vector2(piece.tiles.GetLength(0), 0);
                            break;
                        case 2:
                            offset2 = new Vector2(0, piece.tiles.GetLength(1));
                            break;
                        default:
                            offset2 = new Vector2(-offPiece.tiles.GetLength(0), 0);
                            break;
                    }
                    if (offPiece is not null && ((offPiece.position == piece.position + offset2 * 64) || (!ModEntry.Config.Snap && Vector2.Distance(offPiece.position, piece.position + offset2 * 64) < 64)))
                    {
                        var diff = offPiece.position - offset2 * 64 - piece.position;
                        foreach (var p in GetConnected(piece))
                        {
                            p.position += diff;
                        }
                        piece.connected[i] = offPiece.index;
                        piece._connected[i] = offPiece;
                        offPiece.connected[(i + 2) % 4] = piece.index;
                        offPiece._connected[(i + 2) % 4] = piece;
                        connect = true;
                        if (!list.Contains(offPiece))
                        {
                            ConnectPieces(offPiece, list);
                        }
                    }
                }
            }
            return connect;
        }

        private Vector2 Clamp(Vector2 v)
        {
            var v2 = new Vector2(Math.Clamp(v.X, -border, map.DisplayWidth + border - pieceSize * 64), Math.Clamp(v.Y, -border, map.DisplayHeight + border - pieceSize * 64));
            if (ModEntry.Config.Snap)
            {
                int snap = 64;
                v2 = new Vector2((float)Math.Round(v2.X / snap), (float)Math.Round(v2.Y / snap)) * snap;
            }
            return v2;   
        }

        private Vector2 ClampToRect(Vector2 v)
        {
            return new Vector2(Math.Clamp(v.X, -border, Math.Max(-border, map.DisplayWidth + border - Game1.uiViewport.Width / zoom)), Math.Clamp(v.Y, -border, Math.Max(-border, map.DisplayHeight + border - Game1.uiViewport.Height / zoom)));
        }

        public override void receiveKeyPress(Keys key)
        {
            if ((SButton)key == ModEntry.Config.SnapKey)
            {
                ModEntry.Config.Snap = !ModEntry.Config.Snap;
                ModEntry.SHelper.WriteConfig(ModEntry.Config);
            }
            else if((SButton)key == ModEntry.Config.SolveKey)
            {
                Solve();
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
            {
                exitThisMenu();
                
            }
        }
        public override void update(GameTime time)
        {
            map.Update(time.ElapsedGameTime.Milliseconds);
            //updateWater(time);
            
            if ((Game1.activeClickableMenu is not TitleMenu tm || !tm.startupPreferences.startMuted) && Game1.currentSong?.IsPlaying == false && !AccessTools.StaticFieldRefAccess<Game1, bool>("requestedMusicDirty"))
            {
                ChangeMusic(ModEntry.Config.CurrentMusic);
            }
            if (solving)
            {
                bool done = true;
                foreach(var p in sortedList)
                {
                    if(p.position != p.properPosition)
                    {
                        done = false;
                        var diff = p.properPosition - p.position;
                        if (Math.Abs(diff.X) < 64 && Math.Abs(diff.Y) < 64)
                        {
                            p.position = p.properPosition;
                        }
                        else
                        {
                            diff.Normalize();
                            p.position += diff * 32;
                        }
                    }
                }
                if (done)
                {
                    CheckComplete();
                }
                if (solved)
                {
                    solving = false;
                    ConnectPieces(sortedList[0], new List<PuzzlePiece>());
                }
            }

        }
        public void updateWater(GameTime time)
        {
            this.waterAnimationTimer -= time.ElapsedGameTime.Milliseconds;
            if (this.waterAnimationTimer <= 0)
            {
                this.waterAnimationIndex = (this.waterAnimationIndex + 1) % 10;
                this.waterAnimationTimer = 200;
            }
            this.waterPosition += ((!isFarm) ? ((float)((Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f)) + 1.0) * 0.15000000596046448)) : 0.1f);
            if (this.waterPosition >= 64f)
            {
                this.waterPosition -= 64f;
                this.waterTileFlip = !this.waterTileFlip;
            }
        }
        private void ChangeMusic(string song)
        {
            AccessTools.StaticFieldRefAccess<Game1, bool>("requestedMusicDirty") = true;
            AccessTools.StaticFieldRefAccess<Game1, string>("requestedMusicTrack") = song;
            AccessTools.StaticFieldRefAccess<Game1, bool>("requestedMusicTrackOverrideable") = false;

            if(song != null)
            {
                musicDropdown.SetCurrentItem(song);
                bool changed = false;
                if (ModEntry.Config.CurrentMusic != song)
                {
                    ModEntry.Config.CurrentMusic = song;
                    changed = true;
                }
                if (!ModEntry.Config.PlayingMusic)
                {
                    ModEntry.Config.PlayingMusic = true;
                    changed = true;
                }
                if (changed)
                {
                    ModEntry.SHelper.WriteConfig(ModEntry.Config);
                }
                Game1.changeMusicTrack(song, false, MusicContext.Default);
            }
            else
            {
                Game1.currentSong?.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
            }
        }
        private bool TileContains(Vector2 point, Vector2 pos)
        {
            return point.X >= pos.X * zoom && point.Y >= pos.Y * zoom && point.X <= (pos.X + 64) * zoom && point.Y <= (pos.Y + 64) * zoom;
        }

        private void SetDragging(PuzzlePiece p, int i)
        {
            draggedStart = p.position;
            dragged = Vector2.Zero;
            draggingPiece = p;
            draggingPieces.Clear();
            sortedList.Remove(p);
            sortedList.Add(p);
            AddDraggingPieces(p);
        }
        private void CheckComplete()
        {
            var list = new List<PuzzlePiece>();
            ConnectPieces(sortedList[0], list);
            if (list.Count != sortedList.Count)
                return;
            if( !ModEntry.Config.Snap && Vector2.Distance(sortedList[0].position, sortedList[0].properPosition) < 64 && !solving)
            {
                foreach(var p in sortedList)
                {
                    p.position = p.properPosition;
                }
                solved = true;
                Game1.playSound(ModEntry.Config.SolveSound);
                return;
            }
            else
            {
                foreach (var p in sortedList)
                {
                    if (p.position != p.properPosition)
                        return;
                }
                solved = true;
                Game1.playSound(ModEntry.Config.SolveSound);
            }
        }
        public void BackButtonPressed()
        {
            Game1.playSound("bigDeSelect", null);

            exitThisMenu();
            if (Game1.activeClickableMenu is TitleMenu)
            {
                ModEntry.sgapi.ReturnToMenu();
            }
        }
        public void UpdateSeasonalTileSheets()
        {
            map.DisposeTileSheets(Game1.mapDisplayDevice);
            foreach (TileSheet tilesheet in map.TileSheets)
            {
                string prevImageSource = tilesheet.ImageSource;
                try
                {
                    tilesheet.ImageSource = GameLocation.GetSeasonalTilesheetName(tilesheet.ImageSource, Utility.getSeasonKey((Season)season));
                    Game1.mapDisplayDevice.LoadTileSheet(tilesheet);
                }
                catch
                {
                }
            }
            map.LoadTileSheets(Game1.mapDisplayDevice);
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0,0,0,0.5f));
            int borderWidth = 12;
            IClickableMenu.drawTextureBox(b, (int)(-offset.X * zoom - borderWidth), (int)(-offset.Y * zoom - borderWidth), (int)(map.DisplayWidth * zoom + borderWidth * 2), (int)(map.DisplayHeight * zoom + borderWidth * 2), Color.White);

            var dict = AccessTools.FieldRefAccess<XnaDisplayDevice, Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice as XnaDisplayDevice, "m_tileSheetTextures");
            foreach (var p in sortedList)
            {
                p.Draw(b, offset, zoom, dict, this);
            }

            IClickableMenu.drawTextureBox(b, 0, 0, Game1.viewport.Width, 68, Color.White);
            var musicStr = ModEntry.SHelper.Translation.Get("music");
            var mapStr = ModEntry.SHelper.Translation.Get("map");
            var seasonStr = ModEntry.SHelper.Translation.Get("season");
            var pieceStr = ModEntry.SHelper.Translation.Get("piece-size");
            
            b.DrawString(Game1.smallFont, musicStr, new Vector2(musicDropdown.bounds.X  - 8 - Game1.smallFont.MeasureString(musicStr).X, 20), Color.DarkSlateGray);
            musicDropdown.draw(b, 0, 0);

            if (ModEntry.Config.PlayingMusic)
            {
                stopButton.draw(b, Color.Brown, 1);
            }
            else
            {
                playButton.draw(b);
            }
            nextButton.draw(b);

            b.DrawString(Game1.smallFont, mapStr, new Vector2(mapDropdown.bounds.X - 8 - Game1.smallFont.MeasureString(mapStr).X, 20), Color.DarkSlateGray);
            mapDropdown.draw(b, 0, 0);

            b.DrawString(Game1.smallFont, seasonStr, new Vector2(seasonDropdown.bounds.X - 8 - Game1.smallFont.MeasureString(seasonStr).X, 20), Color.DarkSlateGray);
            seasonDropdown.draw(b, 0, 0);

            b.DrawString(Game1.smallFont, pieceStr, new Vector2(pieceSizeDropdown.bounds.X - 8 - Game1.smallFont.MeasureString(pieceStr).X, 20), Color.DarkSlateGray);
            pieceSizeDropdown.draw(b, 0, 0);

            if (Game1.activeClickableMenu is TitleMenu)
            {
                backButton.draw(b);
            }

            //var no  = ClampToRect(new Vector2(-(Game1.uiViewport.Width / 2 - map.DisplayWidth / 2 * zoom), -(Game1.uiViewport.Height / 2 - map.DisplayHeight / 2 * zoom)) / zoom);
            //b.DrawString(Game1.smallFont, $"{offset}; {no}", new Vector2(0, 1400), Color.Brown);
            drawMouse(b);
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            RebuildElements();
        }
        public override void receiveGamePadButton(Buttons button)
        {
            if (button == Buttons.B)
            {
                BackButtonPressed();
            }
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {

            if (ModEntry.Config.PlayingMusic && stopButton.bounds.Contains(x, y))
            {
                ModEntry.Config.PlayingMusic = false;
                ModEntry.SHelper.WriteConfig(ModEntry.Config);
                Game1.playSound("shiny4");
                ChangeMusic(null);
            }
            else if (!ModEntry.Config.PlayingMusic && playButton.bounds.Contains(x, y))
            {
                ModEntry.Config.PlayingMusic = true;
                ModEntry.SHelper.WriteConfig(ModEntry.Config);
                Game1.playSound("shiny4");
                ChangeMusic(ModEntry.Config.CurrentMusic);
            }
            else if (nextButton.bounds.Contains(x, y))
            {
                if (!ModEntry.Config.PlayingMusic)
                {
                    ModEntry.Config.PlayingMusic = true;
                    ModEntry.SHelper.WriteConfig(ModEntry.Config);
                }
                ChangeMusic(music[(music.IndexOf(ModEntry.Config.CurrentMusic) + 1) % music.Count]);
                Game1.playSound("shiny4");
                return;
            }
            else if (Game1.activeClickableMenu is TitleMenu)
            {
                if (backButton.containsPoint(x, y))
                {
                    BackButtonPressed();
                    return;
                }
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            var mousePos = new Vector2(x, y);
            if (draggingPiece == null && !held)
            {
                held = true;
                if (!solved)
                {

                    for (int i = sortedList.Count - 1; i >= 0; i--)
                    {
                        var p = sortedList[i];
                        for (int tx = 0; tx < p.tiles.GetLength(0); tx++)
                        {
                            for (int ty = 0; ty < p.tiles.GetLength(1); ty++)
                            {
                                if (p.tiles[tx, ty] != null && TileContains(new Vector2(x, y), p.position - offset + new Vector2(tx, ty) * 64))
                                {
                                    SetDragging(p, i);
                                    return;
                                }
                            }
                        }
                        if (p.leftTiles != null)
                            for (int k = 0; k < p.leftTiles.Length; k++)
                            {
                                if (p.leftTiles[k] != null && TileContains(new Vector2(x, y), p.position - offset + new Vector2(-64, k * 64)))
                                {
                                    SetDragging(p, i);
                                    return;
                                }
                            }

                        if (p.rightTiles != null)
                            for (int k = 0; k < p.rightTiles.Length; k++)
                            {
                                if (p.rightTiles[k] != null && TileContains(new Vector2(x, y), p.position - offset + new Vector2(p.tiles.GetLength(0) * 64, k * 64)))
                                {
                                    SetDragging(p, i);
                                    return;
                                }
                            }

                        if (p.topTiles != null)
                            for (int k = 0; k < p.topTiles.Length; k++)
                            {
                                if (p.topTiles[k] != null && TileContains(new Vector2(x, y), p.position - offset + new Vector2(k * 64, -64)))
                                {
                                    SetDragging(p, i);
                                    return;
                                }
                            }

                        if (p.bottomTiles != null)
                            for (int k = 0; k < p.bottomTiles.Length; k++)
                            {
                                if (p.bottomTiles[k] != null && TileContains(new Vector2(x, y), p.position - offset + new Vector2(k * 64, p.tiles.GetLength(1) * 64)))
                                {
                                    SetDragging(p, i);
                                    return;
                                }
                            }
                    }
                }
                if (musicDropdown.bounds.Contains(x, y))
                {
                    musicDropdown.receiveLeftClick(x, y);
                    return;
                }
                else if (mapDropdown.bounds.Contains(x, y))
                {
                    mapDropdown.receiveLeftClick(x, y);
                    return;
                }
                else if (seasonDropdown.bounds.Contains(x, y))
                {
                    seasonDropdown.receiveLeftClick(x, y);
                    return;
                }
                else if (pieceSizeDropdown.bounds.Contains(x, y))
                {
                    pieceSizeDropdown.receiveLeftClick(x, y);
                    return;
                }

            }
            if (draggingPiece != null)
            {
                int amount = 128;
                Vector2 scroll = Vector2.Zero;
                var drag = (mousePos - Game1.oldMouseState.Position.ToVector2()) / zoom;
                if (mousePos.X == 0 && offset.X > -border)
                {
                    scroll = new Vector2(-amount, 0);
                    drag = new Vector2(0, drag.Y);
                }
                else if (mousePos.Y == 0 && offset.Y > -border)
                {
                    scroll = new Vector2(0, -amount);
                    drag = new Vector2(drag.X, 0);
                }
                else if (mousePos.X >= Game1.uiViewport.Width - 1 && offset.X < map.DisplayWidth + border - Game1.uiViewport.Width / zoom)
                {
                    scroll = new Vector2(amount, 0);
                    drag = new Vector2(0, drag.Y);
                }
                else if (mousePos.Y >= Game1.uiViewport.Height - 1 && offset.Y < map.DisplayHeight + border - Game1.uiViewport.Height / zoom)
                {
                    scroll = new Vector2(0, amount);
                    drag = new Vector2(drag.X, 0);
                }
                if (scroll != Vector2.Zero)
                {
                    dragged += scroll + drag;
                    offset = ClampToRect(offset + scroll);
                }
                else
                {
                    dragged += drag;
                }
                var shift = Clamp(draggedStart + dragged) - draggingPiece.position;
                draggingPiece.position += shift;
                if (ModEntry.Config.FuseConnected)
                {
                    foreach (var p in draggingPieces)
                    {
                        p.position += shift;
                    }
                }

            }
            else
            {
                if (!draggingScreen)
                {
                    if (musicDropdown.clicked || musicDropdown.bounds.Contains(x, y))
                    {
                        musicDropdown.leftClickHeld(x, y);
                        return;
                    }
                    if (mapDropdown.clicked || mapDropdown.bounds.Contains(x, y))
                    {
                        mapDropdown.leftClickHeld(x, y);
                        return;
                    }
                    if (seasonDropdown.clicked || seasonDropdown.bounds.Contains(x, y))
                    {
                        seasonDropdown.leftClickHeld(x, y);
                        return;
                    }
                    if (pieceSizeDropdown.clicked || pieceSizeDropdown.bounds.Contains(x, y))
                    {
                        pieceSizeDropdown.leftClickHeld(x, y);
                        return;
                    }
                }
                offset -= (Game1.getMousePosition().ToVector2() - Game1.oldMouseState.Position.ToVector2()) / zoom;
                offset = ClampToRect(offset);
                draggingScreen = true;
            }
        }




        public override void releaseLeftClick(int x, int y)
        {
            held = false;
            draggingScreen = false;
            if (musicDropdown.clicked)
            {
                musicDropdown.leftClickReleased(x, y);
                ChangeMusic(musicDropdown.GetCurrentItem());
            }
            if (mapDropdown.clicked)
            {
                mapDropdown.leftClickReleased(x, y);
                mapPath = mapDropdown.GetCurrentItem();
                ReloadMap();
                RebuildElements();
            }
            if (seasonDropdown.clicked)
            {
                seasonDropdown.leftClickReleased(x, y);
                season = seasonDropdown.selectedOption;
                UpdateSeasonalTileSheets();
                RebuildElements();
            }
            if (pieceSizeDropdown.clicked)
            {
                pieceSizeDropdown.leftClickReleased(x, y);
                pieceSize = int.Parse(pieceSizeDropdown.GetCurrentItem());
                ReloadMap();
                RebuildElements();
            }
            if (draggingPiece != null)
            {
                if (ModEntry.Config.FuseConnected)
                {
                    var list = GetConnected(draggingPiece);
                    bool connect = false;
                    foreach(var p in list.ToArray())
                    {
                        if(ConnectPieces(draggingPiece, new List<PuzzlePiece>()))
                        { 
                            connect = true; 
                        }
                    }
                    if (connect)
                    {
                        Game1.playSound(ModEntry.Config.SnapSound);
                    }
                }
                draggingPiece = null;
                draggingPieces.Clear();
                CheckComplete();
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            var change = Math.Sign(direction) / 20f * zoom;
            var newZoom = Math.Clamp(zoom + change, 0.15f, 10f);

            var mousePos = Game1.getMousePosition().ToVector2();
            var canvasPoint = offset + mousePos / zoom;
            var newCanvasPoint = offset + mousePos / newZoom;
            //offset *= zoom;
            var shift = newCanvasPoint - canvasPoint;
            offset -= shift;
            offset = ClampToRect(offset);
            //offset = Vector2.Zero;
            zoom = newZoom;
        }
        public override bool readyToClose()
        {
            return Game1.activeClickableMenu is not TitleMenu;
        }
    }
    public static class Extensions
    {
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}