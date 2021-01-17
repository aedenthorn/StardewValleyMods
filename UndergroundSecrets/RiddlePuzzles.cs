using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using static StardewValley.Network.NetAudio;

namespace UndergroundSecrets
{
    internal class RiddlePuzzles
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int litEyes = 249;
        private static int questions = 0;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
            CountQuestions();
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            if (Game1.random.NextDouble() >= config.RiddlesBaseChance * Math.Pow(shaft.mineLevel, config.PuzzleChanceIncreaseRate) || clearCenters.Count == 0)
                return;

            monitor.Log($"adding a riddler");

            Vector2 spot = clearCenters[Game1.random.Next(0, clearCenters.Count)];

            CreatePuzzle(spot, shaft);

            foreach (Vector2 v in Utils.GetSurroundingTiles(spot, 3))
            {
                superClearCenters.Remove(v);
                if (Math.Abs(v.X - spot.X) < 3 && Math.Abs(v.Y - spot.Y) < 3)
                {
                    clearCenters.Remove(v);
                    if (Math.Abs(v.X - spot.X) < 2 && Math.Abs(v.Y - spot.Y) < 2)
                        clearSpots.Remove(v);
                }
            }
        }

        public static void CreatePuzzle(Vector2 spot, MineShaft shaft)
        {
            Layer front = shaft.map.GetLayer("Front");
            Layer buildings = shaft.map.GetLayer("Buildings");
            if (shaft.map.TileSheets.FirstOrDefault(s => s.Id == ModEntry.tileSheetId) == null)
                shaft.map.AddTileSheet(new TileSheet(ModEntry.tileSheetId, shaft.map, ModEntry.tileSheetPath, new Size(16, 18), new Size(16, 16)));
            TileSheet tilesheet = shaft.map.GetTileSheet(ModEntry.tileSheetId);

            front.Tiles[(int)spot.X, (int)spot.Y - 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: litEyes);
            buildings.Tiles[(int)spot.X, (int)spot.Y] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: litEyes + 32);

            shaft.setTileProperty((int)spot.X, (int)spot.Y, "Buildings", "Action", $"undergroundRiddles");
        }

        internal static void Interact(MineShaft shaft, Location tileLocation, Farmer who)
        {
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            ShowQuestion(tileLocation);
        }

        private static void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu != null && Game1.player.currentLocation is MineShaft && Game1.player.currentLocation.lastQuestionKey == "UndergroundSecrets_Question")
            {
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;
                
                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                List<Response> resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;
                Game1.player.currentLocation.lastQuestionKey = "";
                helper.Events.Input.ButtonPressed -= Input_ButtonPressed;

                AnswerResult(resps[resp].responseKey);
                return;
            }
        }

        private static void AnswerResult(string responseKey)
        {
            string[] split = responseKey.Split('#');
            int x = int.Parse(split[1]);
            int y = int.Parse(split[2]);
            
            Game1.player.currentLocation.removeTileProperty(x, y, "Buildings", "Action");

            string type = split[3];
            switch (type)
            {
                case "Y":
                    Game1.player.currentLocation.removeTile(x, y - 1, "Front");
                    Game1.player.currentLocation.removeTile(x, y, "Buildings");
                    Utils.DropChest(Game1.player.currentLocation as MineShaft, new Vector2(x, y));
                    return;
                case "S":
                    Game1.player.currentLocation.removeTile(x, y - 1, "Front");
                    Game1.player.currentLocation.removeTile(x, y, "Buildings");
                    CollapsingFloors.collapseFloor(Game1.player.currentLocation as MineShaft, Game1.player.getTileLocation());
                    return;
                default:
                    Game1.player.currentLocation.setMapTileIndex(x, y - 1, litEyes + 16, "Front");
                    Traps.TriggerRandomTrap(Game1.player.currentLocation as MineShaft, Game1.player.getTileLocation(), false);
                    return;
            }
        }

        private static void ShowQuestion(Location tileLocation)
        {
            int qi = Game1.random.Next(1,questions);
            Translation s2 = helper.Translation.Get($"question-{qi}");
            if (!s2.HasValue())
            {
                monitor.Log("no dialogue: " + s2.ToString(), LogLevel.Error);
                return;
            }
            //Monitor.Log("has dialogue: " + s2.ToString());
            List<Response> responses = new List<Response>();
            int i = 1;
            while (true)
            {
                Translation r = helper.Translation.Get($"answer-{qi}-{i}");
                if (!r.HasValue())
                    break;
                string[] split = r.ToString().Split('#');
                string str = split[0];

                responses.Add(new Response($"UndergroundSecrets_Answer#{tileLocation.X}#{tileLocation.Y}#{split[1]}", str));
                i++;
            }

            int n = responses.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = responses[k];
                responses[k] = responses[n];
                responses[n] = value;
            }

            Game1.player.currentLocation.createQuestionDialogue($"{s2}", responses.ToArray(), $"UndergroundSecrets_Question");
        }
        private static void CountQuestions()
        {
            while (true)
            {
                questions++;
                Translation r = helper.Translation.Get($"question-{questions}");
                if (!r.HasValue())
                    break;
            }
        }
    }
}