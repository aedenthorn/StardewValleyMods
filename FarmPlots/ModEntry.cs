using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmPlots
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string plotsKey = "aedenthorn.FarmPlots/plots";
        public static Dictionary<GameLocation, List<AutoPlot>> locationDict = new Dictionary<GameLocation, List<AutoPlot>>();

        public static PerScreen<Vector2> startTile = new PerScreen<Vector2>(() => new Vector2(-1,-1));
        public static PerScreen<AutoPlot> currentPlot = new PerScreen<AutoPlot>(() => null);

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            Helper.Events.Display.MenuChanged += Display_MenuChanged;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            
            //var harmony = new Harmony(ModManifest.UniqueID);
            //harmony.PatchAll();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsMainPlayer)
                return; 
            foreach(var l in Game1.locations)
            {
                if (TryGetAutoPlots(l, out var list))
                {
                    foreach(var p in list)
                    {
                        if (p.active[(int)Game1.season])
                        {
                            ActivatePlot(l, p);
                            foreach(var o in l.Objects.Values)
                            {
                                if (o.IsSprinkler() && (!l.IsOutdoors || !l.IsRainingHere()) && o.GetModifiedRadiusForSprinkler() >= 0)
                                {
                                    foreach (Vector2 v2 in o.GetSprinklerTiles())
                                    {
                                        o.ApplySprinkler(v2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if(e.NewMenu is not FarmPlotsMenu)
                currentPlot.Value = null;
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady || Game1.activeClickableMenu is not FarmPlotsMenu)
                return;
            if (startTile.Value.X >= 0 && (SHelper.Input.IsDown(Config.CreateKey) || SHelper.Input.IsDown(Config.DeleteKey)))
            {
                Rectangle rect = GetCurrentDragRect();
                for (int x = rect.X; x < rect.X + rect.Width; x++)
                {
                    for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        var tile = new Vector2(x, y);
                        e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)tile.X * 64), (float)((int)tile.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), SHelper.Input.IsDown(Config.CreateKey) ? Color.White : Color.Gray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    }
                }
            }
            if(TryGetAutoPlots(Game1.currentLocation, out var list))
            {
                foreach (var plot in list)
                {
                    Color color = currentPlot.Value == plot ? Color.White : Color.Gray;
                    float alpha = currentPlot.Value == plot ? 1f : (plot.tiles.Contains(Game1.currentCursorTile) ? 0.75f : 0.5f);
                    foreach (var tile in plot.tiles)
                    {
                        e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)tile.X * 64), (float)((int)tile.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), color * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    }
                }
            }
        }


        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Config.MenuKey.JustPressed())
            {
                if (Game1.activeClickableMenu == null)
                {
                    if(TryGetAutoPlot(out var plot, Game1.currentCursorTile))
                    {
                        currentPlot.Value = plot;
                    }
                    Game1.playSound("bigSelect");
                    Game1.activeClickableMenu = new FarmPlotsMenu();
                }
            }
            else if (Game1.activeClickableMenu is FarmPlotsMenu)
            {
                Vector2 tile = Game1.currentCursorTile;
                if (!Game1.activeClickableMenu.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
                {
                    if (e.Pressed.Contains(Config.CreateKey))
                    {
                        startTile.Value = tile;
                        return;
                    }
                    else if (e.Released.Contains(Config.CreateKey))
                    {
                        if (startTile.Value.X >= 0)
                        {
                            AddAutoPlot(GetCurrentDragRect(), startTile.Value);
                            if (TryGetAutoPlot(out var plot, startTile.Value))
                            {
                                currentPlot.Value = plot;
                                (Game1.activeClickableMenu as FarmPlotsMenu).RepopulateComponentList();
                            }
                            startTile.Value = new Vector2(-1, -1);
                            Game1.playSound("bigSelect");
                            return;
                        }
                    }
                    else if(e.Held.Contains(Config.CreateKey))
                    {
                        return;
                    }
                    else if (e.Pressed.Contains(Config.DeleteKey))
                    {
                        startTile.Value = tile;
                        return;
                    }
                    else if (e.Released.Contains(Config.DeleteKey))
                    {
                        if (startTile.Value.X >= 0)
                        {
                            RemoveAutoPlot(GetCurrentDragRect(), startTile.Value);
                            startTile.Value = new Vector2(-1, -1);
                            Game1.playSound("bigDeSelect");
                            return;
                        }
                    }
                    else if(e.Held.Contains(Config.DeleteKey))
                    {
                        return;
                    }
                }
                startTile.Value = new Vector2(-1, -1);
            }
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.EnableMod"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.MenuKey"),
                getValue: () => Config.MenuKey,
                setValue: value => Config.MenuKey= value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.CreateKey"),
                getValue: () => Config.CreateKey,
                setValue: value => Config.CreateKey = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.DeleteKey"),
                getValue: () => Config.DeleteKey,
                setValue: value => Config.DeleteKey = value
            );
        }
    }
}
