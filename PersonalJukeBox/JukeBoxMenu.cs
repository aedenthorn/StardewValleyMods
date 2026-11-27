using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalJukeBox
{
    public class JukeBoxMenu : IClickableMenu
    {
        public static JukeBoxMenu instance;

        public SpriteFont font;
        public DropDown musicDropdown;

        public ClickableTextureComponent stopButton;
        public ClickableTextureComponent playButton;
        public ClickableTextureComponent prevButton;
        public ClickableTextureComponent nextButton;
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent addButton;
        public ClickableTextureComponent removeButton;
        public ClickableTextureComponent clearButton;
        public ClickableTextureComponent searchButton;

        public static Rectangle addRect = new Rectangle(310, 392, 16, 16);
        public static Rectangle addedRect = new Rectangle(294, 392, 16, 16);

        public List<string> tracks;
        public List<string> trackNames;
        
        public LocalizedContentManager loader;
        
        public bool held;
        public int dropDownWidth;
        public bool stopped;
        public bool added;
        public string hoverText;
        public bool searching;


        public JukeBoxMenu() : base(0, 0, 1, 1)
        {
            instance = this;
            
            
            tracks = ModEntry.GetTracks(false);
            trackNames = tracks.Select(t => Utility.getSongTitleFromCueName(t)).ToList();
            var max = trackNames.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
            dropDownWidth = (int)Game1.smallFont.MeasureString(max).X;
            RebuildElements();
            
        }

        public void RebuildElements()
        {
            prevButton = new ClickableTextureComponent("prev", new Rectangle(new Point(16, 6), new Point(52, 52)), "", "Prev", Game1.mouseCursors, new Rectangle(352, 493, 13, 13), 4);
            musicDropdown = new DropDown(tracks, trackNames, prevButton.bounds.Right + 16, 12, dropDownWidth + 96, 44, 2);
            stopButton = new ClickableTextureComponent("stop", new Rectangle(new Point(musicDropdown.bounds.Right, 22), new Point(28, 28)), "", "Stop", Game1.staminaRect, new Rectangle(0,0,1,1), 24);

            playButton = new ClickableTextureComponent("play", new Rectangle(new Point(musicDropdown.bounds.Right, 18), new Point(32, 32)), "", "Play", Game1.mouseCursors, new Rectangle(448, 96, 32, 32), 1);

            nextButton = new ClickableTextureComponent("next", new Rectangle(new Point(playButton.bounds.Right, 6), new Point(52, 52)), "", "Next", Game1.mouseCursors, new Rectangle(365, 493, 13, 13), 4);

            addButton = new ClickableTextureComponent("add", new Rectangle(new Point(nextButton.bounds.Right + 16, 16), new Point(32, 32)), "", "Add", Game1.mouseCursors, addRect, 2);

            clearButton = new ClickableTextureComponent("clear", new Rectangle(new Point(addButton.bounds.Right + 8, 16), new Point(36, 36)), "", "Clear", Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3);

            searchButton = new ClickableTextureComponent("search", new Rectangle(musicDropdown.bounds.Right - 84, 21, 26, 26), "", "Search", Game1.mouseCursors, new Rectangle(80, 0, 13, 13), 2);

            width = musicDropdown.bounds.Width + 272;
            height = 68;

            UpdateSongFlags();

        }


        public void UpdateSongFlags()
        {
            var song = ModEntry.PlayerSong;
            if (song == null && Game1.currentSong?.IsPlaying == true)
            {
                song = Game1.currentSong.Name;
            }
            musicDropdown.SetCurrentItem(song);
            stopped = ModEntry.PlayerSong == null;
            added = song != null && ModEntry.PlayerList.Contains(song);
        }

        public override void receiveKeyPress(Keys key)
        {

        }

        public override void draw(SpriteBatch b)
        {
            //b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0,0,0,0.5f));

            IClickableMenu.drawTextureBox(b, 0, 0, musicDropdown.bounds.Width + 272, 68, Color.White);
            if (!searching) 
            { 
                musicDropdown.draw(b, 0, 0);
            }

            if (!stopped)
            {
                stopButton.draw(b, Color.Brown, 1);
            }
            else
            {
                playButton.draw(b);
            }
            nextButton.draw(b);
            prevButton.draw(b);
            addButton.sourceRect = added ? addedRect : addRect;
            addButton.draw(b);
            clearButton.draw(b);
            if (!musicDropdown.clicked && !searching)
            {
                searchButton.draw(b);
            }
            if(hoverText != null)
            {
                drawHoverText(b, hoverText, Game1.smallFont);
            }
            drawMouse(b);
        }
        public override void performHoverAction(int x, int y)
        {
            hoverText = null;
            if (held)
                return;
            if(!stopped && stopButton.containsPoint(x, y))
            {
                hoverText = ModEntry.SHelper.Translation.Get("stop");
            }
            else if(stopped && playButton.containsPoint(x, y))
            {
                hoverText = ModEntry.SHelper.Translation.Get("play");
            }
            else if(addButton.containsPoint(x, y))
            {
                hoverText = ModEntry.SHelper.Translation.Get(added ? "remove" : "add");
            }
            else if(clearButton.containsPoint(x, y))
            {
                hoverText = ModEntry.SHelper.Translation.Get("clear");
            }
            else if(prevButton.containsPoint(x, y))
            {
                hoverText = ModEntry.SHelper.Translation.Get("prev");
            }
            else if(nextButton.containsPoint(x, y))
            {
                hoverText = ModEntry.SHelper.Translation.Get("next");
            }
            else if(searchButton.containsPoint(x, y) && !searching)
            {
                hoverText = ModEntry.SHelper.Translation.Get("search");
            }
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            RebuildElements();
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var song = ModEntry.PlayerSong;
            if (searchButton.bounds.Contains(x, y))
            {
                searching = true;
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = new SearchSongMenu(musicDropdown.bounds.X - 8, musicDropdown.bounds.Y, musicDropdown.bounds.Width - 40, height);
                return;
            }
            else if (song != null && stopButton.bounds.Contains(x, y))
            {
                Game1.playSound("shiny4");
                ChangeMusic(null);
                musicDropdown.SetCurrentItem(song);
            }
            else if (song == null && playButton.bounds.Contains(x, y))
            {
                Game1.playSound("shiny4");
                ModEntry.playingSong.Value = null;
                ModEntry.restarting.Value = true;
                ChangeMusic(musicDropdown.GetCurrentItem());
            }
            else if (prevButton.bounds.Contains(x, y))
            {
                if (song == null)
                {
                    song = tracks[tracks.Count - 1];
                }
                else
                {
                    var i = tracks.IndexOf(song) - 1;
                    song = tracks[i < 0 ? tracks.Count - 1 : i];
                }
                ChangeMusic(song);
                Game1.playSound("shiny4");
                return;
            }
            else if (nextButton.bounds.Contains(x, y))
            {
                if (song == null)
                {
                    song = tracks[1];
                }
                else
                {
                    song = tracks[(tracks.IndexOf(song) + 1) % tracks.Count];
                }
                ChangeMusic(song);
                Game1.playSound("shiny4");
                return;
            }
            else if (addButton.bounds.Contains(x, y))
            {
                added = ModEntry.ToggleFavorite(musicDropdown.GetCurrentItem());
                return;
            }
            else if (clearButton.bounds.Contains(x, y))
            {
                ModEntry.PlayerList = null;
                Game1.playSound("grassyStep");
                added = false;
                return;
            }
        }


        public void ChangeMusic(string song)
        {
            if (song != null)
            {
                musicDropdown.SetCurrentItem(song);
                
                if (ModEntry.PlayerSong != song)
                {
                    ModEntry.PlayerSong = song;
                }
                else
                {
                    UpdateSongFlags();
                    return;
                }
                if (Game1.currentSong?.Name == song && !ModEntry.restarting.Value)
                {
                    UpdateSongFlags();
                    return;
                }
                ModEntry.restarting.Value = false;
            }
            else
            {
                ModEntry.PlayerSong = null;
                Game1.currentSong?.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
            }
            UpdateSongFlags();
            AccessTools.StaticFieldRefAccess<Game1, bool>("requestedMusicDirty") = true;
            AccessTools.StaticFieldRefAccess<Game1, string>("requestedMusicTrack") = song;
            AccessTools.StaticFieldRefAccess<Game1, bool>("requestedMusicTrackOverrideable") = false;
        }

        public override void leftClickHeld(int x, int y)
        {
            if (searching)
                return;
            if (!held && isWithinBounds(x, y))
            {
                held = true;
                if (musicDropdown.bounds.Contains(x, y))
                {
                    musicDropdown.receiveLeftClick(x, y);
                    return;
                }

            }
            else
            {
                if (musicDropdown.clicked || musicDropdown.bounds.Contains(x, y))
                {
                    musicDropdown.leftClickHeld(x, y);
                    return;
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            held = false;
            if (musicDropdown.clicked)
            {
                musicDropdown.leftClickReleased(x, y);
                ChangeMusic(musicDropdown.GetCurrentItem());
            }
        }

        public override bool readyToClose()
        {
            return Game1.activeClickableMenu is not TitleMenu;
        }
    }
}