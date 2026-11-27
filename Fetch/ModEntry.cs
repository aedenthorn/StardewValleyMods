using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System.Collections.Generic;

namespace Fetch
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.update), new System.Type[] { typeof(GameTime), typeof(GameLocation) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_update_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.update), new System.Type[] { typeof(GameTime), typeof(GameLocation), typeof(long), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_update_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Character), nameof(Character.MovePosition)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Character_MovePosition_Prefix))
            );
            /*
            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.shouldCollideWithBuildingLayer)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_shouldCollideWithBuildingLayer_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.canPassThroughActionTiles)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_canPassThroughActionTiles_Prefix))
            );
            */
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }
        private static List<Vector2> GetShortestPath(Vector2 tile1, Vector2 tile2, GameLocation location)
        {
            List<List<Vector2>> steps = new List<List<Vector2>>();
            //SMonitor.Log($"Trying to get path from {tile1} to {tile2}");
            GetPossiblePath(steps, new List<Vector2>() { tile1 }, tile1, tile2, location);
            if (steps.Count == 0)
                return null;
            steps.Sort(delegate (List<Vector2> a, List<Vector2> b) { return a.Count.CompareTo(b.Count); });
            foreach(var step in steps)
            {
                SMonitor.Log(string.Join(" | ", step));
            }
            SMonitor.Log($"Got shortest path from {tile1} to {tile2}: {steps[0].Count}-steps");
            return steps[0];
        }

        private static Vector2[] adjacent = new Vector2[]
        {
            new Vector2(0,-1),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(-1,0)
        };

        private static void GetPossiblePath(List<List<Vector2>> steps, List<Vector2> tempSteps, Vector2 tile1, Vector2 tile2, GameLocation location)
        {
            if (tempSteps.Count >= Config.MaxSteps || steps.Exists(v => v.Count <= tempSteps.Count))
                return;
            foreach(var a in adjacent)
            {
                Vector2 adjTile = tile1 + a;
                SMonitor.Log($"trying to add {adjTile} to {string.Join(" | ", tempSteps)} ");
                if (tempSteps.Contains(adjTile) || (tempSteps.Count > 0 && Vector2.Distance(tempSteps[tempSteps.Count - 1], tile2) < Vector2.Distance(adjTile, tile2)))
                {
                    SMonitor.Log($"tile {adjTile} is going the wrong way");
                    continue;
                }
                var newTempSteps = new List<Vector2>(tempSteps);
                newTempSteps.Add(adjTile);
                if (adjTile == tile2)
                {
                    SMonitor.Log($"Got {newTempSteps.Count}-step path {string.Join(" | ", newTempSteps)} from {tile1} to {tile2}");
                    steps.Add(newTempSteps);
                    return;
                }
                if (location.objects.ContainsKey(adjTile) || location.terrainFeatures.ContainsKey(adjTile) || adjTile.X < 0 || adjTile.Y < 0 || adjTile.X >= location.Map.Layers[0].LayerWidth || adjTile.Y >= location.Map.Layers[0].LayerHeight || location.getTileIndexAt((int)adjTile.X, (int)adjTile.Y, "Buildings") > -1)
                {
                    SMonitor.Log($"tile {adjTile} is invalid: objects {location.objects.ContainsKey(adjTile)}, {location.terrainFeatures.ContainsKey(adjTile)}, buildings {location.getTileIndexAt((int)adjTile.X, (int)adjTile.Y, "Buildings")}");
                    continue;
                }
                //SMonitor.Log($"Continuing path {string.Join(" | ",newTempSteps)}");

                GetPossiblePath(steps, newTempSteps, adjTile, tile2, location);
            }
        }

    }
}