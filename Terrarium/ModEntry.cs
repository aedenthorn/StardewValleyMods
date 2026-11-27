using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace Terrarium
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IJsonAssetsApi JsonAssets = null;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.Player.Warped += Player_Warped;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(placementAction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(performRemoveAction_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(checkForAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.isPassable)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_isPassable_Prefix))
            );

            if (Config.LoadCustomTerrarium)
            {
                try
                {
                    string path = Directory.GetParent(helper.DirectoryPath).GetDirectories().Where(f => f.FullName.EndsWith("[CP] Lively Frog Sanctuary")).FirstOrDefault()?.FullName;
                    if (path != null)
                    {
                        Texture2D tex = new Texture2D(Game1.graphics.GraphicsDevice, 48, 48);
                        Color[] data = new Color[tex.Width * tex.Height];
                        tex.GetData(data);

                        FileStream setStream = File.Open(Path.Combine(path, "assets", "frog-vivarium.png"), FileMode.Open);
                        Texture2D source = Texture2D.FromStream(Game1.graphics.GraphicsDevice, setStream);
                        setStream.Dispose();
                        Color[] srcData = new Color[source.Width * source.Height];
                        source.GetData(srcData);

                        for (int i = 0; i < srcData.Length; i++)
                        {
                            if (data.Length <= i + 48 * 12)
                                break;
                            data[i + 48 * 12] = srcData[i];
                        }
                        tex.SetData(data);
                        string outDir = Directory.GetParent(helper.DirectoryPath).GetDirectories().Where(f => f.FullName.EndsWith("[BC] Terrarium")).FirstOrDefault()?.FullName;
                        Stream stream = File.Create(Path.Combine(outDir, "assets", "terrarium.png"));
                        tex.SaveAsPng(stream, tex.Width, tex.Height);
                        stream.Dispose();
                        Monitor.Log("Terrarium overwritten with lively frog sanctuary", LogLevel.Debug);
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Can't load lively frog sanctuary for Terrarium\n{ex}", LogLevel.Error);
                }
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            ShowFrogs(Game1.player.currentLocation);
        }

        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for Terrarium", LogLevel.Error);
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "json-assets"));
            }
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            ShowFrogs(e.NewLocation);
        }

        private static void placementAction_Postfix(Object __instance, GameLocation location)
        {
            if(__instance.bigCraftable && IsTerrarium(__instance))
            {
                ShowFrogs(location);
            }
        }
        private static void performRemoveAction_Postfix(Object __instance, GameLocation environment)
        {
            if (__instance.bigCraftable && IsTerrarium(__instance))
            {
                Game1.playSound("croak");
                DelayedShowTerrariums(environment);
            }
        }


        private static void checkForAction_Prefix(Object __instance, Farmer who, bool justCheckingForActivity)
        {
            if (__instance.bigCraftable && IsTerrarium(__instance) && !justCheckingForActivity)
            {
                var sprite = who.currentLocation.temporarySprites.FirstOrDefault(s => s is TerrariumFrogs && (s as TerrariumFrogs).tile == __instance.tileLocation.Value);
                if(sprite is TerrariumFrogs)
                {
                    context.Monitor.Log($"Animating terrarium at tile {__instance.TileLocation}");
                    (sprite as TerrariumFrogs).doAction();
                }
            }
        }

        public static bool Object_isPassable_Prefix(Object __instance, ref bool __result)
        {
            if (__instance.bigCraftable && __instance.Name == "Terrarium" && __instance.modData.ContainsKey("spacechase0.BiggerCraftables/BiggerIndex") && int.Parse(__instance.modData["spacechase0.BiggerCraftables/BiggerIndex"]) >= 6)
            {
                __result = true;
                return false;
            }
            return true;
        }

        private static bool IsTerrarium(Object obj)
        {
            return obj.name.Equals("Terrarium");
        }

        private static Item GetRandomGift(Object obj)
        {
            return null;
        }
        private static async void DelayedShowTerrariums(GameLocation environment)
        {
            await Task.Delay(100);
            ShowFrogs(environment);
        }

        private static void ShowFrogs(GameLocation location, Object excluded = null)
        {
            if (JsonAssets == null)
                return;
            location.temporarySprites.RemoveAll((s) => s is TerrariumFrogs);
            foreach (KeyValuePair<Vector2, Object> kvp in location.objects.Pairs)
            {
                if (!IsTerrarium(kvp.Value) || kvp.Value == excluded || !kvp.Value.modData.ContainsKey("spacechase0.BiggerCraftables/BiggerIndex") || kvp.Value.modData["spacechase0.BiggerCraftables/BiggerIndex"] != "6")
                    continue;

                context.Monitor.Log($"Showing {Config.Frogs} terrarium frogs for tile {kvp.Key}");
                int i = 0;
                while (i++ < Config.Frogs)
                {
                    bool which = Game1.random.NextDouble() > 0.5;
                    Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
                    location.TemporarySprites.Add(new TerrariumFrogs(kvp.Key)
                    {
                        texture = crittersText2,
                        sourceRect = which ? new Rectangle(64, 224, 16, 16) : new Rectangle(64, 240, 16, 16),
                        animationLength = 1,
                        sourceRectStartingPos = which ? new Vector2(64f, 224f) : new Vector2(64f, 240f),
                        interval = Game1.random.Next(100, 200),
                        totalNumberOfLoops = 9999,
                        position = kvp.Key * 64f + new Vector2(((Game1.random.NextDouble() < 0.5) ? 22 : 25), ((Game1.random.NextDouble() < 0.5) ? 2 : 1)) * 4f  + new Vector2(0, 42 + 42 / Config.Frogs * i),
                        scale = 4f,
                        flipped = (Game1.random.NextDouble() < 0.5),
                        layerDepth = (kvp.Key.Y + 2f + 0.11f + 0.01f * i) * 64f / 10000f + 0.005f,
                        Parent = location
                    });
                }
                if (Config.PlaySound != null && Config.PlaySound.Length > 0 && Game1.random.NextDouble() < 0.05 && Game1.timeOfDay > 610)
                {
                    DelayedAction.playSoundAfterDelay(Config.PlaySound, Game1.random.Next(1000,3000), null, -1);
                }
            }
        }
    }
}
