using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Events;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;

namespace MeteorDefence
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

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

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.setUp)),
					prefix: new HarmonyMethod(typeof(SoundInTheNightEvent_setUp_Patch), nameof(SoundInTheNightEvent_setUp_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.makeChangesToLocation)),
					prefix: new HarmonyMethod(typeof(SoundInTheNightEvent_makeChangesToLocation_Patch), nameof(SoundInTheNightEvent_makeChangesToLocation_Patch.Prefix)),
					postfix: new HarmonyMethod(typeof(SoundInTheNightEvent_makeChangesToLocation_Patch), nameof(SoundInTheNightEvent_makeChangesToLocation_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.tickUpdate)),
					prefix: new HarmonyMethod(typeof(SoundInTheNightEvent_tickUpdate_Patch), nameof(SoundInTheNightEvent_tickUpdate_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (e.Name.IsEquivalentTo("Data/CraftingRecipes"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add($"{ModManifest.UniqueID}_SpaceLaser", $"60 1 335 5 787 1/Home/{ModManifest.UniqueID}_SpaceLaser/true/{Config.SkillsRequired}/");
				});
			}
			if (e.Name.IsEquivalentTo("Data/BigCraftables"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, BigCraftableData> data = asset.AsDictionary<string, BigCraftableData>().Data;

					data.Add($"{ModManifest.UniqueID}_SpaceLaser", new BigCraftableData()
					{
						Name = "Space Laser",
						DisplayName = $"[{ModManifest.UniqueID}_i18n item.space-laser.name]",
						Description = $"[{ModManifest.UniqueID}_i18n item.space-laser.description]",
						Texture = SHelper.ModContent.GetInternalAssetName("assets/spaceLaser").Name,
						Price = 1000
					});
				});
			}
			if (e.Name.IsEquivalentTo("Data/Shops"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, ShopData> data = asset.AsDictionary<string, ShopData>().Data;

					data["Carpenter"].Items.Add(new ShopItemData()
					{
						Id = $"{ModManifest.UniqueID}_SpaceLaser",
						ItemId = $"(BC){ModManifest.UniqueID}_SpaceLaser",
						Price = 5000,
						AvailableStock = 1,
						AvailableStockLimit = LimitedStockMode.Global
					});
				});
			}
			if (e.Name.IsEquivalentTo("Data/AudioChanges"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, AudioCueData> data = asset.AsDictionary<string, AudioCueData>().Data;

					data.Add($"{ModManifest.UniqueID}_flyingMeteorite", new AudioCueData(){
						Id = $"{ModManifest.UniqueID}_flyingMeteorite",
						FilePaths = new List<string>() { $"{SHelper.DirectoryPath}/assets/flyingMeteorite.wav" },
						Category = "Sound"
					});
					data.Add($"{ModManifest.UniqueID}_meteoriteImpact", new AudioCueData(){
						Id = $"{ModManifest.UniqueID}_meteoriteImpact",
						FilePaths = new List<string>() { $"{SHelper.DirectoryPath}/assets/meteoriteImpact.wav" },
						Category = "Sound"
					});
				});
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			TokensUtility.Register();

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => {
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/Shops"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/AudioChanges"));
					Helper.WriteConfig(Config);
				}
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.StrikeAnywhere.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.StrikeAnywhere.Tooltip"),
				getValue: () => Config.StrikeAnywhere,
				setValue: value => Config.StrikeAnywhere = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.MinimumMeteorites.Name"),
				getValue: () => Config.MinimumMeteorites,
				setValue: value => {
					value = Math.Clamp(value, 0, 50);
					Config.MinimumMeteorites = value;
				}
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.MaximumMeteorites.Name"),
				getValue: () => Config.MaximumMeteorites,
				setValue: value => {
					value = Math.Clamp(value, 0, 50);
					Config.MaximumMeteorites = value;
				}
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.MeteoritesDestroyedPerObject.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.MeteoritesDestroyedPerObject.Tooltip"),
				getValue: () => Config.MeteoritesDestroyedPerObject,
				setValue: value => {
					value = Math.Clamp(value, -1, 50);
					Config.MeteoritesDestroyedPerObject = value;
				}
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.DefenceSound.Name"),
				getValue: () => Config.DefenceSound,
				setValue: value => Config.DefenceSound = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.DestructionSound.Name"),
				getValue: () => Config.DestructionSound,
				setValue: value => Config.DestructionSound = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ImpactSound.Name"),
				getValue: () => Config.ImpactSound,
				setValue: value => Config.ImpactSound = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.SkillsRequired.Name"),
				tooltip: () => Helper.Translation.Get("GMCM.SkillsRequired.Tooltip"),
				getValue: () => Config.SkillsRequired,
				setValue: value => Config.SkillsRequired = value
			);
		}
	}

}
