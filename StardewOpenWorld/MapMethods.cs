using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Reflection;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {
        public static unsafe void TakeMapScreenshot(GameLocation screenshotLocation, float scale)
        {
            //int start_x = (int)Game1.player.Position.X - openWorldChunkSize * 64 + mapOffset.X * openWorldChunkSize * 128;
            //int start_y = (int)Game1.player.Position.Y - openWorldChunkSize * 64 + mapOffset.Y * openWorldChunkSize * 128;

            int width = Config.MapTilesDimension;
            int height = Config.MapTilesDimension;
            int start_x = (int)Game1.player.Position.X - width / 2;
            int start_y = (int)Game1.player.Position.Y - height / 2;
            SKSurface map_bitmap = null;
            int scaled_width;
            int scaled_height;
            for (; ; )
            {
                bool failed = false;
                scaled_width = (int)((float)width * scale);
                scaled_height = (int)((float)height * scale);
                try
                {
                    map_bitmap = SKSurface.Create(new SKImageInfo(scaled_width, scaled_height, SKColorType.Rgb888x, SKAlphaType.Opaque));
                }
                catch (Exception e)
                {
                    failed = true;
                }
                if (failed)
                {
                    scale -= 0.25f;
                }
                if (scale <= 0f)
                {
                    return;
                }
                if (!failed)
                {
                    break;
                }
            }
            int chunk_size = 2048;
            int scaled_chunk_size = (int)((float)chunk_size * scale);
            xTile.Dimensions.Rectangle old_viewport = Game1.viewport;
            bool old_display_hud = Game1.displayHUD;
            Game1.game1.takingMapScreenshot = true;
            float old_zoom_level = Game1.options.baseZoomLevel;
            Game1.options.baseZoomLevel = 1f;
            RenderTarget2D cached_lightmap = (RenderTarget2D)AccessTools.Field(typeof(Game1), "_lightmap").GetValue(null);
            AccessTools.Field(typeof(Game1), "_lightmap").SetValue(null, null);
            bool fail = false;
            try
            {
                typeof(Game1).GetMethod("allocateLightmap", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { chunk_size, chunk_size });
                int chunks_wide = (int)Math.Ceiling((double)((float)scaled_width / (float)scaled_chunk_size));
                int chunks_high = (int)Math.Ceiling((double)((float)scaled_height / (float)scaled_chunk_size));
                for (int y_offset = 0; y_offset < chunks_high; y_offset++)
                {
                    for (int x_offset = 0; x_offset < chunks_wide; x_offset++)
                    {
                        int current_width = scaled_chunk_size;
                        int current_height = scaled_chunk_size;
                        int current_x = x_offset * scaled_chunk_size;
                        int current_y = y_offset * scaled_chunk_size;
                        if (current_x + scaled_chunk_size > scaled_width)
                        {
                            current_width += scaled_width - (current_x + scaled_chunk_size);
                        }
                        if (current_y + scaled_chunk_size > scaled_height)
                        {
                            current_height += scaled_height - (current_y + scaled_chunk_size);
                        }
                        if (current_height > 0 && current_width > 0)
                        {
                            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(current_x, current_y, current_width, current_height);
                            RenderTarget2D render_target = new RenderTarget2D(Game1.graphics.GraphicsDevice, chunk_size, chunk_size, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                            Game1.viewport = new xTile.Dimensions.Rectangle(x_offset * chunk_size + start_x, y_offset * chunk_size + start_y, chunk_size, chunk_size);
                            typeof(Game1).GetMethod("_draw", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Game1.game1, new object[] { Game1.currentGameTime, render_target });
                            RenderTarget2D scaled_render_target = new RenderTarget2D(Game1.graphics.GraphicsDevice, current_width, current_height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                            Game1.game1.GraphicsDevice.SetRenderTarget(scaled_render_target);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, null);
                            Color color = Color.White;
                            Game1.spriteBatch.Draw(render_target, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(render_target.Bounds), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                            render_target.Dispose();
                            Game1.game1.GraphicsDevice.SetRenderTarget(null);
                            Color[] colors = new Color[current_width * current_height];
                            scaled_render_target.GetData<Color>(colors);
                            SKBitmap portion_bitmap = new SKBitmap(rect.Width, rect.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
                            byte* ptr = (byte*)portion_bitmap.GetPixels().ToPointer();
                            for (int row = 0; row < current_height; row++)
                            {
                                for (int col = 0; col < current_width; col++)
                                {
                                    *(ptr++) = colors[col + row * current_width].R;
                                    *(ptr++) = colors[col + row * current_width].G;
                                    *(ptr++) = colors[col + row * current_width].B;
                                    *(ptr++) = byte.MaxValue;
                                }
                            }
                            SKPaint paint = new SKPaint();
                            map_bitmap.Canvas.DrawBitmap(portion_bitmap, SKRect.Create((float)rect.X, (float)rect.Y, (float)current_width, (float)current_height), paint);
                            portion_bitmap.Dispose();
                            scaled_render_target.Dispose();
                        }
                    }
                }
                renderTarget = Texture2D.FromStream(Game1.game1.GraphicsDevice, map_bitmap.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream());
                map_bitmap.Dispose();
            }
            catch (Exception e2)
            {
                Game1.game1.GraphicsDevice.SetRenderTarget(null);
                fail = true;
            }
            if (AccessTools.Field(typeof(Game1), "_lightmap").GetValue(null) != null)
            {
                (AccessTools.Field(typeof(Game1), "_lightmap").GetValue(null) as RenderTarget2D).Dispose();
                AccessTools.Field(typeof(Game1), "_lightmap").SetValue(null, null);
            }
            AccessTools.Field(typeof(Game1), "_lightmap").SetValue(null, cached_lightmap);
            Game1.options.baseZoomLevel = old_zoom_level;
            Game1.game1.takingMapScreenshot = false;
            Game1.displayHUD = old_display_hud;
            Game1.viewport = old_viewport;
            if (fail)
            {
                return;
            }
        }
        public static void DrawMap(RenderedActiveMenuEventArgs e)
        {
            if (!showingMap)
                return;
            var menu = Game1.activeClickableMenu;
            int mapSize = openWorldChunkSize;
            int vpHeight = menu.yPositionOnScreen * 2 + menu.height;
            int vpWidth = menu.xPositionOnScreen * 2 + menu.width - (int)(128 / Game1.options.uiScale);
            Point vpCenter = new(vpWidth / 2, vpHeight / 2);
            int tileSize = 16;
            mapSize = Math.Min(Math.Min(vpHeight - 32, vpWidth - 32) / 16, mapSize);
            var scale = Game1.options.uiScale;

            Point playerChunk = GetPlayerChunk(Game1.player);
            Rectangle playerBox = new Rectangle(Game1.player.TilePoint.X - mapSize / 2, Game1.player.TilePoint.Y - mapSize / 2, mapSize, mapSize);

            if (renderTarget is null)
            {
                return;

                /*
                if (renderTarget is null)
                {
                }

                renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, mapSize * tileSize, mapSize * tileSize);
                Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget);

                var renderBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
                renderBatch.Begin();

                foreach (var offset in GetSurroundingPointArray(true))
                {
                    var cp = playerChunk + offset;
                    Vector2 startPos = Vector2.Zero;
                    for (int i = 0; i < openWorldLocation.Map.Layers.Count; i++)
                    {
                        var l = openWorldLocation.Map.Layers[i];
                        var tiles = GetChunkTiles(l.Id, cp.X, cp.Y);
                        for (int x = 0; x < openWorldChunkSize; x++)
                        {
                            for (int y = 0; y < openWorldChunkSize; y++)
                            {
                                var ap = GetAbsolutePosition(cp, x, y);
                                if (!playerBox.Contains(ap))
                                    continue;
                                var rp = ap - playerBox.Location;
                                var pos = startPos + rp.ToVector2() * tileSize;
                                if (tiles == null || tiles[x, y] == null)
                                {
                                    if (l.Id == "Back")
                                    {
                                        renderBatch.Draw(Game1.staminaRect, new Rectangle(pos.ToPoint(), new Point(tileSize, tileSize)), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 1);
                                    }
                                }
                                else
                                {
                                    var tile = tiles[x, y];
                                    Texture2D texture2D;
                                    var m_tileSheetTextures = AccessTools.FieldRefAccess<XnaDisplayDevice, Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice as XnaDisplayDevice, "m_tileSheetTextures");
                                    if (!m_tileSheetTextures.TryGetValue(tile.TileSheet, out texture2D))
                                    {
                                        Game1.mapDisplayDevice.LoadTileSheet(tile.TileSheet);
                                    }
                                    texture2D = m_tileSheetTextures[tile.TileSheet];
                                    if (!texture2D.IsDisposed)
                                    {
                                        renderBatch.Draw(texture2D, new Rectangle(pos.ToPoint(), new Point(tileSize, tileSize)), tile.TileSheet.GetTileImageBounds(tile.TileIndex).ToXna(), Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
                                    }
                                }
                                if (i == openWorldLocation.Map.Layers.Count - 1)
                                {
                                    if(openWorldLocation.Objects.TryGetValue(ap.ToVector2(), out var obj))
                                    {
                                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                                        renderBatch.Draw(itemData.GetTexture(), new Rectangle(pos.ToPoint() - new Point(0, (obj.bigCraftable.Value ? tileSize : 0)), new Point(tileSize, tileSize * (obj.bigCraftable.Value ? 2 : 1))), itemData.GetSourceRect(0, obj.ParentSheetIndex), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1);
                                    }
                                    else if(openWorldLocation.terrainFeatures.TryGetValue(ap.ToVector2(), out var tf))
                                    {
                                        if (tf is Tree tree)
                                        {
                                            renderBatch.Draw(tree.texture.Value, new Rectangle(pos.ToPoint(), new Point(tileSize *2, tileSize * 4)), Tree.treeTopSourceRect, Color.White, 0f, Vector2.Zero, tree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
                                        }
                                        else if (tf is Grass grass)
                                        {
                                            renderBatch.Draw(grass.texture.Value, new Rectangle(pos.ToPoint(), new Point(tileSize, tileSize)), new Rectangle?(new Rectangle(0, grass.grassSourceOffset.Value, 15, 20)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, (pos.Y + 16f - 20f) / 10000f + pos.X / 10000000f);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                renderBatch.End();
                Game1.graphics.GraphicsDevice.SetRenderTarget(null);
                */
                upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Math.Max(0, vpCenter.X - mapSize / 2 * tileSize - 32) + Math.Min(Game1.viewport.Width -8, mapSize * tileSize - 16), Math.Max(16, vpCenter.Y- mapSize / 2 * tileSize), 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
            }
            Game1.drawDialogueBox(Math.Max(0, vpCenter.X - mapSize / 2 * tileSize - 32), Math.Max(-82,vpCenter.Y - mapSize / 2 * tileSize - 96), (int)(Math.Min(vpWidth, mapSize * tileSize + 64)), (int)(Math.Min(vpHeight + 98, mapSize * tileSize + 128)), false, true);

            mapRect = new Rectangle(vpCenter.X - mapSize / 2 * tileSize, vpCenter.Y - mapSize / 2 * tileSize, (int)(mapSize * tileSize), (int)(mapSize * tileSize));

            e.SpriteBatch.Draw(renderTarget, new Rectangle((int)(vpCenter.X - mapSize / 2 * tileSize), (int)(vpCenter.Y - mapSize / 2 * tileSize), mapSize * tileSize, mapSize * tileSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
            foreach (Farmer player in Game1.getOnlineFarmers())
            {
                if (!player.currentLocation.Name.Equals(locName))
                    continue;
                float alpha = player == Game1.player ? 1f : 0.75f;
                if (!playerBox.Contains(player.Tile))
                    continue;
                Vector2 pos = player.Position - Game1.player.Position + new Vector2(vpWidth / 2, vpHeight / 2 - 16) ;
                player.FarmerRenderer.drawMiniPortrat(e.SpriteBatch, pos, 0.00011f, 2f, 2, player, alpha);
            }
            if(upperRightCloseButton is null)
            {
                int space = 32;
                upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Math.Max(0, Math.Min(vpCenter.X - mapSize / 2 * tileSize - 32 + vpWidth - 8, vpCenter.X - mapSize / 2 * tileSize - 32 + mapSize * tileSize - 16)), Math.Max(16, vpCenter.Y - mapSize / 2 * tileSize), 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
                westButton = new ClickableTextureComponent(new Rectangle(vpCenter.X - mapSize * tileSize / 2  - 44 - space, vpCenter.Y - 22, 44, 44), Game1.mouseCursors, new Rectangle(8, 264, 44, 44), 1f, false);
                eastButton = new ClickableTextureComponent(new Rectangle(vpCenter.X + mapSize * tileSize / 2 + space, vpCenter.Y - 22, 44, 44), Game1.mouseCursors, new Rectangle(12, 201, 44, 44), 1f, false);
                northButton = new ClickableTextureComponent(new Rectangle(vpCenter.X - 22, vpCenter.Y - mapSize * tileSize / 2 - space - 44, 44, 44), Game1.mouseCursors, new Rectangle(76, 74, 44, 44), 1f, false);
                southButton = new ClickableTextureComponent(new Rectangle(vpCenter.X - 22, vpCenter.Y + mapSize * tileSize / 2 + space, 44, 44), Game1.mouseCursors, new Rectangle(12, 74, 44, 44), 1f, false);
            }
            upperRightCloseButton.draw(e.SpriteBatch);
            //westButton.draw(e.SpriteBatch);
            //eastButton.draw(e.SpriteBatch);
            //northButton.draw(e.SpriteBatch);
            //southButton.draw(e.SpriteBatch);
            Game1.activeClickableMenu.drawMouse(e.SpriteBatch);


        }

    }
}