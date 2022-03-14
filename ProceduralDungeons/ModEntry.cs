using StardewModdingAPI;
using System.Text.RegularExpressions;

namespace ProceduralDungeons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            string assetName = asset.AssetName;
            Regex rgx = new Regex(@"^Maps/Mines/[0-9]{1,2}$");

            if (rgx.IsMatch(assetName))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading Map");

            return (T)(object)GetRandomMap();
        }



        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;
            SMonitor = Monitor;
            SHelper = helper;
        }
    }
}