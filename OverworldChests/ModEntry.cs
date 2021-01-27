using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace OverworldChests
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        private List<string> niceNPCs = new List<string>();
        public static IAdvancedLootFrameworkApi advancedLootFrameworkApi = null;
        private Random myRand;
        private Color[] tintColors = new Color[]
        {
            Color.DarkGray,
            Color.Brown,
            Color.Silver,
            Color.Gold,
            Color.Purple,
        };
        private static string namePrefix = "Overworld Chest Mod Chest";
        private List<object> treasuresList;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            myRand = new Random();

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Chest_draw_Prefix))
            );
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi != null)
            {
                Monitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
            }
            treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListTypes, Config.MinItemValue, Config.MaxItemValue);
            Monitor.Log($"Got {treasuresList.Count} possible treasures");

        }

        private static bool Chest_draw_Prefix(Chest __instance)
        {
            if (!__instance.name.StartsWith(namePrefix))
                return true;

            if (!Game1.player.currentLocation.overlayObjects.ContainsKey(__instance.tileLocation) || (__instance.items.Count > 0 && __instance.items[0] != null) || __instance.coins > 0)
                return true;

            context.Monitor.Log($"removing chest at {__instance.tileLocation}");
            Game1.player.currentLocation.overlayObjects.Remove(__instance.tileLocation);
            return false;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            var spawn = Helper.Data.ReadSaveData<LastOverWorldChestSpawn>("lastOverworldChestSpawn") ?? new LastOverWorldChestSpawn();
            int days = Game1.Date.TotalDays - spawn.lastOverworldChestSpawn;
            Monitor.Log($"Last spawn: {days} days ago");
            if (spawn.lastOverworldChestSpawn < 1 || days < 2 || (Config.RespawnInterval > 0 && days >= Config.RespawnInterval)) 
            {
                Monitor.Log($"Respawning chests", LogLevel.Debug);
                spawn.lastOverworldChestSpawn = Game1.Date.TotalDays;
                Helper.Data.WriteSaveData("lastOverworldChestSpawn", spawn);
                RespawnChests();
            }
        }

        private void RespawnChests()
        {
            foreach(GameLocation l in Game1.locations)
            {
                if (l is FarmHouse || l is Cabin || (!Config.AllowIndoorSpawns && !l.IsOutdoors) || !IsLocationAllowed(l))
                    continue;

                Monitor.Log($"Respawning chests in {l.name}");
                int rem = 0;
                for (int i = l.overlayObjects.Keys.Count - 1; i >= 0; i--)
                {
                    Object obj = l.overlayObjects[l.overlayObjects.Keys.ToArray()[i]];
                    if (obj is Chest && obj.Name.StartsWith(namePrefix))
                    {
                        rem++;
                        l.overlayObjects.Remove(l.overlayObjects.Keys.ToArray()[i]);
                    }
                }
                Monitor.Log($"Removed {rem} chests");
                List<Vector2> freeTiles = new List<Vector2>();
                for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                    {
                        bool water = false;
                        try { water = l.waterTiles[x, y]; } catch { }
                        if (!l.isTileOccupiedForPlacement(new Vector2(x, y)) && !water && !l.isCropAtTile(x, y))
                            freeTiles.Add(new Vector2(x, y));

                    }
                }
                Monitor.Log($"Got {freeTiles.Count} free tiles");

                // shuffle list
                int n = freeTiles.Count;
                while (n > 1)
                {
                    n--;
                    int k = myRand.Next(n + 1);
                    var value = freeTiles[k];
                    freeTiles[k] = freeTiles[n];
                    freeTiles[n] = value;
                }
                int maxChests = Math.Min(freeTiles.Count, (int)Math.Floor(freeTiles.Count * Config.ChestDensity) + (Config.RoundNumberOfChestsUp ? 1 : 0));
                Monitor.Log($"Max chests: {maxChests}");

                for (int i = 0; i < maxChests; i++)
                {
                    Chest chest;
                    if (advancedLootFrameworkApi == null)
                    {
                        //Monitor.Log($"Adding ordinary chest");
                        chest = new Chest(0, new List<Item>() { MineShaft.getTreasureRoomItem() }, freeTiles[i], false, 0);
                    }
                    else
                    {
                        double fraction = Math.Pow(myRand.NextDouble(), 1 / Config.RarityChance);
                        int level = (int)Math.Ceiling(fraction * Config.Mult);
                        //Monitor.Log($"Adding expanded chest of value {level} to {l.name}");
                        chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, level, Config.IncreaseRate, Config.ItemsBaseMaxValue, Config.CoinBaseMin, Config.CoinBaseMax, freeTiles[i]);
                        chest.playerChoiceColor.Value = MakeTint(fraction);
                    }
                    chest.name = namePrefix;
                    chest.resetLidFrame();
                    l.overlayObjects[freeTiles[i]] = chest;
                }
            }
        }

        private bool IsLocationAllowed(GameLocation l)
        {
            if(Config.OnlyAllowLocations.Length > 0)
                return Config.OnlyAllowLocations.Split(',').Contains(l.name);
            return !Config.DisallowLocations.Split(',').Contains(l.name);
        }

        private Color MakeTint(double fraction)
        {
            Color color = tintColors[(int)Math.Floor(fraction * tintColors.Length)];
            return color;
        }

    }
}
