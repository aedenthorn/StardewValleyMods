using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace CoinCollector
{
    public partial class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModEntry context;
        public static ModConfig Config;

        public static Dictionary<string, CoinData> coinDataDict = new Dictionary<string, CoinData>();
        public static Dictionary<string, Dictionary<Vector2, string>> coinLocationDict = new Dictionary<string, Dictionary<Vector2, string>>();
        public static Dictionary<string, Texture2D> coinTextures = new Dictionary<string, Texture2D>();

        public static int deltaTime;
        private static SoundEffect blipEffectCenter;
        private static SoundEffect blipEffectLeft;
        private static SoundEffect blipEffectRight;
        private float totalRarities;
        public static readonly int detectorIndex = -42424202;
        public static readonly int coinFirstIndex = -42425000;

        private static string dictPath = "aedenthorn.CoinCollector/dictionary";
        private static string texturePath = "aedenthorn.CoinCollector/detector";
        private static string detectorName = "Metal Detector";
        private static Texture2D detectorTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CoinData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(AddDetectorRecipe);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/weapons"))
            {
                e.Edit(AddDetectorInfo);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit(AddCoinInfo);
            }
        }

        private void AddDetectorRecipe(IAssetData obj)
        {
            IDictionary<string, string> data = obj.AsDictionary<string, string>().Data;
            data.Add(detectorName, $"{Config.CraftingRequirements}/Home/{detectorIndex}/true/{Helper.Translation.Get("name")}");
        }
        private void AddDetectorInfo(IAssetData obj)

        {
            IDictionary<int, string> data = obj.AsDictionary<int, string>().Data;
            data.Add(detectorIndex, $"{detectorName}/{Helper.Translation.Get("description")}/5/15/1.5/-16/0/0/2/56/50/1/.02/3/{Helper.Translation.Get("name")}");
        }

        private void AddCoinInfo(IAssetData obj)

        {
            IDictionary<int, string> dict = obj.AsDictionary<int, string>().Data;
            foreach(var data in coinDataDict.Values)
            {
                dict.Add(data.index, $"{data.name}/750/-300/Minerals -2/Diamond/A rare and valuable gem.")fasdfasfd;
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            try
            {
                detectorTexture = Game1.content.Load<Texture2D>(texturePath);
            }
            catch
            {

            }
            if (detectorTexture == null)
            {
                detectorTexture = Helper.ModContent.Load<Texture2D>("assets/metaldetector.png");
            }
            int index = coinFirstIndex;

            coinDataDict = Game1.content.Load<Dictionary<string, CoinData>>(dictPath);
            foreach (var key in coinDataDict.Keys.ToArray())
            {
                var data = coinDataDict[key];
                try
                {
                    coinTextures[key] = Game1.content.Load<Texture2D>(data.texturePath);
                    totalRarities += data.rarity;
                    coinDataDict[key].index = index;
                    index++;
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error loading coin {data.name}: \n\n{ex}", LogLevel.Error);
                    coinDataDict.Remove(key);
                }
            }
            Monitor.Log($"Loaded coin data for {coinDataDict.Count} coins.");
            Helper.GameContent.InvalidateCache("Data/ObjectInformation");
            Game1.objectInformation = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Config.ModEnabled || coinDataDict.Count == 0)
                return;

            coinLocationDict.Clear();

            foreach (GameLocation gl in Game1.locations)
            {
                if (gl.IsOutdoors && Game1.random.NextDouble() < Config.MapHasCoinsChance)
                {
                    int coins = Game1.random.Next(Config.MinCoinsPerMap, Config.MaxCoinsPerMap + 1 + (int)Math.Round(Game1.player.LuckLevel * Config.LuckFactor));
                    if (coins == 0)
                        continue;

                    List<Vector2> diggableTiles = new List<Vector2>();
                    for (int x = 0; x < gl.map.GetLayer("Back").LayerWidth; x++)
                    {
                        for (int y = 0; y < gl.map.GetLayer("Back").LayerHeight; y++)
                        {
                            if (gl.doesTileHaveProperty(x, y, "Diggable", "Back") != null && !gl.isTileOccupied(new Vector2(x, y), "", false) && gl.isTilePassable(new Location(x, y), Game1.viewport))
                                diggableTiles.Add(new Vector2(x, y));
                        }
                    }
                    if (diggableTiles.Count == 0)
                        continue;

                    Monitor.Log($"Adding coins to {gl.Name}");

                    if (!coinLocationDict.ContainsKey(gl.Name))
                    {
                        coinLocationDict.Add(gl.Name, new Dictionary<Vector2, string>());
                    }
                    var locationCoins = coinDataDict.Where(c => c.Value.locations == null || c.Value.locations.Count == 0 || c.Value.locations.Contains(gl.Name));
                    for (int i = 0; i < coins; i++)
                    {
                        double whichRarity = Game1.random.NextDouble() * totalRarities;
                        float rarities = 0;
                        foreach (var coin in locationCoins)
                        {
                            rarities += coin.Value.rarity;
                            if(whichRarity < rarities)
                            {
                                int idx = Game1.random.Next(diggableTiles.Count);
                                coinLocationDict[gl.Name][diggableTiles[idx]] = coin.Key;
                                Monitor.Log($"Added coin {coin.Value.name} to {gl.Name} at {diggableTiles[idx]}");
                                diggableTiles.RemoveAt(idx);
                                break;
                            }
                        }
                        if (diggableTiles.Count == 0)
                            break;
                    }
                }
            }
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || Config.RequireMetalDetectorSwing || !Context.IsPlayerFree || (Config.RequireMetalDetector && Game1.player.CurrentTool?.Name != Config.MetalDetectorID) || ++deltaTime < Config.SecondsPerPoll)
                return;

            DoBlip();
        }

        private static void DoBlip()
        {
            deltaTime = 0;
            if (!coinLocationDict.ContainsKey(Game1.player.currentLocation.Name))
                return;
            Vector2 playerLocation = Game1.player.Position + new Vector2(0, -Game1.tileSize / 2f);

            Vector2 nearest = -Vector2.One;
            foreach (var kvp in coinLocationDict[Game1.player.currentLocation.Name])
            {
                if (Vector2.Distance(playerLocation, nearest * 64) > Vector2.Distance(playerLocation, kvp.Key * 64))
                {
                    nearest = kvp.Key;
                }
            }
            //Monitor.Log($"Nearest coin at {nearest} {Vector2.Distance(playerLocation / 64, nearest)} tiles away");
            if (nearest.X >= 0 && (Config.MaxPixelPingDistance <= 0 || (Math.Abs(nearest.X * 64 - playerLocation.X) < Config.MaxPixelPingDistance) && Vector2.Distance(playerLocation, nearest * 64) < Config.MaxPixelPingDistance))
            {
                float pan = nearest.X * 64 > playerLocation.X ? 1f : (nearest.X * 64 < playerLocation.X ? -1f : 0f);
                float vol = Math.Clamp((1 - Vector2.Distance(playerLocation, nearest * 64) / (Config.MaxPixelPingDistance > 0 ? Config.MaxPixelPingDistance : Game1.viewport.Width) * Config.BlipAudioVolume), 0, 1);
                float pitch = Config.BlipAudioIncreasePitch ? vol * 2 - 1 : 0;

                SoundEffect blipEffect;

                if (pan == 0)
                {
                    blipEffect = blipEffectCenter;
                }
                else if (pan > 0)
                {
                    blipEffect = blipEffectRight;
                }
                else
                {
                    blipEffect = blipEffectLeft;
                }

                if (vol > 0 && blipEffect != null)
                {
                    blipEffect.Play(vol, pitch, pan);
                }
                Vector2 velocity = nearest * 64 - playerLocation;
                velocity.Normalize();

                if (Config.EnableIndicator)
                {
                    IndicatorProjectile bp = new IndicatorProjectile(0, Config.IndicatorSprite, 0, 32, 0, velocity.X * Config.IndicatorSpeed, velocity.Y * Config.IndicatorSpeed, playerLocation, "", "", false, false, Game1.player.currentLocation, Game1.player, false, null);
                    bp.maxTravelDistance.Value = (int)Math.Min(Math.Round(64 * Config.IndicatorLength), Vector2.Distance(playerLocation, nearest * 64));
                    context.Helper.Reflection.GetField<NetBool>(bp, "damagesMonsters").GetValue().Value = false;
                    context.Helper.Reflection.GetField<NetBool>(bp, "ignoreLocationCollision").GetValue().Value = true;
                    context.Helper.Reflection.GetField<NetBool>(bp, "ignoreMeleeAttacks").GetValue().Value = true;
                    Game1.player.currentLocation.projectiles.Add(bp);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if(Config.BlipAudioPath.Length > 0)
            {
                try
                {
                    blipEffectCenter = SoundEffect.FromStream(new FileStream($"{Helper.DirectoryPath}/{Config.BlipAudioPath}", FileMode.Open));
                    blipEffectLeft = SoundEffect.FromStream(new FileStream($"{Helper.DirectoryPath}/{Config.BlipAudioPathLeft}", FileMode.Open));
                    blipEffectRight = SoundEffect.FromStream(new FileStream($"{Helper.DirectoryPath}/{Config.BlipAudioPathRight}", FileMode.Open));
                }
                catch { }
            }

            totalRarities = 0;

        }
    }
}