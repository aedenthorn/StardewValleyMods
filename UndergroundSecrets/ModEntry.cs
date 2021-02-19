using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System.IO;

namespace UndergroundSecrets
{
    public class ModEntry : Mod
    {

        public static ModEntry context;

        internal static ModConfig Config;
        public static string tileSheetPath;
        internal static string tileSheetId = "z_underground_secrets";
        public static ITreasureChestsExpandedApi treasureChestsExpandedApi = null;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = this.Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            tileSheetPath = this.Helper.Content.GetActualAssetKey(Path.Combine("assets","underground_secrets.png"), ContentSource.ModFolder);

            HelperEvents.Initialize(Helper, Monitor, Config);
            UndergroundPatches.Initialize(Helper, Monitor, Config);
            Utils.Initialize(Helper, Monitor, Config);

            TilePuzzles.Initialize(Helper, Monitor, Config);
            OfferingPuzzles.Initialize(Helper, Monitor, Config);
            LightPuzzles.Initialize(Helper, Monitor, Config);
            Altars.Initialize(Helper, Monitor, Config);
            RiddlePuzzles.Initialize(Helper, Monitor, Config);
            CollapsingFloors.Initialize(Helper, Monitor, Config);
            Traps.Initialize(Helper, Monitor, Config);
            MushroomTrees.Initialize(Helper, Monitor, Config);

            //Helper.Events.Player.Warped += HelperEvents.Player_Warped;
            Helper.Events.GameLoop.UpdateTicked += HelperEvents.GameLoop_UpdateTicked;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched; 

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction)),
                prefix: new HarmonyMethod(typeof(UndergroundPatches), nameof(UndergroundPatches.GameLocation_performTouchAction_prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.loadMap)),
                postfix: new HarmonyMethod(typeof(UndergroundPatches), nameof(UndergroundPatches.GameLocation_loadMap_postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "populateLevel"),
                transpiler: new HarmonyMethod(typeof(UndergroundPatches), nameof(UndergroundPatches.MineShaft_populateLevel_transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "checkAction"),
                prefix: new HarmonyMethod(typeof(UndergroundPatches), nameof(UndergroundPatches.MineShaft_checkAction_prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "addLevelChests"),
                prefix: new HarmonyMethod(typeof(UndergroundPatches), nameof(UndergroundPatches.MineShaft_addLevelChests_prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.enterMineShaft)),
                postfix: new HarmonyMethod(typeof(UndergroundPatches), nameof(UndergroundPatches.MineShaft_enterMineShaft_postfix))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            treasureChestsExpandedApi = context.Helper.ModRegistry.GetApi<ITreasureChestsExpandedApi>("aedenthorn.TreasureChestsExpanded");
            if (treasureChestsExpandedApi != null)
            {
                Monitor.Log($"loaded TreasureChestsExpanded API", LogLevel.Debug);
            }
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            if (asset.AssetName.EndsWith("underground_secrets"))
            {
                Monitor.Log($"can load underground_secrets");
                return true;
            }

            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)Helper.Content.Load<Texture2D>(Path.Combine("assets", "underground_secrets.png"));

        }
    }
}