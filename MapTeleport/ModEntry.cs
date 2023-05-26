using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.IO;

namespace MapTeleport
{
    public partial class ModEntry : Mod 
    {
        public static ModEntry context;
        public static ModConfig Config;
        public static IModHelper SHelper;
        public static IMonitor SMonitor;
        private static bool isSVE;
        private Harmony harmony;

        public static string dictPath = "aedenthorn.MapTeleport/coordinates";

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            context = this;
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Content.AssetRequested += Content_AssetRequested;
            isSVE = Helper.ModRegistry.IsLoaded("FlashShifter.SVECode");

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                if(File.Exists(Path.Combine(SHelper.DirectoryPath, "coordinates.json")))
                {
                    e.LoadFromModFile<CoordinatesList>("coordinates.json", AssetLoadPriority.Exclusive);
                }
                else
                {
                    CoordinatesList coordinatesList = new CoordinatesList();
                    if (isSVE)
                    {
                        coordinatesList = Helper.Data.ReadJsonFile<CoordinatesList>("assets/sve_coordinates.json");
                    }
                    else
                    {
                        coordinatesList = Helper.Data.ReadJsonFile<CoordinatesList>("assets/coordinates.json");
                    }
                    e.LoadFrom( ()=> coordinatesList, AssetLoadPriority.Exclusive);
                    Helper.Data.WriteJsonFile("coordinates.json", coordinatesList);
                }
            }
        }

    }
}
