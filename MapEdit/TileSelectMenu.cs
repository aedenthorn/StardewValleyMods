using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
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
        public static ClickableTextureComponent button;
        public ClickableTextureComponent addSheetButton;
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent addThisSheetButton;
        public ClickableTextureComponent removeThisSheetButton;
        public List<ClickableComponent> layerCCList = new();
        public List<ClickableComponent> sheetCCList = new();
        private int currentTileSheet;
        private Texture2D mapTex;
        private bool addingTileSheets;
        private int mouseDownTicks;
        private float mapZoom = 1;
        private int scrolledAdd;
        private int scrolledSheets;
        private int scrolledLayers;
        private int linesPerPageAdd;
        private int linesPerPageLayers;
        private int linesPerPageSheets;
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

        public void RebuildElements()
        {
            width = Game1.uiViewport.Width / 2;
            height = Game1.uiViewport.Height;
            linesPerPageSheets = (height - 128 - spaceToClearTopBorder  - spaceToClearSideBorder) / 2 / 48 - 1;
            linesPerPageLayers = (height - 128 - spaceToClearTopBorder  - spaceToClearSideBorder) / 2 / 64;
            linesPerPageAdd = (height - 64 - spaceToClearTopBorder  - spaceToClearSideBorder) / 48;
            button = new ClickableTextureComponent(new Rectangle(0, Game1.uiViewport.Height / 2 - 60, 44, 60), Game1.mouseCursors, new Rectangle(180, 379, 11, 15), 4);
            backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + 16 + width / 3 - 64, yPositionOnScreen + spaceToClearTopBorder + 12, 28, 28), Game1.mouseCursors, new Rectangle(8, 269, 44, 40), 1);
            addSheetButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + 16 + width / 3 - 56, yPositionOnScreen + spaceToClearTopBorder + 16, 28, 28), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 2);
            addThisSheetButton = new ClickableTextureComponent(new Rectangle(), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 2);
            removeThisSheetButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 16, yPositionOnScreen + spaceToClearTopBorder + 8 + 48, 28, 28), Game1.mouseCursors, new Rectangle(269, 471, 14, 15), 1);

            mapCanvasRect = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3, yPositionOnScreen + spaceToClearTopBorder + 8, width * 2 / 3 - spaceToClearSideBorder * 4 - 8, height - spaceToClearTopBorder - spaceToClearSideBorder * 2 - 16);
            if(mapTex is not null)
            {
                mapZoom = mapCanvasRect.Width / (float)mapTex.Width;
            }
            else
            {
                mapZoom = 1;
            }
            mapDisplayOffsetPos = Point.Zero;
            RebuildLists();
        }

        private void RebuildLists()
        {
            layerCCList.Clear();
            sheetCCList.Clear();
            if(addingTileSheets)
            {
                var count = 0;
                for (int i = scrolledAdd; i < Math.Min(scrolledAdd + linesPerPageAdd, availableSheets.Count); i++)
                {
                    sheetCCList.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + spaceToClearTopBorder + 8 + (count + 1) * 48, width / 3, 48), i.ToString()));
                    count++;
                }
            }
            else
            {
                var count = 0;
                for (int i = scrolledSheets; i < Math.Min(scrolledSheets + linesPerPageSheets, Game1.currentLocation.Map.TileSheets.Count); i++)
                {
                    sheetCCList.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + spaceToClearTopBorder + 8 + (count + 1) * 48, width / 3, 48), i.ToString()));
                    count++;
                }
                count = 0;
                for (int i = scrolledLayers; i < Math.Min(scrolledLayers + linesPerPageLayers, Game1.currentLocation.Map.Layers.Count); i++)
                {
                    if (Game1.currentLocation.Map.Layers[i].Id == "Paths")
                        continue;
                    layerCCList.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + height / 2 + (count + 1) * 64 - 8, width / 3, 64), Game1.currentLocation.Map.Layers[i].Id));
                    count++;
                }
            }
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (!ModEntry.Config.ShowMenu)
            {
                button.bounds.Location = new Point(0, button.bounds.Location.Y);
                button.draw(spriteBatch, Color.White, 1);
                if (ModEntry.MouseInMenu())
                    drawMouse(spriteBatch, true);
                return;
            }
            button.draw(spriteBatch, Color.White, 1);
            if (ModEntry.currentLayer.Value is null)
            {
                ModEntry.currentLayer.Value = Game1.currentLocation.Map.Layers[0].Id;
            }
            Color selectColor = Color.White;
            button.bounds.Location = new Point(xPositionOnScreen + width - spaceToClearSideBorder, button.bounds.Location.Y);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            var mousePos = Game1.getMousePosition(true);

            mapCanvasRect = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3, yPositionOnScreen + spaceToClearTopBorder + 8, width * 2 / 3 - spaceToClearSideBorder * 4 - 8, height - spaceToClearTopBorder - spaceToClearSideBorder * 2 - 16);

            if (addingTileSheets)
            {
                SpriteText.drawString(spriteBatch, ModEntry.SHelper.Translation.Get("available-sheets"), xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 8);
                backButton.draw(spriteBatch);
                var count = 0;
                for (int i = scrolledAdd; i < Math.Min(scrolledAdd + linesPerPageAdd, availableSheets.Count); i++)
                {
                    var ts = availableSheets[i];
                    if (currentTileSheet == i)
                    {
                        spriteBatch.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + spaceToClearTopBorder + 8 + (count + 1) * 48, width / 3, 48), selectColor * 0.5f);
                        addThisSheetButton.draw(spriteBatch);
                        mapTex = ModEntry.SHelper.GameContent.Load<Texture2D>(ts.ImageSource);
                    }
                    string id = ts.Id;
                    if (Game1.smallFont.MeasureString(id + "...   ").X > width / 3)
                    {
                        while (Game1.smallFont.MeasureString(id + "...   ").X > width / 3)
                        {
                            id = id.Substring(0, id.Length - 1);
                        }
                        id += "...";
                    }

                    spriteBatch.DrawString(Game1.smallFont, id, new Vector2(xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 16 + (count + 1) * 48), Color.SaddleBrown);
                    count++;
                }
            }
            else
            {
                SpriteText.drawString(spriteBatch, ModEntry.SHelper.Translation.Get("tile-sheets"), xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 8);
                addSheetButton.draw(spriteBatch);
                var count = 0;
                for (int i = scrolledSheets; i < Math.Min(scrolledSheets + linesPerPageSheets, Game1.currentLocation.Map.TileSheets.Count); i++)
                {
                    var ts = Game1.currentLocation.Map.TileSheets[i];
                    if (currentTileSheet == i)
                    {
                        spriteBatch.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + spaceToClearTopBorder + 8 + (count + 1) * 48, width / 3, 48), selectColor * 0.5f);
                        if(mapTex is null)
                        {
                            ReloadMap(ts.ImageSource);
                        }
                        if (ModEntry.mapCollectionData.mapDataDict.TryGetValue(Game1.currentLocation.mapPath.Value.Replace("Maps\\", ""), out var data) && data.customSheets.ContainsKey(ts.Id))
                        {
                            removeThisSheetButton.draw(spriteBatch);
                        }
                    }
                    spriteBatch.DrawString(Game1.dialogueFont, ts.Id, new Vector2(xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + spaceToClearTopBorder + 8 + (count + 1) * 48), Color.SaddleBrown);
                    count++;
                }
                SpriteText.drawString(spriteBatch, ModEntry.SHelper.Translation.Get("current-tile"), xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + height / 2);
                count = 0;
                for (int i = scrolledLayers; i < Math.Min(scrolledLayers + linesPerPageLayers, Game1.currentLocation.Map.Layers.Count); i++)
                {
                    var id = Game1.currentLocation.Map.Layers[i].Id;
                    if (id == "Paths")
                        continue;
                    if (id == ModEntry.currentLayer.Value)
                    {
                        spriteBatch.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2, yPositionOnScreen + height / 2 + (count + 1) * 64 - 8, width / 3, 64), selectColor * 0.5f);
                    }
                    spriteBatch.DrawString(Game1.dialogueFont, id, new Vector2(xPositionOnScreen + spaceToClearSideBorder * 2 + 8, yPositionOnScreen + height / 2 + (count + 1) * 64), Color.SaddleBrown);
                    if (ModEntry.currentTileDict.Value.TryGetValue(id, out var tile))
                    {
                        var idx = tile.TileIndex;
                        AccessTools.Field(typeof(Tile), "m_layer").SetValue(tile, Game1.currentLocation.Map.GetLayer(id)); 
                        Game1.mapDisplayDevice.DrawTile(tile, new xTile.Dimensions.Location(xPositionOnScreen + spaceToClearSideBorder * 2 + 16 + (int)Game1.dialogueFont.MeasureString(id).X, yPositionOnScreen + height / 2 - 8 + (count + 1) * 64), 1);
                    }
                    count++;
                }
            }
            if(mapTex is not null)
            {
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

                //spriteBatch.DrawString(Game1.dialogueFont, $"{yRemain} {yAdjust} {squareSize}", new Vector2(2000, 900), Color.White);
                //spriteBatch.DrawString(Game1.dialogueFont, mapDisplayOffsetPos.ToString(), new Vector2(2000, 1000), Color.White);


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
                        var xMax = Math.Min(0, mapCanvasRect.Width - mapTex.Width * mapZoom);
                        var xR = xMax % squareSize;
                        if(xR != 0)
                        {
                            xMax -= squareSize + xR;
                        }
                        var yMax = Math.Min(0, mapCanvasRect.Height - mapTex.Height * mapZoom);
                        var yR = yMax % squareSize;
                        if (yR != 0)
                        {
                            yMax -= squareSize + yR;
                        }
                        mapDisplayOffsetPos = new Point((int)Math.Round(MathHelper.Clamp(mapDisplayOffsetPos.X, xMax, 0)), (int)Math.Round(MathHelper.Clamp(mapDisplayOffsetPos.Y, yMax, 0)));
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
                for (int i = 0; i < layerCCList.Count; i++)
                {
                    if (layerCCList[i].containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                    {
                        Game1.playSound("drumkit6");
                        ModEntry.currentTileDict.Value.Remove(layerCCList[i].name);
                        return;
                    }
                }
            }
        }
        public override void receiveScrollWheelAction(int direction)
        {
            if (!ModEntry.Config.ShowMenu)
                return;
            var mousePos = Game1.getMousePosition(true);
            if (mousePos.X < width / 3)
            {
                if (addingTileSheets)
                {
                    var oldScrolled = scrolledAdd;
                    scrolledAdd = Math.Clamp(scrolledAdd - direction, 0, Math.Max(0, availableSheets.Count - linesPerPageAdd));
                    if (oldScrolled != scrolledAdd)
                    {
                        Game1.playSound("shiny4");
                        addThisSheetButton.bounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 32, yPositionOnScreen + spaceToClearTopBorder + 20 + (currentTileSheet - scrolledAdd + 1) * 48, 28, 28);
                        RebuildLists();
                    }
                }
                else
                {
                    if(mousePos.Y >= yPositionOnScreen + height / 2)
                    {
                        var oldScrolled = scrolledLayers;
                        scrolledLayers = Math.Clamp(scrolledLayers - direction, 0, Math.Max(0, Game1.currentLocation.Map.Layers.Count - linesPerPageLayers));
                        if (oldScrolled != scrolledLayers)
                        {
                            Game1.playSound("shiny4");
                            RebuildLists();
                        }
                    }
                    else
                    {

                        var oldScrolled = scrolledSheets;
                        scrolledSheets = Math.Clamp(scrolledSheets - direction, 0, Math.Max(0, Game1.currentLocation.Map.TileSheets.Count - linesPerPageSheets));
                        if (oldScrolled != scrolledSheets)
                        {
                            Game1.playSound("shiny4");
                            removeThisSheetButton.bounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 16, yPositionOnScreen + spaceToClearTopBorder + 8 + (currentTileSheet - scrolledSheets + 1) * 48, 28, 28);
                            RebuildLists();
                        }
                    }
                }
            }
            else if (mapTex is not null && mapCanvasRect.Contains(mousePos)) 
            {
                AdjustZoom(direction);
            }
        }

        private void AdjustZoom(int v)
        {
            var oldZoom = mapZoom;
            int tilesToShow = (int)Math.Round(mapCanvasRect.Width / (16 * mapZoom));
            tilesToShow += v;
            tilesToShow = MathHelper.Clamp(tilesToShow, 1, mapTex.Width / 16);
            mapZoom = (float)mapCanvasRect.Width / (16 * tilesToShow);
            if(oldZoom != mapZoom)
            {
                Game1.playSound("shiny4");
            }
            mapDisplayOffsetPos = new Point((int)MathHelper.Clamp(mapDisplayOffsetPos.X, Math.Min(0, mapCanvasRect.Width - mapTex.Width * mapZoom), 0), (int)MathHelper.Clamp(mapDisplayOffsetPos.Y, Math.Min(0, mapCanvasRect.Height - mapTex.Height * mapZoom), 0));
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (button.containsPoint(x, y))
            {
                if(!ModEntry.Config.ShowMenu)
                {
                    Game1.playSound("bigSelect");
                    ModEntry.Config.ShowMenu = true;
                }
                else
                {
                    Game1.playSound("bigDeSelect");
                    ModEntry.Config.ShowMenu = false;
                    addingTileSheets = false;
                    mapDisplayOffsetPos = Point.Zero;
                    mapTex = null;
                    currentTileSheet = 0;
                }
                ModEntry.SHelper.WriteConfig(ModEntry.Config);
                return;
            }
            if (!ModEntry.Config.ShowMenu)
                return;
            if(x < xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3)
            {
                if(addingTileSheets)
                {
                    if(backButton.containsPoint(x, y)) 
                    {
                        Game1.playSound("bigDeSelect");
                        addingTileSheets = false;
                        currentTileSheet = 0;
                        mapTex = null;
                        RebuildLists();
                        return;
                    }
                    if (addThisSheetButton.containsPoint(x, y))
                    {
                        Game1.playSound("bigSelect");
                        ModEntry.AddTilesheet(availableSheets[currentTileSheet], Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", ""));
                        addingTileSheets = false;
                        currentTileSheet = 0;
                        mapTex = null;
                        RebuildLists();
                        return;
                    }
                    int count = 0;
                    for (int i = 0; i < sheetCCList.Count; i++)
                    {
                        if (sheetCCList[i].containsPoint(x, y))
                        {
                            Game1.playSound("drumkit6");
                            SetTileSheet(int.Parse(sheetCCList[i].name), i);
                            return;
                        }
                        count++;
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
                        RebuildLists();
                        return;
                    }
                    for (int i = 0; i < sheetCCList.Count; i++)
                    {
                        int idx = i + scrolledSheets;
                        if (currentTileSheet == i + scrolledSheets)
                        {

                            if (removeThisSheetButton.containsPoint(x, y) && ModEntry.mapCollectionData.mapDataDict.TryGetValue(Game1.currentLocation.mapPath.Value.Replace("Maps\\", ""), out var data) && data.customSheets.ContainsKey(Game1.currentLocation.Map.TileSheets[idx].Id))
                            {
                                ModEntry.RemoveTilesheet(Game1.currentLocation.Map.TileSheets[idx].Id, Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", ""));
                                SetTileSheet(0, 0);
                                return;
                            }
                            continue;
                        }

                        if (sheetCCList[i].containsPoint(x, y))
                        {
                            Game1.playSound("drumkit6");
                            SetTileSheet(int.Parse(sheetCCList[i].name), i);
                            return;
                        }
                    }
                    for(int i = 0; i < layerCCList.Count; i++)
                    {
                        if (layerCCList[i].name == "Paths")
                            continue;
                        if (layerCCList[i].containsPoint(x, y))
                        {
                            Game1.playSound("drumkit6");
                            ModEntry.currentLayer.Value = layerCCList[i].name;
                            return;
                        }
                    }
                }
            }
            else
            {
                mouseLastPos = Game1.getMousePosition(true);
                mouseDownPos = mouseLastPos;
                mouseDownTicks = Game1.ticks;
            }
            base.receiveLeftClick(x, y, playSound);
        }

        private void SetTileSheet(int which, int pos)
        {
            currentTileSheet = which;
            if (addingTileSheets)
            {
                addThisSheetButton.bounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 32, yPositionOnScreen + spaceToClearTopBorder + 20 + (pos + 1) * 48, 28, 28);
                ReloadMap(availableSheets[currentTileSheet].ImageSource);
            }
            else
            {
                removeThisSheetButton.bounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 2 + width / 3 - 16, yPositionOnScreen + spaceToClearTopBorder + 8 + (pos + 1) * 48, 28, 28);
                ReloadMap(Game1.currentLocation.Map.TileSheets[which].ImageSource);
            }
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
                if(availableSheets.FirstOrDefault(s => s.ImageSource == sheet.ImageSource) == null && Game1.currentLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == sheet.ImageSource) == null)
                    availableSheets.Add(sheet);
            }
        }
    }
}