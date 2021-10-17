using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Toolkit.Serialization.Models;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace CoinCollector
{
    public class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModEntry context;
        public static ModConfig config;

        public static Dictionary<string, CoinData> coinDataDict = new Dictionary<string, CoinData>();
        public static Dictionary<string, Dictionary<Vector2, string>> coinLocationDict = new Dictionary<string, Dictionary<Vector2, string>>();
        
        public static int deltaTime;
        private static SoundEffect blipEffectCenter;
        private static SoundEffect blipEffectLeft;
        private static SoundEffect blipEffectRight;
        private float totalRarities;
        private static IDynamicGameAssetsApi apiDGA;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(Projectile), nameof(Projectile.isColliding)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Projectile_isColliding_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.makeHoeDirt)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_makeHoeDirt_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.doSwipe)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MeleeWeapon_doSwipe_Prefix))
            );
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
        }

        private static void MeleeWeapon_doSwipe_Prefix(MeleeWeapon __instance)
        {
            if (!config.EnableMod || __instance.Name != config.MetalDetectorID)
                return;
            DoBlip();
        }

        private static bool Projectile_isColliding_Prefix(Projectile __instance, ref bool __result)
        {
            if (!config.EnableMod || !(__instance is IndicatorProjectile))
                return true;
            __result = false;
            return false;
        }

        private static void GameLocation_makeHoeDirt_Prefix(GameLocation __instance, Vector2 tileLocation, bool ignoreChecks)
        {
            if (!config.EnableMod || !coinLocationDict.ContainsKey(__instance.Name) || !coinLocationDict[__instance.Name].ContainsKey(tileLocation) || (!ignoreChecks && (__instance.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") == null || __instance.isTileOccupied(tileLocation, "", false) || !__instance.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport))))
                return;
            var data = coinDataDict[coinLocationDict[__instance.Name][tileLocation]];
            context.Monitor.Log($"Digging up {data.id}");
            if (data.isDGA)
            {
                Debris d = new Debris((Object)apiDGA.SpawnDGAItem(data.id), new Vector2(tileLocation.X * 64 + 32, tileLocation.Y * 64 + 32), new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()));
                __instance.debris.Add(d);
            }
            else
            {
                Game1.createObjectDebris(data.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, null);
            }
            coinLocationDict[__instance.Name].Remove(tileLocation);
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!config.EnableMod || coinDataDict.Count == 0)
                return;

            coinLocationDict.Clear();

            foreach (GameLocation gl in Game1.locations)
            {
                if (gl.IsOutdoors && Game1.random.NextDouble() < config.MapHasCoinsChance)
                {
                    int coins = Game1.random.Next(config.MinCoinsPerMap, config.MaxCoinsPerMap + 1 + (int)Math.Round(Game1.player.LuckLevel * config.LuckFactor));
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

                    for(int i = 0; i < coins; i++)
                    {
                        double whichRarity = Game1.random.NextDouble() * totalRarities;
                        float rarities = 0;
                        foreach (var coin in coinDataDict.Values)
                        {
                            rarities += coin.rarity;
                            if(whichRarity < rarities)
                            {
                                int idx = Game1.random.Next(diggableTiles.Count);
                                coinLocationDict[gl.Name][diggableTiles[idx]] = coin.id;
                                Monitor.Log($"Added coin {coin.id} to {gl.Name} at {diggableTiles[idx]}");
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
            if (!config.EnableMod || config.RequireMetalDetectorSwing || !Context.IsPlayerFree || (config.RequireMetalDetector && Game1.player.CurrentTool?.Name != config.MetalDetectorID) || ++deltaTime < config.SecondsPerPoll)
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
            if (nearest.X >= 0 && (config.MaxPixelPingDistance <= 0 || (Math.Abs(nearest.X * 64 - playerLocation.X) < config.MaxPixelPingDistance) && Vector2.Distance(playerLocation, nearest * 64) < config.MaxPixelPingDistance))
            {
                float pan = nearest.X * 64 > playerLocation.X ? 1f : (nearest.X * 64 < playerLocation.X ? -1f : 0f);
                float vol = Math.Clamp((1 - Vector2.Distance(playerLocation, nearest * 64) / (config.MaxPixelPingDistance > 0 ? config.MaxPixelPingDistance : Game1.viewport.Width) * config.BlipAudioVolume), 0, 1);
                float pitch = config.BlipAudioIncreasePitch ? vol * 2 - 1 : 0;

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

                if (config.EnableIndicator)
                {
                    IndicatorProjectile bp = new IndicatorProjectile(0, config.IndicatorSprite, 0, 32, 0, velocity.X * config.IndicatorSpeed, velocity.Y * config.IndicatorSpeed, playerLocation, "", "", false, false, Game1.player.currentLocation, Game1.player, false, null);
                    bp.maxTravelDistance.Value = (int)Math.Min(Math.Round(64 * config.IndicatorLength), Vector2.Distance(playerLocation, nearest * 64));
                    context.Helper.Reflection.GetField<NetBool>(bp, "damagesMonsters").GetValue().Value = false;
                    context.Helper.Reflection.GetField<NetBool>(bp, "ignoreLocationCollision").GetValue().Value = true;
                    context.Helper.Reflection.GetField<NetBool>(bp, "ignoreMeleeAttacks").GetValue().Value = true;
                    Game1.player.currentLocation.projectiles.Add(bp);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            apiDGA = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            IManifest manifest = new Manifest("aedenthorn.CoinCollectorDGA", "CoinCollectorDGA", "aedenthorn", "CoinCollectorDGA", new SemanticVersion("0.1.0"))
            {
                ContentPackFor = new ManifestContentPackFor
                {
                    UniqueID = "spacechase0.DynamicGameAssets"
                },
                ExtraFields = new Dictionary<string, object>() { { "DGA.FormatVersion", 2 }, { "DGA.ConditionsFormatVersion", "1.23.0" } }
            };

            apiDGA.AddEmbeddedPack(manifest, $"{Helper.DirectoryPath}/dga");

            if(config.BlipAudioPath.Length > 0)
            {
                try
                {
                    blipEffectCenter = SoundEffect.FromStream(new FileStream($"{Helper.DirectoryPath}/{config.BlipAudioPath}", FileMode.Open));
                    blipEffectLeft = SoundEffect.FromStream(new FileStream($"{Helper.DirectoryPath}/{config.BlipAudioPathLeft}", FileMode.Open));
                    blipEffectRight = SoundEffect.FromStream(new FileStream($"{Helper.DirectoryPath}/{config.BlipAudioPathRight}", FileMode.Open));
                }
                catch { }
            }

            totalRarities = 0;

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {

                CoinDataDict floorWallDataDict = contentPack.ReadJsonFile<CoinDataDict>("content.json") ?? new CoinDataDict();
                foreach (CoinData data in floorWallDataDict.data)
                {
                    try
                    {
                        coinDataDict.Add(data.id, data);
                        totalRarities += data.rarity;
                    }
                    catch(Exception ex)
                    {
                        Monitor.Log($"Exception getting data for {data.id} in content pack {contentPack.Manifest.Name}:\n{ex}", LogLevel.Error);
                    }
                }
            }
            Monitor.Log($"Loaded coin data for {coinDataDict.Count} coins.");
        }
    }
}