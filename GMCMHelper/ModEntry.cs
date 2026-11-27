using StardewModdingAPI;

namespace GMCMHelper
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModEntry context;


        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;
            context = this;
        }

        public override object GetApi()
        {
            return new GMCMHelperAPI();
        }
    }
}