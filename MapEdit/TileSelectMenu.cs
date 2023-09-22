using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using xTile.Tiles;

namespace MapEdit
{
    internal class TileSelectMenu : IClickableMenu
    {
        public bool showing;
        public static ClickableTextureComponent button;
        public ClickableTextureComponent addSheetButton;
        public ClickableTextureComponent addThisSheetButton;
        public ClickableTextureComponent removeThisSheetButton;
        private int currentTileSheet;
        private Texture2D mapTex;
        private bool addingTileSheets;
        private int mouseDownTicks;
        private float mapZoom = 1;
        private Point mouseLastPos;
        private Point mouseDownPos;
        private Point mapDisplayOffsetPos = Point.Zero;
        private Rectangle mapCanvasRect = new Rectangle();
        private Rectangle mapDrawRect = new Rectangle();

        private List<TileSheet> availableSheets = new();

        public TileSelectMenu(): base()
        {
            RebuildElements();
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            RebuildElements();
        }

        private void RebuildElements()
        {
            button = new ClickableTextureComponent(new Rectangle(0, Game1.uiViewport.Height / 2 - 60, 44, 60), Game1.mouseCursors, new Rectangle(180, 379, 11, 15), 4);
            addSheetButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + 16 + (int)SpriteText.getWidthOfString("Tile Sheets"), yPositionOnScreen + spaceToClearTopBorder + 16, 28, 28), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 2);
            addThisSheetButton = new ClickableTextureComponent(new Rectangle(), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 2);
            removeThisSheetButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 16, yPositionOnScreen + spaceToClearTopBorder + 8 + 48, 14, 15), Game1.mouseCursors, new Rectangle(269, 471, 14, 15), 1);
            width = Game1.uiViewport.Width / 2;
            height = Game1.uiViewport.Height;
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (!showing)
            {
                button.bounds.Location = new Point(0, button.bounds.Location.Y);
                return;
            }
            if (ModEntry.currentLayer.Value is null)
            {
                ModEntry.currentLayer.Value = Game1.currentLocation.Map.Layers[0].Id;
            }
            Color selectColor = Color.White;
            button.bounds.Location = new Point(xPositionOnScreen + width - spaceToClearSideBorder, button.bounds.Location.Y);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            var mousePos = Game1.getMousePosition();

            mapCanvasRect = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3, yPositionOnScreen + spaceToClearTopBorder + 8, width * 2 / 3 - spaceToClearSideBorder * 4 - 8, height - spaceToClearTopBorder - spaceToClearSideBorder * 2 - 16);

            if (addingTileSheets)
            {
                SpriteText.drawString(spriteBatch, "Available Sheets", xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 8);
                for (int i = 0; i < availableSheets.Count; i++)
                {
                    var ts = availableSheets[i];
                    if (currentTileSheet == i)
                    {
                        spriteBatch.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 1) * 48, width / 3, 48), selectColor * 0.5f);
                        addThisSheetButton.draw(spriteBatch);
                        mapTex = ModEntry.SHelper.GameContent.Load<Texture2D>(ts.ImageSource);
                    }
                    string id = ts.Id;
                    if (Game1.dialogueFont.MeasureString(id + "...   ").X > width / 3)
                    {
                        while (Game1.dialogueFont.MeasureString(id + "...   ").X > width / 3)
                        {
                            id = id.Substring(0, id.Length - 1);
                        }
                        id += "...";
                    }

                    spriteBatch.DrawString(Game1.dialogueFont, id, new Vector2(xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 1) * 48), Color.SaddleBrown);
                }
            }
            else
            {
                SpriteText.drawString(spriteBatch, "Tile Sheets", xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 8);
                addSheetButton.draw(spriteBatch);
                for (int i = 0; i < Game1.currentLocation.Map.TileSheets.Count; i++)
                {
                    var ts = Game1.currentLocation.Map.TileSheets[i];
                    if (currentTileSheet == i)
                    {
                        spriteBatch.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 1) * 48, width / 3, 48), selectColor * 0.5f);
                        if(mapTex is null)
                        {
                            ReloadMap(ts.ImageSource);
                        }
                        if (ModEntry.mapCollectionData.mapDataDict.TryGetValue(Game1.currentLocation.Name, out var data) && data.customSheets.ContainsKey(ts.Id))
                        {
                            removeThisSheetButton.draw(spriteBatch);
                        }
                    }
                    spriteBatch.DrawString(Game1.dialogueFont, ts.Id, new Vector2(xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 1) * 48), Color.SaddleBrown);

                }
                SpriteText.drawString(spriteBatch, "Current Tile", xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + height / 2);
                var count = 0;
                foreach (var layer in Game1.currentLocation.Map.Layers)
                {
                    if (layer.Id == ModEntry.currentLayer.Value)
                    {
                        spriteBatch.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + height / 2 + (count + 1) * 64 - 8, width / 3, 64), selectColor * 0.5f);
                    }
                    spriteBatch.DrawString(Game1.dialogueFont, layer.Id, new Vector2(xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + height / 2 + (count + 1) * 64), Color.SaddleBrown);
                    if (ModEntry.currentTileDict.Value.TryGetValue(layer.Id, out var tile))
                    {
                        var idx = tile.TileIndex;
                        AccessTools.Field(typeof(Tile), "m_layer").SetValue(tile, Game1.currentLocation.Map.GetLayer(layer.Id)); 
                        Game1.mapDisplayDevice.DrawTile(tile, new xTile.Dimensions.Location(xPositionOnScreen + spaceToClearSideBorder * 2 + 16 + (int)Game1.dialogueFont.MeasureString(layer.Id).X, yPositionOnScreen + height / 2 - 8 + (count + 1) * 64), 1);
                    }
                    count++;
                }
            }
            if(mapTex is not null)
            {
                spriteBatch.DrawString(Game1.dialogueFont, mousePos.ToString(), new Vector2(2000, 900), Color.Wheat);
                spriteBatch.DrawString(Game1.dialogueFont, mapDisplayOffsetPos.ToString(), new Vector2(2000, 1000), Color.Wheat);
                int tilesToShow = (int)(mapCanvasRect.Width / mapZoom / 16);
                float squareSize = mapCanvasRect.Width / (float)tilesToShow;
                var xRemain = -mapDisplayOffsetPos.X % squareSize;
                var xAdjust = xRemain / squareSize < 0.5 ? (int)Math.Floor(xRemain) : -(int)Math.Ceiling(squareSize - xRemain); 
                var yRemain = -mapDisplayOffsetPos.Y % squareSize;
                var yAdjust = yRemain / squareSize < 0.5 ? (int)Math.Floor(yRemain) : -(int)Math.Ceiling(squareSize - yRemain);
                var mapDisplayOffsetPoint = new Point(mapDisplayOffsetPos.X + xAdjust, mapDisplayOffsetPos.Y + yAdjust);
                var offset = mousePos - mapCanvasRect.Location - mapDisplayOffsetPoint;
                int squareCeil = (int)Math.Ceiling(squareSize);
                int xTile = (int)(offset.X / mapZoom / mapTex.Width * (mapTex.Width / 16));
                int yTile = (int)(offset.Y / mapZoom / mapTex.Height * (mapTex.Height / 16));

                Rectangle pixelsToDraw = new Rectangle(-(int)(mapDisplayOffsetPoint.X / mapZoom), -(int)(mapDisplayOffsetPoint.Y / mapZoom), (int)Math.Min(mapCanvasRect.Width / mapZoom, mapTex.Width + mapDisplayOffsetPoint.X / mapZoom), (int)Math.Min(mapCanvasRect.Height / mapZoom, mapTex.Height + mapDisplayOffsetPoint.Y / mapZoom));
                mapDrawRect = new Rectangle(mapCanvasRect.Location, new Point((int)Math.Min(mapCanvasRect.Width, mapTex.Width * mapZoom + mapDisplayOffsetPoint.X), (int)Math.Min(mapCanvasRect.Height, mapTex.Height * mapZoom + mapDisplayOffsetPoint.Y)));
                spriteBatch.Draw(mapTex, mapDrawRect, pixelsToDraw, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
                if (!addingTileSheets)
                {
                    if (mapDrawRect.Contains(mousePos))
                    {
                        var rect = new Rectangle(mapCanvasRect.X + mapDisplayOffsetPoint.X + offset.X - (int)Math.Round(offset.X % squareSize), mapCanvasRect.Y + mapDisplayOffsetPoint.Y + offset.Y - (int)Math.Round(offset.Y % squareSize), squareCeil, squareCeil);
                        spriteBatch.Draw(ModEntry.activeTexture, rect, Color.White);
                    }
                }
                if (mouseDownTicks > 0)
                {
                    if (!ModEntry.SHelper.Input.IsSuppressed(StardewModdingAPI.SButton.MouseLeft))
                    {
                        if (Game1.ticks - mouseDownTicks < 20 && mousePos == mouseDownPos)
                        {

                            if (mapCanvasRect.Contains(mouseLastPos))
                            {
                                var layer = Game1.currentLocation.Map.GetLayer(ModEntry.currentLayer.Value);
                                if (layer is null)
                                    return;
                                Game1.playSound(ModEntry.Config.CopySound);

                                int index = xTile + yTile * mapTex.Width / 16;
                                var tile = new StaticTile(layer, Game1.currentLocation.Map.TileSheets[currentTileSheet], BlendMode.Alpha, index);
                                if((ModEntry.SHelper.Input.IsDown(ModEntry.Config.LayerModButton) || ModEntry.SHelper.Input.IsSuppressed(ModEntry.Config.LayerModButton)) && ModEntry.currentTileDict.Value.TryGetValue(ModEntry.currentLayer.Value, out var oldTile))
                                {
                                    AnimatedTile animatedTile;
                                    if(oldTile is AnimatedTile)
                                    {
                                        List<StaticTile> list = (oldTile as AnimatedTile).TileFrames.ToList();
                                        list.Add(tile);
                                        AccessTools.Field(typeof(AnimatedTile), "m_tileFrames").SetValue(oldTile, list.ToArray());
                                        animatedTile = oldTile as AnimatedTile;
                                    }
                                    else
                                    {
                                        animatedTile = new AnimatedTile(layer, new StaticTile[] { tile }, ModEntry.Config.DefaultAnimationInterval);
                                    }
                                    ModEntry.currentTileDict.Value[ModEntry.currentLayer.Value] = animatedTile;
                                }
                                else
                                {
                                    ModEntry.currentTileDict.Value[ModEntry.currentLayer.Value] = tile;
                                }
                                mouseDownTicks = 0;
                                return;
                            }
                        }
                        mouseDownTicks = 0;
                    }
                    else
                    {
                        mapDisplayOffsetPos += mousePos - mouseLastPos;
                        mapDisplayOffsetPos = new Point((int)MathHelper.Clamp(mapDisplayOffsetPos.X, Math.Min(0, mapCanvasRect.Width - mapTex.Width * mapZoom), 0), (int)MathHelper.Clamp(mapDisplayOffsetPos.Y, Math.Min(0, mapCanvasRect.Height -mapTex.Height * mapZoom), 0));
                        mouseLastPos = mousePos;
                    }
                }
            }
            drawMouse(spriteBatch, true);
        }

        private void ReloadMap(string imageSource)
        {
            mapTex = ModEntry.SHelper.GameContent.Load<Texture2D>(imageSource);
            mapZoom = mapCanvasRect.Width / (float)mapTex.Width;
            mapDisplayOffsetPos = Point.Zero;
        }

        public override void receiveKeyPress(Keys key)
        {
            if(!addingTileSheets && key == (Keys)ModEntry.Config.RevertButton)
            {
                var y = Game1.getMouseY();
                for (int i = 0; i < Game1.currentLocation.Map.Layers.Count; i++)
                {
                    if (y > yPositionOnScreen + height / 2 + (i + 1) * 64 - 8 && y < yPositionOnScreen + height / 2 + (i + 2) * 64 - 8)
                    {
                        Game1.playSound("Ship");
                        ModEntry.currentTileDict.Value.Remove(Game1.currentLocation.Map.Layers[i].Id);
                        return;
                    }
                }
            }
        }
        public override void receiveScrollWheelAction(int direction)
        {
            if (!showing || mapTex is null)
                return;
            if (mapCanvasRect.Contains(Game1.getMousePosition())) 
            {
                Game1.playSound("shiny4");
                int tilesToShow = (int)(mapCanvasRect.Width / (16 * mapZoom));
                tilesToShow += direction > 0 ? -1 : 1;
                tilesToShow = MathHelper.Clamp(tilesToShow, 4, mapTex.Width / 16);
                mapZoom = (float)mapCanvasRect.Width / (16 * tilesToShow);
                mapDisplayOffsetPos = new Point((int)MathHelper.Clamp(mapDisplayOffsetPos.X, Math.Min(0, mapCanvasRect.Width - mapTex.Width * mapZoom), 0), (int)MathHelper.Clamp(mapDisplayOffsetPos.Y, Math.Min(0, mapCanvasRect.Height - mapTex.Height * mapZoom), 0));
            }
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (button.containsPoint(x, y))
            {
                if(!showing)
                {
                    Game1.playSound("bigSelect");
                    showing = true;
                }
                else
                {
                    Game1.playSound("bigDeSelect");
                    showing = false;
                    addingTileSheets = false;
                    mapDisplayOffsetPos = Point.Zero;
                    mapTex = null;
                    currentTileSheet = 0;
                }
                return;
            }
            if (!showing)
                return;
            if(x < xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3)
            {
                if(addingTileSheets)
                {
                    if (addThisSheetButton.containsPoint(x, y))
                    {
                        Game1.playSound("bigSelect");
                        ModEntry.AddTilesheet(availableSheets[currentTileSheet], Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", ""));
                        addingTileSheets = false;
                        currentTileSheet = 0;
                        mapTex = null;
                        return;
                    }
                    for (int i = 0; i < availableSheets.Count; i++)
                    {
                        if (y > yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 1) * 48 && y < yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 2) * 48)
                        {
                            Game1.playSound("Ship");

                            currentTileSheet = i;
                            ReloadMap(availableSheets[currentTileSheet].ImageSource);
                            addThisSheetButton.bounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 32, yPositionOnScreen + spaceToClearTopBorder + 20 + (i + 1) * 48, 28, 28);
                            return;
                        }
                    }
                }
                else
                {
                    if(addSheetButton.containsPoint(x, y))
                    {
                        Game1.playSound("bigSelect");
                        addingTileSheets = true;
                        currentTileSheet = 0;
                        BuildAdditionalTilesheets();
                        return;
                    }
                    for (int i = 0; i < Game1.currentLocation.Map.TileSheets.Count; i++)
                    {
                        if(currentTileSheet == i)
                        {

                            if (removeThisSheetButton.containsPoint(x, y) && ModEntry.mapCollectionData.mapDataDict.TryGetValue(Game1.currentLocation.Name, out var data) && data.customSheets.ContainsKey(Game1.currentLocation.Map.TileSheets[i].Id))
                            {
                                ModEntry.RemoveTilesheet(Game1.currentLocation.Map.TileSheets[i].Id, Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", ""));
                            }
                            continue;
                        }
                        if (y > yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 1) * 48 && y < yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 2) * 48)
                        {
                            Game1.playSound("Ship");
                            currentTileSheet = i;
                            ReloadMap(Game1.currentLocation.Map.TileSheets[i].ImageSource);
                            removeThisSheetButton.bounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 16, yPositionOnScreen + spaceToClearTopBorder + 8 + (i + 1) * 48, 28, 28);
                            return;
                        }
                    }
                    for (int i = 0; i < Game1.currentLocation.Map.Layers.Count; i++)
                    {
                        if (y > yPositionOnScreen + height / 2 + (i + 1) * 64 - 8 && y < yPositionOnScreen + height / 2 + (i + 2) * 64 - 8)
                        {
                            Game1.playSound("Ship");
                            ModEntry.currentLayer.Value = Game1.currentLocation.Map.Layers[i].Id;
                            return;
                        }
                    }
                }
            }
            else
            {
                mouseLastPos = Game1.getMousePosition();
                mouseDownPos = mouseLastPos;
                mouseDownTicks = Game1.ticks;
            }
            base.receiveLeftClick(x, y, playSound);
        }


        public void BuildAdditionalTilesheets()
        {
            availableSheets.Clear();
            currentTileSheet = 0;
            foreach (var l in Game1.locations)
            {
                AddSheets(l.Map.TileSheets);
                if (l is BuildableGameLocation)
                {
                    foreach(var b in (l as BuildableGameLocation).buildings)
                    {
                        if(b.indoors.Value is not null)
                        {
                            AddSheets(b.indoors.Value.Map.TileSheets);
                        }
                    }
                }
            }
            if(availableSheets.Count == 0)
            {
                addingTileSheets = false;
                return;
            }
            addThisSheetButton.bounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 32, yPositionOnScreen + spaceToClearTopBorder + 20 + 48, 28, 28);
            ReloadMap(availableSheets[0].ImageSource);
        }

        private void AddSheets(ReadOnlyCollection<TileSheet> tileSheets)
        {
            foreach(var sheet in tileSheets) 
            {
                if(availableSheets.FirstOrDefault(s => s.ImageSource == sheet.ImageSource) == null)
                    availableSheets.Add(sheet);
            }
        }
    }
}