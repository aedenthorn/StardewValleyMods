using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using xTile;
using Object = StardewValley.Object;

namespace GemIsles
{
	public class ModEntry : Mod
	{

		public static ModEntry context;

		internal static ModConfig Config;
		private static IMonitor SMonitor;

        private static int mapX;
        private static int mapY;

        private static Dictionary<Point,Map> isleMaps = new Dictionary<Point, Map>();

		public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			SMonitor = Monitor;

            Utils.Initialize(Config, Monitor, Helper);

            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Player.Warped += Player_Warped;
		}

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            string mapAssetKey = Helper.Content.GetActualAssetKey("assets/isles.tbin", ContentSource.ModFolder);
            GameLocation location = new GameLocation(mapAssetKey, "GemIsles") { IsOutdoors = true, IsFarm = false };
            Game1.locations.Add(location);
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            isleMaps.Clear();
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name != "GemIsles")
                return;
            Monitor.Log($"Gem Isles map: {mapX},{mapY}");
            Point p = new Point(mapX, mapY);
            if (!isleMaps.ContainsKey(p))
            {
                Map map = Utils.CreateIslesMap(e.NewLocation);
                isleMaps[p] = map;
            }
            e.NewLocation.map = isleMaps[p];
            e.NewLocation.waterTiles = new bool[e.NewLocation.map.Layers[0].LayerWidth, e.NewLocation.map.Layers[0].LayerHeight];
            bool foundAnyWater = false;
            for (int x = 0; x < e.NewLocation.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < e.NewLocation.map.Layers[0].LayerHeight; y++)
                {
                    if (e.NewLocation.doesTileHaveProperty(x, y, "Water", "Back") != null)
                    {
                        foundAnyWater = true;
                        e.NewLocation.waterTiles[x, y] = true;
                    }
                }
            }
            if (!foundAnyWater)
            {
                e.NewLocation.waterTiles = null;
            }
            //e.NewLocation.resetForPlayerEntry();
        }
        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {

        }
        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Game1.player.swimming || (!(Game1.player.currentLocation is Beach) && !(Game1.player.currentLocation is Forest) && Game1.currentLocation?.Name != "GemIsles"))
                    return;

            Point pos = Game1.player.getTileLocationPoint();
            if (Game1.player.position.Y > Game1.viewport.Y + Game1.viewport.Height - 24)
            {
                Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y + Game1.viewport.Height - 25);
                Monitor.Log("warping south");
                mapY++;
                if (Game1.player.currentLocation is Beach)
                {
                    mapY = 1;
                    mapX = 1;
                    pos.X = pos.X * 104 / 128;
                }
                else if (Game1.player.currentLocation is Forest)
                {
                    mapY = 1;
                    mapX = 0;
                    pos.X = pos.X * 120 / 128;
                }
                Game1.warpFarmer("GemIsles", pos.X, 0, false);
                return;
            }
            if (Game1.currentLocation.Name != "GemIsles")
                return;
            if (Game1.player.position.Y < Game1.viewport.Y - 8)
            {
                Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y - 7);
                mapY--;
                Monitor.Log("warping north");
                if (mapY > 0)
                    Game1.warpFarmer("GemIsles", pos.X, 71, false);
                else
                {
                    if(mapX > 0)
                    {
                        pos.X = pos.X * 104 / 128;
                        Game1.warpFarmer("Beach", pos.X, Game1.getLocationFromName("Beach").map.Layers[0].TileHeight - 1, false);
                    }
                    else
                    {
                        pos.X = pos.X * 120 / 128;
                        Game1.warpFarmer("Forest", pos.X, Game1.getLocationFromName("Forest").map.Layers[0].TileHeight - 1, false);
                    }
                }
            }
            else if (Game1.player.position.X > Game1.viewport.X + Game1.viewport.Width - 40)
            {
                Game1.player.position.Value = new Vector2(Game1.viewport.X + Game1.viewport.Width - 41, Game1.player.position.Y);

                mapX++;
                Monitor.Log("warping east");
                Game1.warpFarmer("GemIsles", 0, pos.Y, false);
            }
            else if (Game1.player.position.X < Game1.viewport.X - 24)
            {
                Game1.player.position.Value = new Vector2(Game1.viewport.X - 23, Game1.player.position.Y);
                mapX--;
                Monitor.Log("warping west");
                Game1.warpFarmer("GemIsles", 127, pos.Y, false);
            }
        }
    }
}
 