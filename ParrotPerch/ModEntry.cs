using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace ParrotPerch
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IJsonAssetsApi JsonAssets = null;
        private static Vector2 parrotOffset = new Vector2(4, -26) * 4;
        private static IAdvancedLootFrameworkApi advancedLootFrameworkApi = null;
        private static List<object> giftList = new List<object>();
        private static Dictionary<string, int> possibleGifts = new Dictionary<string, int>()
        {
            { "Seed", 100 },
            { "BasicObject", 50 },
            { "Fish", 10 },
            { "Cooking", 20 },
            { "Relic", 5 }
        };
        private static int[] fertilizers = new int[] { 368, 369, 919 };

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

        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            ShowParrots(Game1.player.currentLocation);
        }

        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for Parrot Perch");
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "json-assets"));
            }
            advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi != null)
            {
                Monitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
                giftList = advancedLootFrameworkApi.LoadPossibleTreasures(possibleGifts.Keys.ToArray(), -1, 100);
                Monitor.Log($"Got {giftList.Count} possible treasures");
            }

        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            ShowParrots(e.NewLocation);
        }

        private static void placementAction_Postfix(Object __instance, GameLocation location)
        {
            if(__instance.bigCraftable && IsPerch(__instance))
            {
                ShowParrots(location);
            }
        }
        private static void performRemoveAction_Postfix(Object __instance, GameLocation environment)
        {
            if (__instance.bigCraftable && IsPerch(__instance))
            {
                Game1.playSound("parrot");
                ShowParrots(environment, __instance);
            }
        }
        private static void checkForAction_Prefix(Object __instance, Farmer who, bool justCheckingForActivity)
        {
            if (__instance.bigCraftable && IsPerch(__instance) && !justCheckingForActivity)
            {
                var sprite = who.currentLocation.temporarySprites.FirstOrDefault(s => s is PerchParrot && (s as PerchParrot).tile == __instance.tileLocation.Value);
                if(sprite is PerchParrot)
                {
                    context.Monitor.Log($"Animating perch parrot for tile {__instance.TileLocation}");
                    (sprite as PerchParrot).doAction();
                    if(who.CurrentItem is Object && (who.CurrentItem as Object).type.Value.Contains("Seed"))
                    {
                        if (Game1.random.NextDouble() < Config.DropGiftChance)
                        {
                            if (advancedLootFrameworkApi != null)
                                who.currentLocation.debris.Add(new Debris(GetRandomParrotGift(who.CurrentItem as Object), __instance.tileLocation.Value * 64f + new Vector2(32f, -32f)));
                            else
                            {
                                Object obj = new Object(fertilizers[Game1.random.Next(fertilizers.Length)], 1);
                            }
                        }
                        context.Monitor.Log($"giving seed to {__instance.TileLocation}");
                        who.CurrentItem.Stack--;
                    }
                }
            }
        }

        private static bool IsPerch(Object obj)
        {
            return obj.name.EndsWith("Parrot Perch");
        }

        private static Item GetRandomParrotGift(Object obj)
        {
            return advancedLootFrameworkApi.GetChestItems(giftList, possibleGifts, 1, 1, 100, obj.Price > 0 ? obj.Price : 1, 0.2f, 100)[0];
        }

        private static void ShowParrots(GameLocation location, Object excluded = null)
        {
            if (JsonAssets == null)
                return;
            //context.Monitor.Log($"Showing perch parrots for {location.Name}");
            location.temporarySprites.RemoveAll((s) => s is PerchParrot);
            foreach (KeyValuePair<Vector2, Object> kvp in location.objects.Pairs)
            {
                if (IsPerch(kvp.Value) && kvp.Value != excluded)
                {
                    context.Monitor.Log($"Showing parrot for tile {kvp.Key}");
                    location.temporarySprites.Add(new PerchParrot(kvp.Key * 64 + parrotOffset, kvp.Value.tileLocation));
                }
            }
        }

    }
}
