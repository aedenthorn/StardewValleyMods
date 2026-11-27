using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace SGJigsaw
{
    public class TileList
    {
        public List<Tile> list;
        public Point index;

        public TileList(int x, int y, List<Tile> l)
        {
            index = new Point(x, y);
            list = l;
        }

        public void Draw(SpriteBatch b, Vector2 position, float layerDepth, Dictionary<TileSheet, Texture2D> dict, Vector2 offset, float zoom, Point xy, JigsawGameMenu menu)
        {
            var x = xy.X; 
            var y = xy.Y;
            for (int j = 0; j < list.Count; j++)
            {
                var tile = list[j];
                if (tile != null)
                {

                    if (!dict.TryGetValue(tile.TileSheet, out var texture2D))
                    {
                        Game1.mapDisplayDevice.LoadTileSheet(tile.TileSheet);
                        texture2D = dict[tile.TileSheet];
                    }
                    if (texture2D?.IsDisposed == false)
                    {
                        var pos = (position - offset) * zoom + new Vector2(x, y) * 64 * zoom;
                        b.Draw(texture2D, pos, ToXNA(tile.TileSheet.GetTileImageBounds(tile.TileIndex)), Color.White, 0f, Vector2.Zero, zoom * 4, SpriteEffects.None, (layerDepth + j) / 10000f);
                        continue;
                        //x += (int)properPosition.X / 64;
                        //y += (int)properPosition.Y / 64;
                        //if (tile.TileIndexProperties.ContainsKey("Water") && menu.waterTiles.waterTiles[x, y].isWater && menu.waterTiles.waterTiles[x, y].isVisible)
                        //{
                        //    bool flag = y == menu.map.Layers[0].LayerHeight - 1 || !menu.waterTiles[x, y + 1];
                        //    bool topY = y == 0 || !menu.waterTiles[x, y - 1];
                        //    var waterPos1 = -(int)((!topY) ? menu.waterPosition : 0f);
                        //    var waterPos2 = (int)-menu.waterPosition;
                        //    var rect = new Rectangle((pos).ToPoint(), new Point((int)(64 * zoom), (int)(64 * zoom)));
                        //    b.Draw(Game1.mouseCursors, rect, new Rectangle(menu.waterAnimationIndex * 64, 2064 + (((x + y) % 2 == 0) ? (menu.waterTileFlip ? 128 : 0) : (menu.waterTileFlip ? 0 : 128)) + (topY ? ((int)menu.waterPosition) : 0) + waterPos1, 64, 64 + (topY ? ((-(int)menu.waterPosition)) : 0) - waterPos1), menu.waterColor, 0f, Vector2.Zero, SpriteEffects.None, 0.56f);
                        //    if (flag)
                        //    {
                        //        b.Draw(Game1.mouseCursors, rect, new Rectangle(menu.waterAnimationIndex * 64, 2064 + (((x + (y + 1)) % 2 == 0) ? (menu.waterTileFlip ? 128 : 0) : (menu.waterTileFlip ? 0 : 128)) + waterPos2, 64, 64 - (int)(64f - menu.waterPosition) - 1 - waterPos2), menu.waterColor, 0f, Vector2.Zero, SpriteEffects.None, 0.56f);
                        //    }
                        //}
                    }
                }
            }
        }

        private Rectangle ToXNA(xTile.Dimensions.Rectangle r)
        {
            return new Rectangle(r.X, r.Y, r.Width, r.Height);
        }
    }
}