using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
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

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

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
                Monitor.Log("Can't load Json Assets API for Placeable Mine Shaft", 0);
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }
        }

        private static bool checkAction_Prefix(GameLocation __instance, Location tileLocation, Farmer who, ref bool __result)
        {
            if (who.CurrentItem.Name != "Instant Growth Powder")
                return true;

            if(__instance.isCropAtTile(tileLocation.X, tileLocation.Y))
            {
                (__instance.terrainFeatures[new Vector2(tileLocation.X, tileLocation.Y)] as HoeDirt).crop.growCompletely();
                who.CurrentItem.Stack--;
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
                    who.CurrentItem.Stack--;
                    __instance.playSound("yoba");
                    return true;
                }
            }

            foreach (KeyValuePair<Vector2, TerrainFeature> v in __instance.terrainFeatures.Pairs)
            {
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is Tree && (v.Value as Tree).growthStage < 5)
                {
                    Tree tree = v.Value as Tree;
                    tree.growthStage.Value = 5;
                    __instance.terrainFeatures[v.Key] = tree;

                    who.CurrentItem.Stack--;
                    __instance.playSound("yoba");
                    return true;
                }
            }
            
            return true;
        }
    }
}