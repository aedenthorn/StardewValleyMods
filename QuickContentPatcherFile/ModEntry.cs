using StardewModdingAPI;

namespace QuickContentPatcherFile
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;
        }

        public override object GetApi()
        {
            return new QCPFAPI();
        }
    }
}