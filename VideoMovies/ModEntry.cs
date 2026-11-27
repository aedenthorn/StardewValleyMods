extern alias xnavid;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using Video = xnavid::Microsoft.Xna.Framework.Media.Video;
using VideoPlayer = xnavid::Microsoft.Xna.Framework.Media.VideoPlayer;

namespace VideoMovies
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public const string dictPath = "aedenthorn.VideoMovies/dict";
        public static VideoPlayer videoPlayer = new VideoPlayer();
        public static Texture2D lastTexture;
        public static List<Video> currentVideos = new List<Video>();
        public static Dictionary<string, VideoData> Videos
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, VideoData>>(dictPath);
            }
        }

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }



        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                // Main section
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
            }
		}
	}

}
