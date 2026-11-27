using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SGJigsaw
{
    public class PuzzlePiece
    {
        [JsonIgnore]
        public TileList[,] _tiles;
        [JsonIgnore]
        public TileList[] _leftTiles;
        [JsonIgnore]
        public TileList[] _rightTiles;
        [JsonIgnore]
        public TileList[] _topTiles;
        [JsonIgnore]
        public TileList[] _bottomTiles;
        [JsonIgnore]
        public PuzzlePiece[] _connected = new PuzzlePiece[4];

        [JsonIgnore]
        private Dictionary<Point, TileList> drawTiles;

        public Point?[,] tiles;
        public Point?[] leftTiles;
        public Point?[] rightTiles;
        public Point?[] topTiles;
        public Point?[] bottomTiles;
        public Point?[] connected = new Point?[4];
        

        public Vector2 position;
        public Vector2 properPosition;
        public Point index;
        public float layerDepth;

        public void Initialize()
        {
            _tiles = new TileList[tiles.GetLength(0), tiles.GetLength(1)];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] != null)
                        _tiles[x, y] = JigsawGameMenu.instance.tileDict[tiles[x, y].Value.X, tiles[x, y].Value.Y];
                }
            }
            for (int i = 0; i < 4; i++)
            {
                GetWhichTiles(i, out FieldInfo tf, out FieldInfo pf);
                var p = (Point?[])pf.GetValue(this);
                if(p != null)
                {
                    var t = new TileList[p.Length];
                    for (int j = 0; j < p.Length; j++)
                    {
                        if (p[j] != null)
                            t[j] = JigsawGameMenu.instance.tileDict[p[j].Value.X, p[j].Value.Y];
                    }
                    tf.SetValue(this, t);
                }
            }
            _connected = new PuzzlePiece[4];
            for (int i = 0; i < 4; i++)
            {
                if (connected[i] != null)
                    _connected[i] = JigsawGameMenu.instance.pieces[connected[i].Value.X, connected[i].Value.Y];
            }
        }

        public void SetTile(int x, int y, TileList tl)
        {
            _tiles[x, y] = tl;
            tiles[x, y] = tl?.index;

        }
        public void SetExtraTile(int which, int i, int length, TileList tl)
        {
            GetWhichTiles(which, out FieldInfo tf, out FieldInfo pf);

            var t = (TileList[])tf.GetValue(this);
            var p = (Point?[])pf.GetValue(this);
            if (t == null)
            {
                t = new TileList[length];
            }
            if (p == null)
            {
                p = new Point?[length];
            }

            t[i] = tl;
            p[i] = tl?.index;

            tf.SetValue(this, t);
            pf.SetValue(this, p);

        }
        public void GetWhichTiles(int which, out FieldInfo tf, out FieldInfo pf)
        {
            switch (which)
            {
                case 0:
                    pf = typeof(PuzzlePiece).GetField(nameof(topTiles));
                    tf = typeof(PuzzlePiece).GetField(nameof(_topTiles));
                    break;
                case 1:
                    pf = typeof(PuzzlePiece).GetField(nameof(rightTiles));
                    tf = typeof(PuzzlePiece).GetField(nameof(_rightTiles));
                    break;
                case 2:
                    pf = typeof(PuzzlePiece).GetField(nameof(bottomTiles));
                    tf = typeof(PuzzlePiece).GetField(nameof(_bottomTiles));
                    break;
                default:
                    pf = typeof(PuzzlePiece).GetField(nameof(leftTiles));
                    tf = typeof(PuzzlePiece).GetField(nameof(_leftTiles));
                    break;
            }
        }
        public void GetDrawTiles()
        {
            drawTiles = new Dictionary<Point, TileList>();
            if (_topTiles != null)
            {
                for (int i = 0; i < _topTiles.Length; i++)
                {
                    if (_topTiles[i] != null)
                    {
                        drawTiles[new Point(i, -1)] = _topTiles[i];
                    }
                }
            }
            if (_leftTiles != null)
            {
                for (int i = 0; i < _leftTiles.Length; i++)
                {
                    if (_leftTiles[i] != null)
                    {
                        drawTiles[new Point(-1, i)] = _leftTiles[i];

                    }
                }
            }
            for (int x = 0; x < _tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    if (_tiles[x, y] != null)
                    {
                        drawTiles[new Point(x, y)] = _tiles[x, y];
                    }
                }
            }
            if (_rightTiles != null)
            {
                for (int i = 0; i < _rightTiles.Length; i++)
                {
                    if (_rightTiles[i] != null)
                    {
                        drawTiles[new Point(_tiles.GetLength(0), i)] = _rightTiles[i];
                    }
                }
            }
            if (_bottomTiles != null)
            {
                for (int i = 0; i < _bottomTiles.Length; i++)
                {
                    if (_bottomTiles[i] != null)
                    {
                        drawTiles[new Point(i, _tiles.GetLength(1))] = _bottomTiles[i];
                    }
                }
            }
        }
        public void Draw(SpriteBatch b, Vector2 offset, float zoom, Dictionary<TileSheet, Texture2D> dict, JigsawGameMenu menu)
        {
            if(drawTiles is null)
            {
                GetDrawTiles();
            }
            //if (!menu.solved && ModEntry.Config.ShowOutlines && !Array.Exists(connected, c => c != null))
            //{
            //    ModEntry.Config.OutlineThickness = 4;
            //    float thickness = ModEntry.Config.OutlineThickness * zoom;
            //    Point size = (new Vector2(64 + thickness * 2, 64 + thickness * 2) * zoom).ToPoint();
            //    foreach (var t in drawTiles)
            //    {
            //        var pos = (position - offset) * zoom + t.Key.ToVector2() * 64 * zoom - new Vector2(thickness, thickness) * zoom;

            //        b.Draw(Game1.staminaRect, new Rectangle(pos.ToPoint(), size), ModEntry.Config.OutlineColor);
            //    }
            //}
            foreach (var t in drawTiles)
            {
                t.Value.Draw(b, position, layerDepth, dict, offset, zoom, t.Key, menu);
            }
        }



        public Location VectorToLocation(Vector2 v)
        {
            return new Location((int)v.X, (int)v.Y);
        }
    }
}