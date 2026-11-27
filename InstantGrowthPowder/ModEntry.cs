using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.IO;
using xTile.Dimensions;

namespace InstantGrowthPowder
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private IJsonAssetsApi JsonAssets;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            SMonitor = Monitor;
            SHelper = Helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.checkAction_Prefix))
            );
        }

        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for Instant Growth Powder", 0);
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }
        }

        private static bool checkAction_Prefix(GameLocation __instance, Location tileLocation, Farmer who, ref bool __result)
        {
            if (who?.CurrentItem == null || who.CurrentItem.Name != "Instant Growth Powder")
                return true;

            if(__instance.isCropAtTile(tileLocation.X, tileLocation.Y))
            {
                if (!(__instance.terrainFeatures[new Vector2(tileLocation.X, tileLocation.Y)] as HoeDirt).crop.fullyGrown.Value)
                    (__instance.terrainFeatures[new Vector2(tileLocation.X, tileLocation.Y)] as HoeDirt).crop.growCompletely();
                else if ((__instance.terrainFeatures[new Vector2(tileLocation.X, tileLocation.Y)] as HoeDirt).crop.regrowAfterHarvest.Value >= 0 && (__instance.terrainFeatures[new Vector2(tileLocation.X, tileLocation.Y)] as HoeDirt).crop.dayOfCurrentPhase.Value > 0)
                    (__instance.terrainFeatures[new Vector2(tileLocation.X, tileLocation.Y)] as HoeDirt).crop.dayOfCurrentPhase.Value = 0;
                else
                    return true;

                who.reduceActiveItemByOne();
                __instance.playSound("yoba");
                __result = true;
                return false;
            }
            Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);

            foreach (NPC i in __instance.characters)
            {
                if (i != null && i is Child && i.GetBoundingBox().Intersects(tileRect) && (i.Age < 3))
                {
                    i.Age = 3;
                    (i as Child).daysOld.Value = 55;
                    i.speed = 4;
                    i.reloadSprite();

                    who.reduceActiveItemByOne();
                    __instance.playSound("yoba");
                    __result = true;
                    return false;
                }
            }

            try
            {
                NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> dict = SHelper.Reflection.GetField<NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>(__instance, "animals").GetValue();
                foreach (KeyValuePair<long, FarmAnimal> i in dict.Pairs)
                {
                    if (i.Value.GetCursorPetBoundingBox().Intersects(tileRect))
                    {
                        i.Value.age.Value = i.Value.ageWhenMature.Value;
                        i.Value.reloadData();
                        if (i.Value.type.Value.Contains("Sheep"))
                        {
                            i.Value.currentProduce.Value = i.Value.defaultProduceIndex.Value;
                        }
                        i.Value.daysSinceLastLay.Value = 99;

                        who.reduceActiveItemByOne();
                        __instance.playSound("yoba");
                        __result = true;
                        return false;
                    }
                }
            }
            catch { }


            foreach (KeyValuePair<Vector2, TerrainFeature> v in __instance.terrainFeatures.Pairs)
            {
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is Tree && (v.Value as Tree).growthStage.Value < 5)
                {
                    (__instance.terrainFeatures[v.Key] as Tree).growthStage.Value = 5;
                    (__instance.terrainFeatures[v.Key] as Tree).fertilized.Value = true;
                    (__instance.terrainFeatures[v.Key] as Tree).dayUpdate(Game1.currentLocation, v.Key);
                    (__instance.terrainFeatures[v.Key] as Tree).fertilized.Value = false;

                    who.reduceActiveItemByOne();
                    __instance.playSound("yoba");
                    __result = true;
                    return false;
                }
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is FruitTree && (v.Value as FruitTree).growthStage.Value < 4)
                {
                    FruitTree tree = v.Value as FruitTree; 
                    tree.daysUntilMature.Value = 0;
                    tree.growthStage.Value = 4;
                    __instance.terrainFeatures[v.Key] = tree;

                    who.reduceActiveItemByOne();
                    __instance.playSound("yoba");
                    __result = true;
                    return false;
                }
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is Bush && (v.Value as Bush).size.Value == 3 && (v.Value as Bush).getAge() < 20)
                {
                    Bush bush = v.Value as Bush;
                    bush.datePlanted.Value -= 20 - bush.getAge();
                    Game1.currentLocation.terrainFeatures[v.Key] = bush;
                    Game1.currentLocation.terrainFeatures[v.Key].loadSprite();

                    who.reduceActiveItemByOne();
                    __instance.playSound("yoba");
                    __result = true;
                    return false;
                }
            }
            
            foreach (KeyValuePair<Vector2, Object> v in __instance.objects.Pairs)
            {
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is IndoorPot && (v.Value as IndoorPot).hoeDirt.Value?.crop != null)
                {
                    if (!(v.Value as IndoorPot).hoeDirt.Value.crop.fullyGrown.Value)
                        (v.Value as IndoorPot).hoeDirt.Value.crop.growCompletely();
                    else if ((v.Value as IndoorPot).hoeDirt.Value.crop.regrowAfterHarvest.Value >= 0 && (v.Value as IndoorPot).hoeDirt.Value.crop.dayOfCurrentPhase.Value > 0)
                        (v.Value as IndoorPot).hoeDirt.Value.crop.dayOfCurrentPhase.Value = 0;
                    else 
                        return true;

                    who.reduceActiveItemByOne();
                    __instance.playSound("yoba");
                    __result = true;
                    return false;

                }
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is IndoorPot && (v.Value as IndoorPot).bush.Value?.getAge() < 20)
                {
                    (v.Value as IndoorPot).bush.Value.datePlanted.Value -= 20 - (v.Value as IndoorPot).bush.Value.getAge();
                    (v.Value as IndoorPot).bush.Value.loadSprite();

                    who.reduceActiveItemByOne();
                    __instance.playSound("yoba");
                    __result = true;
                    return false;

                }
            }
            
            return true;
        }
    }
}