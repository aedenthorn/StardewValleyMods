using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.HomeRenovations;

namespace InstantBuildingConstructionAndUpgrade
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
					original: AccessTools.Method(typeof(GameLocation), "houseUpgradeAccept"),
					prefix: new HarmonyMethod(typeof(GameLocation_houseUpgradeAccept_Patch), nameof(GameLocation_houseUpgradeAccept_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), "communityUpgradeAccept"),
					prefix: new HarmonyMethod(typeof(GameLocation_communityUpgradeAccept_Patch), nameof(GameLocation_communityUpgradeAccept_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.buildStructure), new Type[] { typeof(Building), typeof(Vector2), typeof(Farmer), typeof(bool) }),
					postfix: new HarmonyMethod(typeof(GameLocation_buildStructure_Patch), nameof(GameLocation_buildStructure_Patch.Postfix1))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.buildStructure), new Type[] { typeof(string), typeof(BuildingData), typeof(Vector2), typeof(Farmer), typeof(Building).MakeByRefType(), typeof(bool), typeof(bool) }),
					postfix: new HarmonyMethod(typeof(GameLocation_buildStructure_Patch), nameof(GameLocation_buildStructure_Patch.Postfix2))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Building), nameof(Building.FinishConstruction)),
					postfix: new HarmonyMethod(typeof(Building_FinishConstruction_Patch), nameof(Building_FinishConstruction_Patch.Postfix))
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

			if (e.Name.IsEquivalentTo("Data/Buildings"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, BuildingData> data = asset.AsDictionary<string, BuildingData>().Data;

					foreach (BuildingData buildingData in data.Values)
					{
						if (Config.InstantBuildingConstructionAndUpgrade)
						{
							buildingData.BuildDays = 0;
						}
						if (Config.FreeConstructionAndUpgrade)
						{
							buildingData.BuildCost = 0;
							buildingData.BuildMaterials = new();
						}
					}
				}, AssetEditPriority.Late);
			}
			if (Config.FreeConstructionAndUpgrade)
			{
				if (e.Name.IsEquivalentTo("Data/HomeRenovations"))
				{
					e.Edit(asset =>
					{
						IDictionary<string, HomeRenovation> data = asset.AsDictionary<string, HomeRenovation>().Data;

						foreach (HomeRenovation homeRenovation in data.Values)
						{
							homeRenovation.Price = 0;
						}
					}, AssetEditPriority.Late);
				}
				if (e.NameWithoutLocale.IsEquivalentTo("Strings/Locations"))
				{
					e.Edit(asset =>
					{
						IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

						foreach (string key in data.Keys)
						{
							if (key.Equals("ScienceHouse_Carpenter_UpgradeHouse1"))
							{
								data[key] = data[key].Replace("10,000", "0").Replace("10.000", "0").Replace("10 000", "0").Replace("10000", "0").Replace("450", "0");
							}
							else if (key.Equals("ScienceHouse_Carpenter_UpgradeHouse2"))
							{
								data[key] = data[key].Replace("65,000", "0").Replace("65.000", "0").Replace("65 000", "0").Replace("65000", "0").Replace("{0}", "0").Replace("100", "0").Replace("{1}", "0");
							}
							else if (key.Equals("ScienceHouse_Carpenter_UpgradeHouse3"))
							{
								data[key] = data[key].Replace("100,000", "0").Replace("100.000", "0").Replace("100 000", "0").Replace("100000", "0");
							}
							else if (key.Equals("ScienceHouse_Carpenter_CommunityUpgrade1"))
							{
								data[key] = data[key].Replace("500,000", "0").Replace("500.000", "0").Replace("500 000", "0").Replace("500000", "0").Replace("950", "0");;
							}
							else if (key.Equals("ScienceHouse_Carpenter_CommunityUpgrade2"))
							{
								data[key] = data[key].Replace("300,000", "0").Replace("300.000", "0").Replace("300 000", "0").Replace("300000", "0");
							}
						}
					});
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			RegisterConsoleCommands();

			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => {
						if (CompatibilityUtility.IsSolidFoundationsLoaded)
						{
							ExecuteCommand("sf_reload");
						}
						InvalidateCaches();
						Helper.WriteConfig(Config);
					}
				);

				// Main section
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.InstantBuildingConstructionAndUpgrade.Name"),
					getValue: () => Config.InstantBuildingConstructionAndUpgrade,
					setValue: value => Config.InstantBuildingConstructionAndUpgrade = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.InstantFarmhouseUpgrade.Name"),
					getValue: () => Config.InstantFarmhouseUpgrade,
					setValue: value => Config.InstantFarmhouseUpgrade = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.InstantCommunityUpgrade.Name"),
					getValue: () => Config.InstantCommunityUpgrade,
					setValue: value => Config.InstantCommunityUpgrade = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.FreeConstructionAndUpgrade.Name"),
					getValue: () => Config.FreeConstructionAndUpgrade,
					setValue: value => Config.FreeConstructionAndUpgrade = value
				);
			}
		}
	}
}
