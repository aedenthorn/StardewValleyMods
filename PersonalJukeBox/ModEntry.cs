using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PersonalJukeBox
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string songKey = "aedenthorn.PersonalJukeBox/song";
        public const string listKey = "aedenthorn.PersonalJukeBox/list";
        public static PerScreen<string> playingSong = new();
        public static PerScreen<bool> clicked = new();
        public static PerScreen<bool> restarting = new();

        public static string PlayerSong
        {
            get
            {
                if (Game1.player?.modData.TryGetValue(songKey, out var str) != true)
                    return null;
                return str;
            }
            set
            {
                if (Game1.player is null)
                    return; 
                if(value is null)
                    Game1.player.modData.Remove(songKey);
                else
                {
                    Game1.player.modData[songKey] = value;
                }
            }
        }
        public static HashSet<string> PlayerList
        {
            get
            {
                return Game1.player?.modData.TryGetValue(listKey, out var str) == true ? str.Split(",").ToHashSet() : new();
            }
            set
            {
                if (Game1.player is null)
                    return; 
                if(value is null)
                    Game1.player.modData.Remove(listKey);
                else
                {
                    Game1.player.modData[listKey] = string.Join(',', value);
                }
            }
        }
        public static JukeBoxMenu Menu {  get; set; }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.Display.RenderedHud += Display_RenderedHud;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("aedenthorn.LauncherDrawer/dict"))
            {
                e.Edit((IAssetData data) =>
                {
                    data.AsDictionary<string, Dictionary<string, object>>().Data["aedenthorn.PersonalJukeBox"] = new()
                    {
                        { "Name", SHelper.Translation.Get("jukebox") },
                        { "Description", SHelper.Translation.Get("toggle-jukebox") },
                        { "Action", new Action(() => ToggleMenu()) }
                    };
                });
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || Menu == null)
                return;

        }

        private void Display_RenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Menu == null)
                return;
            if(!Config.ModEnabled || !Context.IsWorldReady)
            {
                Menu = null;
                return;
            }
            Menu.hoverText = null;
            var x = Game1.getMouseX(true);
            var y = Game1.getMouseY(true);
            if (Menu.isWithinBounds(x, y))
            {
                Menu.performHoverAction(x, y);
            }
            Menu.draw(e.SpriteBatch);
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.activeClickableMenu != null)
            {
                return;
            }
            if (Menu != null)
            {
                var down = SHelper.Input.IsDown(SButton.MouseLeft) || SHelper.Input.IsSuppressed(SButton.MouseLeft);
                if (down && clicked.Value)
                {
                    Menu.leftClickHeld(Game1.getMouseX(true), Game1.getMouseY(true));
                    SHelper.Input.Suppress(SButton.MouseLeft);
                }
                else if (!down && clicked.Value)
                {
                    clicked.Value = false;
                    Menu.releaseLeftClick(Game1.getMouseX(true), Game1.getMouseY(true));
                }
                else if (down && !clicked.Value && Menu.isWithinBounds(Game1.getMouseX(true), Game1.getMouseY(true)))
                {
                    clicked.Value = true;
                    Menu.receiveLeftClick(Game1.getMouseX(true), Game1.getMouseY(true));
                    SHelper.Input.Suppress(SButton.MouseLeft);
                }
            }
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if(!Config.ModEnabled || !Context.IsWorldReady || Game1.textEntry != null) 
                return;
            var track = Game1.currentSong;
            if (track?.IsStopped != false)
            {
                if (track is not null && playingSong.Value != track.Name)
                    return;
                var req = AccessTools.StaticFieldRefAccess<Game1, string>("requestedMusicTrack");
                var song = PlayerSong;
                if (song == null || (track != null && track.Name != req && song == req))
                {
                    return;
                }
                var tracks = GetTracks(true);

                var i = tracks.IndexOf(song) + 1;
                if (i >= tracks.Count)
                    i = 0;
                PlayerSong = tracks[i];
                OnSongChosen();
            }
            else
            {
                playingSong.Value = track.Name;
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            int i = 0;
            if (Config.FavoriteKey.JustPressed())
            {
                if (!string.IsNullOrEmpty(Game1.currentSong?.Name))
                {
                    ToggleFavorite(Game1.currentSong.Name);
                }
                Suppress(Config.FavoriteKey);

            }
            if (Config.ClearFavoriteKey.JustPressed())
            {
                PlayerList = null;
                Suppress(Config.FavoriteKey);

            }
            else if (Config.MenuKey.JustPressed())
            {
                ToggleMenu();
                Suppress(Config.MenuKey);
                return;
            }
            else if (Config.LastKey.JustPressed())
            {
                var tracks = GetTracks(true);

                i = -1;
                if (PlayerSong != null)
                {
                    i = tracks.IndexOf(PlayerSong) - 1;
                }
                if (i < 0)
                    i = tracks.Count - 1;
                PlayerSong = tracks[i];
                Suppress(Config.LastKey);

            }
            else if (Config.NextKey.JustPressed())
            {
                var tracks = GetTracks(true);

                i = PlayerSong != null ? tracks.IndexOf(PlayerSong) + 1 : 0;
                if (i >= tracks.Count)
                    i = 0;
                PlayerSong = tracks[i];
                Suppress(Config.NextKey);

            }
            else if (Config.RandomKey.JustPressed())
            {
                var tracks = GetTracks(true);
                if (PlayerList.Any())
                {
                    tracks.Shuffle();
                    PlayerList = tracks.ToHashSet();
                    PlayerSong = tracks[0];
                }
                else
                {
                    PlayerSong = Game1.random.ChooseFrom(tracks);
                }
                Suppress(Config.RandomKey);

            }
            else if (Config.StopKey.JustPressed())
            {
                
                if(PlayerSong != null)
                {
                    PlayerSong = null;

                    Suppress(Config.StopKey);

                }
                else
                {
                    return;
                }
            }
            else if (Config.PauseKey.JustPressed())
            {
                if (Game1.currentSong is null)
                    return;
                if(Game1.currentSong.Name == PlayerSong)
                {
                    if (Game1.currentSong.IsStopped)
                        Game1.currentSong.Play();
                    else if (Game1.currentSong.IsPaused)
                        Game1.currentSong.Resume();
                    else if (Game1.currentSong.IsPlaying)
                        Game1.currentSong.Pause();
                    Suppress(Config.PauseKey);
                }
                return;
            }
            else 
                return;
            if(Menu is not null)
            {
                Menu.RebuildElements();
            }
            OnSongChosen();
        }

        private void Suppress(KeybindList keylist)
        {
            List<SButton> buttons = new()
            {
                SButton.LeftShift,
                SButton.LeftControl,
                SButton.LeftAlt,
                SButton.RightShift,
                SButton.RightControl,
                SButton.RightAlt
            };
            foreach (var bind in keylist.Keybinds)
            {
                foreach (var key in bind.Buttons)
                {
                    if(!buttons.Contains(key))
                        SHelper.Input.Suppress(key);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.Name == nameof(Config.Debug))
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }

        public static string AddSpaces(string str)
        {
            string newStr = "";
            foreach (var c in str)
            {
                if (c >= 'A' && c <= 'Z' && newStr.Length > 0)
                {
                    newStr += " ";
                }
                newStr += c;
            }
            return newStr;
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