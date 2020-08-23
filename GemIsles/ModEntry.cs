using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using System.Collections.Generic;
using System.Linq;
using xTile;

namespace GemIsles
{
    public class ModEntry : Mod, IAssetEditor
    {

        public static ModEntry context;

        internal static ModConfig Config;
        private static IMonitor SMonitor;

        private static int mapX;
        private static int mapY;

        private static List<string> isleMaps = new List<string>();
        private string mapAssetKey;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;

            Utils.Initialize(Config, Monitor, Helper);

            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Player.Warped += Player_Warped;
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            for (int i = 0; i < Game1.locations.Count; i++)
            {
                if (Game1.locations[i].Name.StartsWith("GemIsles_"))
                {
                    Game1.locations.RemoveAt(i);
                }
            }
            isleMaps.Clear();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            mapAssetKey = Helper.Content.GetActualAssetKey("assets/isles.tbin", ContentSource.ModFolder);
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            //e.NewLocation.resetForPlayerEntry();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Game1.player.swimming || (!(Game1.player.currentLocation is Beach) && !(Game1.player.currentLocation is Forest) && !Game1.currentLocation.Name.StartsWith("GemIsles_")))
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
            if (!Game1.currentLocation.Name.StartsWith("GemIsles_"))
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
            string name = $"GemIsles_{mapX}_{mapY}";
            if (Game1.getLocationFromName(name) == null)
            {
                isleMaps.Add(name);
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
                foreach(string isle in Game1.locations.Where(l => l.Name.StartsWith("GemIsles_")).Select(l => l.Name))
                {
                    editor.Data[isle] = editor.Data["Beach"];
                }
            }
        }
    }
}
 