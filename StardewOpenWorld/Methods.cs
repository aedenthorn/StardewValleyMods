using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {


        private bool CheckPlayerWarp()
        {
            var p = GetTileFromName(Game1.player.currentLocation.Name);

            if (Game1.player.Position.X < 0)
            {
                if (p.X > 0)
                {
                    WarpToOpenWorldTile(p.X - 1, p.Y, Game1.player.Position + new Vector2(openWorldTileSize * 64, 0));
                    return true;
                }
                else
                {
                    Game1.player.Position = new Vector2(0, Game1.player.Position.Y);
                }
            }
            if (Game1.player.Position.X >= openWorldTileSize * 64)
            {
                if (p.X < 199)
                {
                    WarpToOpenWorldTile(p.X + 1, p.Y, Game1.player.Position + new Vector2(-openWorldTileSize * 64, 0));
                    return true;
                }
                else
                {
                    Game1.player.Position = new Vector2(openWorldTileSize * 64, Game1.player.Position.Y);
                }
            }
            if (Game1.player.Position.Y < 0)
            {
                if (p.Y > 0)
                {
                    WarpToOpenWorldTile(p.X, p.Y - 1, Game1.player.Position + new Vector2(0, openWorldTileSize * 64));
                    return true;
                }
                else
                {
                    Game1.player.Position = new Vector2(Game1.player.Position.X, 0);
                }
            }
            if (Game1.player.Position.Y >= openWorldTileSize * 64)
            {
                if (p.Y < 199)
                {
                    WarpToOpenWorldTile(p.X, p.Y + 1, Game1.player.Position + new Vector2(0, -openWorldTileSize * 64));
                    return true;
                }
                else
                {
                    Game1.player.Position = new Vector2(Game1.player.Position.X, openWorldTileSize * 64);
                }
            }
            return false;
        }
        private static Vector2 GetTileFromName(string name)
        {
            var split = name.Split('_');
            return new Vector2(int.Parse(split[1]), int.Parse(split[2]));
        }

        private static void WarpToOpenWorldTile(float x, float y, Vector2 newPosition)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            Game1.locationRequest = Game1.getLocationRequest($"{tilePrefix}_{x}_{y}", false);

            GameLocation previousLocation = Game1.player.currentLocation;
            Multiplayer mp = (Multiplayer)AccessTools.Field(typeof(Game1), "multiplayer").GetValue(null);
            if (Game1.emoteMenu != null)
            {
                Game1.emoteMenu.exitThisMenuNoSound();
            }
            if (Game1.client != null && Game1.currentLocation != null)
            {
                Game1.currentLocation.StoreCachedMultiplayerMap(mp.cachedMultiplayerMaps);
            }
            Game1.currentLocation.cleanupBeforePlayerExit();
            mp.broadcastLocationDelta(Game1.currentLocation);
            bool hasResetLocation = false;
            Game1.displayFarmer = true;
            
            Game1.player.Position = newPosition;
            
            Game1.currentLocation = Game1.locationRequest.Location;
            if (!Game1.IsClient)
            {
                Game1.locationRequest.Loaded(Game1.locationRequest.Location);
                Game1.currentLocation.resetForPlayerEntry();
                hasResetLocation = true;
            }
            Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);
            if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayWidth <= Game1.viewport.Width)
            {
                Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
            }
            if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayHeight <= Game1.viewport.Height)
            {
                Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
            }
            Game1.checkForRunButton(Game1.GetKeyboardState(), true);
            Game1.player.FarmerSprite.PauseForSingleAnimation = false;
            if (Game1.player.ActiveObject != null)
            {
                Game1.player.showCarrying();
            }
            else
            {
                Game1.player.showNotCarrying();
            }
            if (Game1.IsClient)
            {
                if (Game1.locationRequest.Location != null && Game1.locationRequest.Location.Root.Value != null && mp.isActiveLocation(Game1.locationRequest.Location))
                {
                    Game1.currentLocation = Game1.locationRequest.Location;
                    Game1.locationRequest.Loaded(Game1.locationRequest.Location);
                    if (!hasResetLocation)
                    {
                        Game1.currentLocation.resetForPlayerEntry();
                    }
                    Game1.player.currentLocation = Game1.currentLocation;
                    Game1.locationRequest.Warped(Game1.currentLocation);
                    Game1.currentLocation.updateSeasonalTileSheets(null);
                    if (Game1.IsDebrisWeatherHere(null))
                    {
                        Game1.populateDebrisWeatherArray();
                    }
                    Game1.warpingForForcedRemoteEvent = false;
                    Game1.locationRequest = null;
                }
                else
                {
                    Game1.requestLocationInfoFromServer();
                }
            }
            else
            {
                Game1.player.currentLocation = Game1.locationRequest.Location;
                Game1.locationRequest.Warped(Game1.locationRequest.Location);
                Game1.locationRequest = null;
            }
            s.Stop();
            SMonitor.Log($"Warped in {s.ElapsedMilliseconds} ms");
            ReloadFarmerTiles();
        }

        private static void ReloadFarmerTiles()
        {
            List<string> tileNames = new List<string>();
            foreach(Farmer f in Game1.getAllFarmers())
            {
                if (!f.currentLocation.Name.StartsWith(tilePrefix))
                    continue;
                var t = GetTileFromName(f.currentLocation.Name);
                if (!tileNames.Contains($"{tilePrefix}_{t.X}_{t.Y}"))
                    tileNames.Add($"{tilePrefix}_{t.X}_{t.Y}");
                var ts = Utility.getSurroundingTileLocationsArray(t);
                foreach(var v in ts)
                {
                    if (!tileNames.Contains($"{tilePrefix}_{v.X}_{v.Y}"))
                        tileNames.Add($"{tilePrefix}_{v.X}_{v.Y}");
                }
            }
            Thread backgroundThread = new Thread(() => ReloadOpenWorldTiles(tileNames));
            backgroundThread.Start();
        }

        private static void ReloadOpenWorldTiles(List<string> tileNames)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            for (int i = Game1.locations.Count - 1; i >= 0; i--)
            {
                var name = Game1.locations[i].Name;
                if (!name.StartsWith(tilePrefix))
                    continue;
                if (!tileNames.Contains(name))
                {
                    StoreTileInfo(Game1.locations[i]);
                    Game1.locations.RemoveAt(i);
                    SMonitor.Log($"removed location {name}");
                }
                else
                {
                    tileNames.Remove(name);
                }
            }
            SMonitor.Log($"Stored info in {s.ElapsedMilliseconds} ms");
            var mapName = SHelper.ModContent.GetInternalAssetName("assets/StardewOpenWorldTile.tmx").BaseName;
            foreach (var name in tileNames)
            {
                s.Restart();
                var gl = new GameLocation(mapName, name);
                ApplyTileInfo(gl);
                Game1.locations.Add(gl);
                SMonitor.Log($"added location {gl.Name} in {s.ElapsedMilliseconds} ms");
            }
        }

        private static void ApplyTileInfo(GameLocation gl)
        {
            Stopwatch s = new Stopwatch();

            var v = GetTileFromName(gl.Name);
            var rect = GetTileRect(v);
            var offset = new Vector2(rect.X, rect.Y);
            var back = gl.Map.GetLayer("Back");
            var mainSheet = gl.Map.GetTileSheet("outdoors");
            if (back.Tiles[0,0] == null)
            {
                s.Start();
                SMonitor.Log($"applying default tiles");
                for (int y = 0; y < openWorldTileSize; y++)
                {
                    for (int x = 0; x < openWorldTileSize; x++)
                    {
                        var tile = new StaticTile(back, mainSheet, BlendMode.Alpha, 0);
                        var which = Game1.random.NextDouble();
                        if (which < 0.025f)
                        {
                            tile.TileIndex = 304;
                        }
                        else if (which < 0.05f)
                        {
                            tile.TileIndex = 305;

                        }
                        else if (which < 0.15f)
                        {
                            tile.TileIndex = 300;
                        }
                        else
                        {
                            tile.TileIndex = 351;
                        }
                        back.Tiles[x, y] = tile;
                    }
                }
                SMonitor.Log($"applied defaults in {s.ElapsedMilliseconds} ms");
                s.Restart();
            }
            var objs = openWorldLocation.objects.Pairs.Where(o => rect.Contains(o.Key.X, o.Key.Y));
            foreach (var obj in objs)
            {
                gl.objects[obj.Key - offset] = obj.Value;
            }
            SMonitor.Log($"applied objects in {s.ElapsedMilliseconds} ms");
            s.Restart();
            var tfs = openWorldLocation.terrainFeatures.Pairs.Where(o => rect.Contains(o.Key.X, o.Key.Y));
            foreach (var tf in tfs)
            {
                gl.terrainFeatures[tf.Key - offset] = tf.Value;
            }
            SMonitor.Log($"applied tfs in {s.ElapsedMilliseconds} ms");
            s.Stop();
        }
        private static void StoreTileInfo(GameLocation gl)
        {
            var v = GetTileFromName(gl.Name);
            var rect = GetTileRect(v);
            var offset = new Vector2(rect.X, rect.Y);
            var objs = openWorldLocation.objects.Pairs.Where(o => rect.Contains(o.Key.X, o.Key.Y)).ToArray();
            foreach (var obj in objs)
            {
                openWorldLocation.objects.Remove(obj.Key);
            }
            foreach (var obj in gl.objects.Pairs)
            {
                openWorldLocation.objects[obj.Key + offset] = obj.Value;
            }

            var tfs = openWorldLocation.terrainFeatures.Pairs.Where(o => rect.Contains(o.Key.X, o.Key.Y));
            foreach (var tf in tfs)
            {
                openWorldLocation.objects.Remove(tf.Key);
            }
            foreach (var tf in gl.terrainFeatures.Pairs)
            {
                openWorldLocation.terrainFeatures[tf.Key + offset] = tf.Value;
            }
        }

        private static Rectangle GetTileRect(Vector2 v)
        {
            return new Rectangle((int)v.X * openWorldTileSize, (int)v.Y * openWorldTileSize, openWorldTileSize, openWorldTileSize);
        }

        private static void DrawGameLocation(GameLocation loc, int positionX, int positionY)
        {
            Location offset = new Location(positionX, positionY);
            Vector2 offsetTile = new Vector2(positionX, positionY) / 64f;

            Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
            DrawLayer(loc.Map.GetLayer("Back"), Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
            var screen = new Rectangle(Game1.viewport.X / 64 - 1, Game1.viewport.Y / 64 - 1, Game1.viewport.Width / 64 + 3, Game1.viewport.Height / 64 + 7);
            foreach (var kvp in loc.terrainFeatures.Pairs)
            {
                if (screen.Contains(Utility.Vector2ToPoint(kvp.Key)) && (kvp.Value is Flooring || kvp.Value is HoeDirt))
                {
                    kvp.Value.draw(Game1.spriteBatch, kvp.Key);
                }
            }
            DrawLayer(loc.Map.GetLayer("Buildings"), Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
            var viewport = Game1.viewport;
            Game1.viewport = new xTile.Dimensions.Rectangle(viewport.X - offset.X, viewport.Y - offset.Y, viewport.Width, viewport.Height);
            loc.draw(Game1.spriteBatch);
            Game1.viewport = viewport;
            DrawLayer(loc.Map.GetLayer("Front"), Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
            Game1.mapDisplayDevice.EndScene();
            Game1.spriteBatch.End();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            foreach (var kvp in loc.terrainFeatures.Pairs)
            {
                if (screen.Contains(Utility.Vector2ToPoint(kvp.Key)) && kvp.Value is not Flooring && kvp.Value is not HoeDirt)
                {
                    kvp.Value.draw(Game1.spriteBatch, kvp.Key);
                }
            }
            var af = loc.Map.GetLayer("AlwaysFront");
            if (af != null)
            {
                DrawLayer(af, Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
            }
            Game1.mapDisplayDevice.EndScene();
            var currentLoc = Game1.currentLocation;
            Game1.currentLocation = loc;
            if (deltaTime is not null)
            {
                Game1.game1.drawWeather(deltaTime, Game1.game1.screen);
                Game1.updateWeather(deltaTime);
            }

            Game1.spriteBatch.End();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, AccessTools.FieldRefAccess<Game1, BlendState>(Game1.game1, "lightingBlend"), SamplerState.LinearClamp, null, null, null, null);
            Viewport vp = Game1.game1.GraphicsDevice.Viewport;
            vp.Bounds = ((Game1.game1.screen != null) ? Game1.game1.screen.Bounds : Game1.game1.GraphicsDevice.PresentationParameters.Bounds);
            Game1.game1.GraphicsDevice.Viewport = vp;
            float render_zoom = (float)(Game1.options.lightingQuality / 2);
            if (Game1.game1.useUnscaledLighting)
            {
                render_zoom /= Game1.options.zoomLevel;
            }
            //Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(Game1.lightmap.Bounds), Color.White, 0f, Vector2.Zero, render_zoom, SpriteEffects.None, 1f);
            if (Game1.IsRainingHere(null))
            {
                Game1.spriteBatch.Draw(Game1.staminaRect, vp.Bounds, Color.OrangeRed * 0.45f);
            }
            Game1.spriteBatch.End();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

            Game1.currentLocation = currentLoc;
        }

        private static void DrawLayer(Layer layer, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool v1, int pixelZoom)
        {
            int tileWidth = pixelZoom * 16;
            int tileHeight = pixelZoom * 16;
            Location tileInternalOffset = new Location(Wrap(mapViewport.X, tileWidth), Wrap(mapViewport.Y, tileHeight));
            int tileXMin = (mapViewport.X >= 0) ? (mapViewport.X / tileWidth) : ((mapViewport.X - tileWidth + 1) / tileWidth);
            int tileYMin = (mapViewport.Y >= 0) ? (mapViewport.Y / tileHeight) : ((mapViewport.Y - tileHeight + 1) / tileHeight);
            if (tileXMin < 0)
            {
                displayOffset.X -= tileXMin * tileWidth;
                tileXMin = 0;
            }
            if (tileYMin < 0)
            {
                displayOffset.Y -= tileYMin * tileHeight;
                tileYMin = 0;
            }
            int tileColumns = 1 + (mapViewport.Size.Width - 1) / tileWidth;
            int tileRows = 1 + (mapViewport.Size.Height - 1) / tileHeight;
            if (tileInternalOffset.X != 0)
            {
                tileColumns++;
            }
            if (tileInternalOffset.Y != 0)
            {
                tileRows++;
            }
            var screen = new Rectangle(mapViewport.X - displayOffset.X - 64 * pixelZoom, mapViewport.Y - displayOffset.Y - 64 * pixelZoom, (mapViewport.Width + 64) * pixelZoom, (mapViewport.Height + 64) * pixelZoom);
            Location tileLocation = displayOffset - tileInternalOffset;
            int offset = 0;
            tileLocation.Y = displayOffset.Y - tileInternalOffset.Y - tileYMin * 64;
            for (int tileY = 0; tileY < layer.LayerSize.Height; tileY++)
            {
                tileLocation.X = displayOffset.X - tileInternalOffset.X - tileXMin * 64;
                for (int tileX = 0; tileX < layer.LayerSize.Width; tileX++)
                {
                    if (screen.Contains(new Point(tileX * 64, tileY * 64)))
                    {
                        Tile tile = layer.Tiles[tileX, tileY];
                        if (tile != null)
                        {
                            displayDevice.DrawTile(tile, tileLocation, (tileY * (16 * pixelZoom) + 16 * pixelZoom + offset) / 10000f);
                        }
                    }
                    tileLocation.X += tileWidth;
                }
                tileLocation.Y += tileHeight;
            }
        }

        private static int Wrap(int value, int span)
        {
            value %= span;
            if (value < 0)
            {
                value += span;
            }
            return value;
        }
    }
}