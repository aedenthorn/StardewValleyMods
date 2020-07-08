using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Familiars
{
    public class FamiliarsHelperEvents
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }
        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // load scuba gear

            ModEntry.JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            bool flag = ModEntry.JsonAssets == null;
            if (flag)
            {
                Monitor.Log("Can't load Json Assets API for Familiars mod");
            }
            else
            {
                ModEntry.JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }

        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // load scuba gear ids

            if (ModEntry.JsonAssets != null)
            {
                ModEntry.BatFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Bat Familiar Egg");
                ModEntry.DustFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Dust Sprite Familiar Egg");
                ModEntry.DinoFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Dino Familiar Egg");

                if (ModEntry.BatFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #1. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #1 ID is {0}.", ModEntry.BatFamiliarEgg));
                }
                if (ModEntry.DustFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #2. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #2 ID is {0}.", ModEntry.DustFamiliarEgg));
                }
                if (ModEntry.DinoFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #3. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #3 ID is {0}.", ModEntry.DinoFamiliarEgg));
                }
            }
        }

    }
}
