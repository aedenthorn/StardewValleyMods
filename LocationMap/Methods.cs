using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Reflection;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LocationMap
{
    public partial class ModEntry
    {
        public static void CheckForMapPage()
        {
            renderTarget = null;
            upperRightCloseButton = null;
            if (!Config.ModEnabled || !Context.IsWorldReady)
                return;
            if(Game1.activeClickableMenu is not GameMenu menu || menu.GetCurrentPage() is not MapPage)
            {
                showingMap = Config.ShowByDefault;
                return;
            }
            if(showingMap && renderTarget is null)
            {
                TakeMapScreenshot(Game1.currentLocation, Config.MapScale);

            }
        }

        public static unsafe void TakeMapScreenshot(GameLocation l, float scale)
        {
            //int start_x = (int)Game1.player.Position.X - openWorldChunkSize * 64 + mapOffset.X * openWorldChunkSize * 128;
            //int start_y = (int)Game1.player.Position.Y - openWorldChunkSize * 64 + mapOffset.Y * openWorldChunkSize * 128;
            int tileSize = 16;


            int width = Math.Min((Game1.viewport.Width - tileSize) / tileSize, l.map.Layers[0].LayerWidth) * 64;
            int height = Math.Min((Game1.viewport.Height - tileSize) / tileSize, l.map.Layers[0].LayerHeight) * 64;

            int start_x = Math.Clamp((int)Game1.player.Position.X - width / 2, 0, l.map.Layers[0].LayerWidth * 64 - width);
            int start_y = Math.Clamp((int)Game1.player.Position.Y - height / 2, 0, l.map.Layers[0].LayerHeight * 64 - height);
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
            catch
            {
                Game1.game1.GraphicsDevice.SetRenderTarget(null);
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
        }
        public static void DrawMap(RenderedActiveMenuEventArgs e)
        {
            if (!showingMap || renderTarget is null)
                return;
            var l = Game1.currentLocation;
            var scale = Game1.options.uiScale;
            int tileSize = 16;
            var menu = Game1.activeClickableMenu;
            int vpHeight = menu.yPositionOnScreen * 2 + menu.height;
            int vpWidth = menu.xPositionOnScreen * 2 + menu.width - (int)(128 / scale);

            int width = Math.Min((Game1.viewport.Width - tileSize) / tileSize, l.map.Layers[0].LayerWidth);
            int height = Math.Min((Game1.viewport.Height - tileSize) / tileSize, l.map.Layers[0].LayerHeight);

            Point vpCenter = new(vpWidth / 2, vpHeight / 2);
            Point topLeft = new(vpCenter.X - width / 2 * tileSize, vpCenter.Y - height / 2 * tileSize);
            

            //Point playerChunk = GetPlayerChunk(Game1.player);
            //Rectangle playerBox = new Rectangle(Game1.player.TilePoint.X - mapSize / 2, Game1.player.TilePoint.Y - mapSize / 2, mapSize, mapSize);

            Game1.drawDialogueBox(Math.Max(0, topLeft.X - 32), Math.Max(-82,topLeft.Y - 96), (int)(Math.Min(vpWidth, width * tileSize + 64)), (int)(Math.Min(vpHeight, height * tileSize + 32) + 96), false, true);

            mapRect = new Rectangle(topLeft.X, topLeft.Y, (int)(width * tileSize), (int)(height * tileSize));

            e.SpriteBatch.Draw(renderTarget, mapRect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);

            //foreach (var player in Game1.currentLocation.farmers)
            //{
            //    float alpha = player == Game1.player ? 1f : 0.75f;
            //    Vector2 pos = new Vector2(topLeft.X, topLeft.Y) + player.Position * tileSize / 64 - new Vector2(tileSize / 2, tileSize * 1.5f);
            //    player.FarmerRenderer.drawMiniPortrat(e.SpriteBatch, pos, 0.00011f, 2f, 2, player, alpha);
            //}
            if (upperRightCloseButton is null)
            {
                int space = 32;
                upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Math.Max(0, Math.Min(topLeft.X - 32 + vpWidth - 8, topLeft.X - 32 + width * tileSize - 16)), Math.Max(16, topLeft.Y), 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
                westButton = new ClickableTextureComponent(new Rectangle(vpCenter.X - width * tileSize / 2 - 44 - space, vpCenter.Y - 22, 44, 44), Game1.mouseCursors, new Rectangle(8, 264, 44, 44), 1f, false);
                eastButton = new ClickableTextureComponent(new Rectangle(vpCenter.X + width * tileSize / 2 + space, vpCenter.Y - 22, 44, 44), Game1.mouseCursors, new Rectangle(12, 201, 44, 44), 1f, false);
                northButton = new ClickableTextureComponent(new Rectangle(vpCenter.X - 22, vpCenter.Y - height * tileSize / 2 - space - 44, 44, 44), Game1.mouseCursors, new Rectangle(76, 74, 44, 44), 1f, false);
                southButton = new ClickableTextureComponent(new Rectangle(vpCenter.X - 22, vpCenter.Y + height * tileSize / 2 + space, 44, 44), Game1.mouseCursors, new Rectangle(12, 74, 44, 44), 1f, false);
            }
            upperRightCloseButton.draw(e.SpriteBatch);
            Game1.activeClickableMenu.drawMouse(e.SpriteBatch);
        }

    }
}