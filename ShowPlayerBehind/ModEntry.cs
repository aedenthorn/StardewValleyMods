using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile.Layers;

namespace ShowPlayerBehind
{
    public partial class ModEntry : Mod
    {
        
        public static ModConfig Config;
        public static IMonitor SMonitor;

        private Dictionary<GameLocation, HashSet<Point>> fadeInPoints = new();
        private Dictionary<GameLocation, Dictionary<Point, float>> fadeOutPoints = new();
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            SMonitor = Monitor;

            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
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
                name: () => "Transparency Fade Speed",
                getValue: () => Config.TransparencyFadeSpeed+ "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { Config.TransparencyFadeSpeed = f; } }
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
        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady)
                return;
            Dictionary<GameLocation, Dictionary<Point, float>> farmerPoints = new();
            foreach (Farmer f in Game1.getOnlineFarmers())
            {
                if (f?.currentLocation?.Map?.Layers is null)
                    continue;
                bool coverFeet = false;
                bool coverHead = false;
                foreach(var l in f.currentLocation.Map.Layers)
                {
                    if (!l.Id.StartsWith("Front") && !l.Id.StartsWith("AlwaysFront"))
                        continue;
                    if (IsOutOfBounds(f.TilePoint, l) || IsOutOfBounds(f.TilePoint + new Point(0, -1), l))
                        continue;
                    if (l.Tiles[f.TilePoint.X, f.TilePoint.Y] != null)
                        coverFeet = true;
                    if (l.Tiles[f.TilePoint.X, f.TilePoint.Y - 1] != null)
                        coverHead = true;
                    if (coverFeet && coverHead)
                        break;
                }
                if (!coverHead || !coverFeet)
                    continue;

                Point fp = f.TilePoint;
                if (!farmerPoints.TryGetValue(f.currentLocation, out var dict))
                {
                    dict = new();
                    farmerPoints[f.currentLocation] = dict;
                }
                dict[fp] = Config.InnerTransparency;
                dict[fp + new Point(0, -1)] = Config.InnerTransparency;

                float v;
                if(!dict.TryGetValue(fp + new Point(1, 1), out v) || v > Config.CornerTransparency)
                    dict[fp + new Point(1, 1)] = Config.CornerTransparency;
                if (!dict.TryGetValue(fp + new Point(-1, 1), out v) || v > Config.CornerTransparency)
                    dict[fp + new Point(-1, 1)] = Config.CornerTransparency;

                if (!dict.TryGetValue(fp + new Point(-1, 0), out v) || v > Config.OuterTransparency)
                    dict[fp + new Point(-1, 0)] = Config.OuterTransparency;
                if (!dict.TryGetValue(fp + new Point(1, 0), out v) || v > Config.OuterTransparency)
                    dict[fp + new Point(1, 0)] = Config.OuterTransparency;
                if (!dict.TryGetValue(fp + new Point(-1, -1), out v) || v > Config.OuterTransparency)
                    dict[fp + new Point(-1, -1)] = Config.OuterTransparency;
                if (!dict.TryGetValue(fp + new Point(1, -1), out v) || v > Config.OuterTransparency)
                    dict[fp + new Point(1, -1)] = Config.OuterTransparency;
                if (!dict.TryGetValue(fp + new Point(0, 1), out v) || v > Config.OuterTransparency)
                    dict[fp + new Point(0, 1)] = Config.OuterTransparency;

                if (!dict.TryGetValue(fp + new Point(-1, -2), out v) || v > 10 + Config.CornerTransparency)
                    dict[fp + new Point(-1, -2)] = 10 + Config.CornerTransparency;
                if (!dict.TryGetValue(fp + new Point(1, -2), out v) || v > 10 + Config.CornerTransparency)
                    dict[fp + new Point(1, -2)] = 10 + Config.CornerTransparency;
                if (!dict.TryGetValue(fp + new Point(0, -2), out v) || v > 10 + Config.OuterTransparency)
                    dict[fp + new Point(0, -2)] = 10 + Config.OuterTransparency;

            }
            foreach (var kvp in farmerPoints)
            {
                if(!fadeOutPoints.ContainsKey(kvp.Key))
                {
                    fadeOutPoints[kvp.Key] = new();
                }
                var dict = farmerPoints[kvp.Key];
                foreach(var kvp2 in dict)
                {
                    fadeOutPoints[kvp.Key][kvp2.Key] = kvp2.Value;
                }
            }
            foreach (var key in fadeOutPoints.Keys.ToArray())
            {
                foreach (var kvp in fadeOutPoints[key].ToArray())
                {
                    Point p = kvp.Key;
                    if (farmerPoints.TryGetValue(key, out var dict))
                    {
                        if (dict.TryGetValue(p, out var trans))
                        {
                            foreach (var l in key.Map.Layers)
                            {
                                if ((trans >= 10 && l.Id.StartsWith("Front")) || (!l.Id.StartsWith("AlwaysFront") && !l.Id.StartsWith("Front")))
                                {
                                    continue;
                                }
                                if (IsOutOfBounds(p, l))
                                {
                                    continue;
                                }
                                if (l.Tiles[p.X, p.Y] != null)
                                {

                                    string os = null;
                                    if (l.Tiles[p.X, p.Y].Properties.TryGetValue("@Opacity", out os))
                                    {
                                        if (l.Tiles[p.X, p.Y].Properties.ContainsKey("@ShowPlayerBehind"))
                                        {
                                            var opacity = float.Parse(os) - Config.TransparencyFadeSpeed;
                                            l.Tiles[p.X, p.Y].Properties["@Opacity"] = Math.Max(opacity, trans % 10) + "";
                                        }
                                    }
                                    else
                                    {
                                        l.Tiles[p.X, p.Y].Properties["@Opacity"] = "1";
                                        l.Tiles[p.X, p.Y].Properties["@ShowPlayerBehind"] = "T";
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (!fadeInPoints.ContainsKey(key))
                            {
                                fadeInPoints[key] = new();
                            }
                            fadeInPoints[key].Add(p);
                            fadeOutPoints[key].Remove(p);
                        }

                    }
                    else
                    {
                        if (!fadeInPoints.ContainsKey(key))
                        {
                            fadeInPoints[key] = new();
                        }
                        fadeInPoints[key].Add(p);
                        fadeOutPoints[key].Remove(p);
                    }
                }
            }
            foreach (var key in fadeInPoints.Keys.ToArray())
            {
                var front = key.Map.GetLayer("Front");
                foreach (var p in fadeInPoints[key].ToArray())
                {
                    if (!farmerPoints.TryGetValue(key, out var dict) || !dict.ContainsKey(p))
                    {
                        foreach (var l in key.Map.Layers)
                        {
                            if ((l.Id.StartsWith("Front") || l.Id.StartsWith("AlwaysFront")) && l.Tiles[p.X, p.Y] != null)
                            {
                                string os = null;
                                if (l.Tiles[p.X, p.Y].Properties.TryGetValue("@Opacity", out os))
                                {
                                    if(l.Tiles[p.X, p.Y].Properties.ContainsKey("@ShowPlayerBehind"))
                                    {
                                        var opacity = float.Parse(os) + Config.TransparencyFadeSpeed;
                                        if (opacity < 1)
                                        {
                                            l.Tiles[p.X, p.Y].Properties["@Opacity"] = opacity + "";
                                        }
                                        else
                                        {
                                            l.Tiles[p.X, p.Y].Properties.Remove("@Opacity");
                                            l.Tiles[p.X, p.Y].Properties.Remove("@ShowPlayerBehind");
                                            fadeInPoints[key].Remove(p);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsOutOfBounds(Point p, Layer layer)
        {
            return p.X < 0 || p.Y < 0 || p.X >= layer.LayerWidth || p.Y >= layer.LayerHeight;
        }
    }
}
