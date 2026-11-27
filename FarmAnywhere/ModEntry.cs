using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace FarmAnywhere
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            SMonitor = Monitor;
            SHelper = Helper;

            Helper.Events.Player.Warped += Player_Warped;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlantSeedsHere)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CanPlantSeedsHere_Prefix))
            );
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if(Config.AllowIndoorFarming || e.NewLocation.IsOutdoors)
            {
                GameLocation l = e.NewLocation;

                Monitor.Log($"Setting diggable in {l.name}");
                for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                    {
                        bool water = false;
                        try { water = l.waterTiles[x, y]; } catch { }
                        if (!l.isTileOccupiedForPlacement(new Vector2(x, y)) && !water)
                            l.setTileProperty(x, y, "Back", "Diggable", "T");
                        if (l.isCropAtTile(x,y))
                            (l.terrainFeatures[new Vector2(x, y)] as HoeDirt).crop.growCompletely();
                    }
                }
            }
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
        }



        private static bool CanPlantSeedsHere_Prefix(GameLocation __instance, ref bool __result)
        {
            __result = Config.AllowIndoorFarming || __instance.IsOutdoors;
            return false;
        }
    }
}