using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace GemIsles
{
    public class ModEntry : Mod, IAssetEditor
    {

        public static ModEntry context;

        internal static ModConfig Config;
        private static IMonitor SMonitor;

        private static int mapX;
        private static int mapY;

        public static string mapAssetKey;
        private string locationPrefix = "GemIsles_";

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;

            Utils.Initialize(Config, Monitor, Helper);

            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            for (int i = Game1.locations.Count - 1; i >= 0; i--)
            {
                if (Game1.locations[i].Name.StartsWith(locationPrefix))
                {
                    Game1.locations[i].characters.Clear();
                    Game1.locations.RemoveAt(i);
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            mapAssetKey = Helper.Content.GetActualAssetKey("assets/isles.tbin", ContentSource.ModFolder);
        }


        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Game1.player.swimming || (!(Game1.player.currentLocation is Beach) && !(Game1.player.currentLocation is Forest) && !Game1.currentLocation.Name.StartsWith(locationPrefix)))
                return;

            Point pos = Game1.player.getTileLocationPoint();
            if (Game1.player.position.Y > Game1.viewport.Y + Game1.viewport.Height - 20)
            {
                Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y + Game1.viewport.Height - 21);
                Monitor.Log("warping south");
                mapY++;
                if (Game1.player.currentLocation.Name == "Beach")
                {
                    mapY = 1;
                    mapX = 1;
                    pos.X = pos.X * 128 / 104;
                }
                else if (Game1.player.currentLocation.Name == "Forest")
                {
                    mapY = 1;
                    mapX = 0;
                    pos.X = pos.X * 128 / 120;
                }
                WarpToGemIsles(pos.X, 0);
                return;
            }
            if (!Game1.currentLocation.Name.StartsWith(locationPrefix))
                return;
            if (Game1.player.position.Y < Game1.viewport.Y - 12)
            {
                Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y - 11);
                mapY--;
                Monitor.Log("warping north");
                if (mapY > 0)
                {
                    WarpToGemIsles(pos.X, 71);
                }
                else
                {
                    if (true || mapX > 0)
                    {
                        pos.X = pos.X * 104 / 128;
                        Game1.warpFarmer("Beach", pos.X, Game1.getLocationFromName("Beach").map.DisplaySize.Height / Game1.tileSize - 2, false);
                    }
                    else
                    {
                        pos.X = pos.X * 120 / 128;
                        Game1.warpFarmer("Forest", pos.X, Game1.getLocationFromName("Forest").map.DisplaySize.Height - 2, false);
                    }
                }
            }
            else if (Game1.player.position.X > Game1.viewport.X + Game1.viewport.Width - 36)
            {
                Game1.player.position.Value = new Vector2(Game1.viewport.X + Game1.viewport.Width - 37, Game1.player.position.Y);
                mapX++;
                Monitor.Log("warping east");
                WarpToGemIsles(0, pos.Y);
            }
            else if (Game1.player.position.X < Game1.viewport.X - 28)
            {
                Game1.player.position.Value = new Vector2(Game1.viewport.X - 27, Game1.player.position.Y);
                mapX--;
                Monitor.Log("warping west");
                WarpToGemIsles(127, pos.Y);
            }
        }
        private void WarpToGemIsles(int x, int y)
        {
            if (Game1.eventUp)
                return;
            string name = $"{locationPrefix}{mapX}_{mapY}";
            if (Game1.getLocationFromName(name) == null)
            {
                GameLocation location = new GameLocation(mapAssetKey, name) { IsOutdoors = true, IsFarm = false };
                Game1.locations.Add(location);
                Helper.Content.InvalidateCache("Data/Locations");
                Utils.CreateIslesMap(location);
            }
            Game1.warpFarmer(name, x, y, false);
        }


        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;
            if (asset.AssetNameEquals("Data/Locations"))
                return true;

            return false;
        }
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/Locations"))
            {
                var editor = asset.AsDictionary<string, string>();
                foreach(string isle in Game1.locations.Where(l => l.Name.StartsWith(locationPrefix)).Select(l => l.Name))
                {
                    editor.Data[isle] = editor.Data["Beach"];
                }
            }
        }
    }
}
 