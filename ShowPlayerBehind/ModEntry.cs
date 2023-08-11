using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace ShowPlayerBehind
{
    public partial class ModEntry : Mod
    {
        
        public static ModConfig Config;
        public static IMonitor SMonitor;

        private Dictionary<long, Point[]> transparentPoints = new Dictionary<long, Point[]>();
        private Dictionary<long, Point> farmerPoints = new Dictionary<long, Point>();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            SMonitor = Monitor;

            Helper.Events.Display.RenderingWorld += Display_RenderingWorld;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Inner Transparency",
                getValue: () => Config.InnerTransparency + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { Config.InnerTransparency = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Outer Transparency",
                getValue: () => Config.OuterTransparency + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { Config.OuterTransparency = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Corner Transparency",
                getValue: () => Config.CornerTransparency + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { Config.CornerTransparency = f; } }
            );
        }
        private void Display_RenderingWorld(object sender, StardewModdingAPI.Events.RenderingWorldEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            foreach(Farmer f in Game1.getAllFarmers())
            {
                Point fp = f.getTileLocationPoint();
                if (farmerPoints.ContainsKey(f.UniqueMultiplayerID))
                {
                    if (farmerPoints[f.UniqueMultiplayerID] == fp)
                        continue;
                    farmerPoints[f.UniqueMultiplayerID] = fp;
                }
                else
                {
                    farmerPoints.Add(f.UniqueMultiplayerID, fp);
                }

                GameLocation gl = f.currentLocation;

                if (gl == null)
                    continue;

                Point[] outpoints = AddFarmerPoints(f);

                Tile tile1 = null;
                Tile tile2 = null;
                Tile tile3 = null;
                Tile tile4 = null;

                if (gl.map.GetLayer("Front") != null)
                {
                    tile1 = gl.map.GetLayer("Front").PickTile(new Location(fp.X, fp.Y) * Game1.tileSize, Game1.viewport.Size);
                    tile3 = gl.map.GetLayer("Front").PickTile(new Location(fp.X, fp.Y - 1) * Game1.tileSize, Game1.viewport.Size);
                }
                if (gl.map.GetLayer("AlwaysFront") != null)
                {
                    tile2 = gl.map.GetLayer("AlwaysFront").PickTile(new Location(fp.X, fp.Y) * Game1.tileSize, Game1.viewport.Size);
                    tile4 = gl.map.GetLayer("AlwaysFront").PickTile(new Location(fp.X, fp.Y - 1) * Game1.tileSize, Game1.viewport.Size);
                }


                bool fullyCovered = (tile1 != null || tile2 != null) && (tile3 != null || tile4 != null);

                foreach (string layer in new string[] { "Front", "AlwaysFront" })
                {
                    if (gl.map.GetLayer(layer) == null)
                        continue;

                    foreach(Point p in outpoints)
                    {
                        Tile tile = gl.map.GetLayer(layer).PickTile(new Location(p.X, p.Y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile != null)
                        {
                            tile.Properties.TryGetValue("@Opacity", out PropertyValue property);
                            if (property != null)
                            {
                                gl.map.GetLayer(layer).Tiles[p.X, p.Y].Properties.Remove("@Opacity");
                            }
                        }
                    }

                    if (!fullyCovered)
                        continue;

                    foreach (Point p in transparentPoints[f.UniqueMultiplayerID])
                    {
                        Tile tile = gl.map.GetLayer(layer).PickTile(new Location(p.X, p.Y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile != null)
                        {
                            float opacity = Config.OuterTransparency;
                            if (p == fp || p == new Point(fp.X, fp.Y - 1))
                            {
                                opacity = Config.InnerTransparency;
                            }
                            else if (layer == "Front" && p.Y == fp.Y - 2)
                            {
                                continue;
                            }
                            else if(p.X != fp.X && (p.Y == fp.Y - 2 || p.Y == fp.Y + 1))
                            {
                                opacity = Config.CornerTransparency;
                            }
                            if (tile.Properties.ContainsKey("@Opacity"))
                            {
                                gl.map.GetLayer(layer).Tiles[p.X, p.Y].Properties["@Opacity"] = opacity + "";
                            }
                            else
                            {
                                gl.map.GetLayer(layer).Tiles[p.X, p.Y].Properties.Add("@Opacity", opacity + "");
                            }
                        }
                    }
                }

            }
        }

        private Point[] AddFarmerPoints(Farmer f)
        {
            Point p = f.getTileLocationPoint();
            Point[] points = new Point[]
            {
                new Point(p.X, p.Y),
                new Point(p.X - 1, p.Y - 2),
                new Point(p.X, p.Y - 2),
                new Point(p.X + 1, p.Y - 2),
                new Point(p.X - 1, p.Y - 1),
                new Point(p.X, p.Y - 1),
                new Point(p.X + 1, p.Y - 1),
                new Point(p.X + 1, p.Y),
                new Point(p.X + 1, p.Y + 1),
                new Point(p.X, p.Y + 1),
                new Point(p.X - 1, p.Y + 1),
                new Point(p.X - 1, p.Y)
            };

            List<Point> outpoints = new List<Point>();

            if (!transparentPoints.ContainsKey(f.UniqueMultiplayerID))
                transparentPoints.Add(f.UniqueMultiplayerID, new Point[0]);

            foreach (Point i in transparentPoints[f.UniqueMultiplayerID])
            {
                if (!points.Contains(i))
                    outpoints.Add(i);
            }
            transparentPoints[f.UniqueMultiplayerID] = points;

            return outpoints.ToArray();
        }

    }
}
