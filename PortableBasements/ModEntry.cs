using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Locations;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace PortableBasements
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;
        internal static string modKey = "aedenthorn.PortableBasements";
        internal static string ladderDownKey = "aedenthorn.PortableBasements_ladderDown";
        internal static string ladderUpKey = "aedenthorn.PortableBasements_ladderUp";
        internal static string ladderKey = "aedenthorn.PortableBasements_ladder";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
			
            Harmony harmony = new(ModManifest.UniqueID);


            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                postfix: new(typeof(ModEntry), nameof(Object_placementAction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
                postfix: new(typeof(ModEntry), nameof(Object_checkForAction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
                postfix: new(typeof(ModEntry), nameof(Object_performRemoveAction_Postfix))
            );
        }

        private void Content_AssetRequested(object? sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, BigCraftableData>().Data;
                    dict[ladderDownKey] = new BigCraftableData()
                    {
                        Name = ladderDownKey,
                        DisplayName = SHelper.Translation.Get("LadderDown.Name"),
                        Description = SHelper.Translation.Get("LadderDown.Desc"),
                        Fragility = 0,
                        CanBePlacedIndoors = true,
                        CanBePlacedOutdoors = true,
                        Texture = ladderKey,
                        SpriteIndex = 0
                    };
                    dict[ladderUpKey] = new BigCraftableData()
                    {
                        Name = ladderUpKey,
                        DisplayName = SHelper.Translation.Get("LadderUp.Name"),
                        Description = SHelper.Translation.Get("LadderUp.Desc"),
                        Fragility = 2,
                        CanBePlacedIndoors = true,
                        CanBePlacedOutdoors = true,
                        Texture = ladderKey,
                        SpriteIndex = 1
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, LocationData>().Data;
                    dict[$"Portable_Basement"] = new()
                    {
                        DisplayName = "Basement",
                        DefaultArrivalTile = new(6, 6),
                        CreateOnLoad = new ()
                        {
                            MapPath = "Maps/Mines/4"
                        },
                        Music = new()
                        {
                            new()
                            {
                                Track = "Volcano_Ambient"
                            }
                        }
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                    data[ladderDownKey] = $"{Config.LadderCost}/Home/{ladderDownKey}/true/{(string.IsNullOrEmpty(Config.SkillReq) ? "null" : $"s {Config.SkillReq}")}/";
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(ladderKey))
            {
                e.LoadFromModFile<Texture2D>("assets/ladder.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is not null)
			{

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.LadderCost.Name"),
                    getValue: () => Config.LadderCost,
                    setValue: value => Config.LadderCost = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.SkillReq.Name"),
                    getValue: () => Config.SkillReq,
                    setValue: value => Config.SkillReq = value
                );
            }

		}
	}
}
