using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace ShowPlayerBehind
{
    public class ModEntry : Mod
    {
        
        public static ModConfig config;
        public static IMonitor SMonitor;

        private Dictionary<long, Point[]> transparentPoints = new Dictionary<long, Point[]>();
        private Dictionary<long, Point> farmerPoints = new Dictionary<long, Point>();
        private Texture2D _houseTexture;

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();
            if (!config.EnableMod)
                return;

            SMonitor = Monitor;

            //Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Display.RenderingWorld += Display_RenderingWorld;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            //_houseTexture = Helper.Content.Load<Texture2D>("Buildings\\houses", ContentSource.GameContent);
        }

        private void Display_RenderingWorld(object sender, StardewModdingAPI.Events.RenderingWorldEventArgs e)
        {
            Texture2D houseTexture = null;
            if (config.TransparentFarmBuildings)
            {
                houseTexture = _houseTexture;
            }
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
                            tile.Properties.TryGetValue("@Opacity", out PropertyValue property);
                            float opacity = config.OuterTransparency;
                            if (p == fp || p == new Point(fp.X, fp.Y - 1))
                            {
                                opacity = config.InnerTransparency;
                            }
                            else if(p.X != fp.X && (p.Y == fp.Y - 2 || p.Y == fp.Y + 1))
                            {
                                opacity = config.CornerTransparency;
                            }
                            if (property != null)
                            {
                                gl.map.GetLayer(layer).Tiles[p.X, p.Y].Properties["@Opacity"] = opacity;
                            }
                            else
                            {
                                gl.map.GetLayer(layer).Tiles[p.X, p.Y].Properties.Add("@Opacity", opacity);
                            }
                        }
                    }
                }

                if(gl is Farm && config.TransparentFarmBuildings)
                {
                    if (config.HouseRectangle.Contains(fp))
                    {
                        //AddBuildingTransparency(houseTexture, fp, );
                    }
                }
            }
        }

        private void AddBuildingTransparency(Texture2D texture, Point fp)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

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
