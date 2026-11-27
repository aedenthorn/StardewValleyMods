using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace FishingChestsExpanded
{
	public partial class ModEntry : Mod
	{
		private static ModConfig Config;
		private static IMonitor SMonitor;
		private static IModHelper SHelper;
		private static List<object> treasuresList = new();
		private static IAdvancedLootFrameworkApi advancedLootFrameworkApi;

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			SMonitor = Monitor;
			SHelper = Helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Constructor(typeof(ItemGrabMenu), new Type[] { typeof(IList<Item>), typeof(object) }),
					prefix: new HarmonyMethod(typeof(ItemGrabMenu_Patch), nameof(ItemGrabMenu_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.startMinigameEndFunction)),
					prefix: new HarmonyMethod(typeof(FishingRod_startMinigameEndFunction_Patch), nameof(FishingRod_startMinigameEndFunction_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			advancedLootFrameworkApi = Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
			if (advancedLootFrameworkApi != null)
			{
				Monitor.Log($"Loaded AdvancedLootFramework API", LogLevel.Debug);
				UpdateTreasuresList();
				Monitor.Log($"Got {treasuresList.Count} possible treasures");
			}

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => Helper.WriteConfig(Config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.EnableMod,
				setValue: value => Config.EnableMod = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.VanillaLootChance.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.VanillaLootChance.Tooltip"),
				getValue: () => Config.VanillaLootChance,
				setValue: value => Config.VanillaLootChance = value,
				min: 0,
				max: 100
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AlwaysIncludeRoe.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AlwaysIncludeRoe.Tooltip"),
				getValue: () => Config.AlwaysIncludeRoe,
				setValue: value => Config.AlwaysIncludeRoe = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AlwaysIncludeBooks.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AlwaysIncludeBooks.Tooltip"),
				getValue: () => Config.AlwaysIncludeBooks,
				setValue: value => Config.AlwaysIncludeBooks = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AlwaysIncludeGeodes.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AlwaysIncludeGeodes.Tooltip"),
				getValue: () => Config.AlwaysIncludeGeodes,
				setValue: value => Config.AlwaysIncludeGeodes = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AlwaysIncludeArtifacts.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AlwaysIncludeArtifacts.Tooltip"),
				getValue: () => Config.AlwaysIncludeArtifacts,
				setValue: value => Config.AlwaysIncludeArtifacts = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AlwaysIncludeItems.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AlwaysIncludeItems.Tooltip"),
				getValue: () => Config.AlwaysIncludeItems,
				setValue: value => Config.AlwaysIncludeItems = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ChanceForTreasureChest.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ChanceForTreasureChest.Tooltip"),
				getValue: () => Config.ChanceForTreasureChest,
				setValue: value => Config.ChanceForTreasureChest = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxItems.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxItems.Tooltip"),
				getValue: () => Config.MaxItems,
				setValue: value => Config.MaxItems = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Tooltip"),
				getValue: () => Config.ItemsBaseMaxValue,
				setValue: value => Config.ItemsBaseMaxValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MinItemValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MinItemValue.Tooltip"),
				getValue: () => Config.MinItemValue,
				setValue: value => Config.MinItemValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxItemValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxItemValue.Tooltip"),
				getValue: () => Config.MaxItemValue,
				setValue: value => Config.MaxItemValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Tooltip"),
				getValue: () => Config.CoinBaseMin,
				setValue: value => Config.CoinBaseMin = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Tooltip"),
				getValue: () => Config.CoinBaseMax,
				setValue: value => Config.CoinBaseMax = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.IncreaseRate.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.IncreaseRate.Tooltip"),
				getValue: () => Config.IncreaseRate,
				setValue: value => Config.IncreaseRate = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.ItemListChances.Text"),
				tooltip: () => SHelper.Translation.Get("GMCM.ItemListChances.Tooltip")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesHat.Name"),
				getValue: () => Config.ItemListChances["Hat"],
				setValue: value => {
					Config.ItemListChances["Hat"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesShirt.Name"),
				getValue: () => Config.ItemListChances["Shirt"],
				setValue: value => {
					Config.ItemListChances["Shirt"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesPants.Name"),
				getValue: () => Config.ItemListChances["Pants"],
				setValue: value => {
					Config.ItemListChances["Pants"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBoots.Name"),
				getValue: () => Config.ItemListChances["Boots"],
				setValue: value => {
					Config.ItemListChances["Boots"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesMeleeWeapon.Name"),
				getValue: () => Config.ItemListChances["MeleeWeapon"],
				setValue: value => {
					Config.ItemListChances["MeleeWeapon"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesRing.Name"),
				getValue: () => Config.ItemListChances["Ring"],
				setValue: value => {
					Config.ItemListChances["Ring"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesRelic.Name"),
				getValue: () => Config.ItemListChances["Relic"],
				setValue: value => {
					Config.ItemListChances["Relic"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesMineral.Name"),
				getValue: () => Config.ItemListChances["Mineral"],
				setValue: value => {
					Config.ItemListChances["Mineral"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesCooking.Name"),
				getValue: () => Config.ItemListChances["Cooking"],
				setValue: value => {
					Config.ItemListChances["Cooking"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesFish.Name"),
				getValue: () => Config.ItemListChances["Fish"],
				setValue: value => {
					Config.ItemListChances["Fish"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesSeed.Name"),
				getValue: () => Config.ItemListChances["Seed"],
				setValue: value => {
					Config.ItemListChances["Seed"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBasicObject.Name"),
				getValue: () => Config.ItemListChances["BasicObject"],
				setValue: value => {
					Config.ItemListChances["BasicObject"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBigCraftable.Name"),
				getValue: () => Config.ItemListChances["BigCraftable"],
				setValue: value => {
					Config.ItemListChances["BigCraftable"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
		}

		private static void UpdateTreasuresList()
		{
			treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
		}

		private static IList<Item> GetVanillaLoot(FishingRod fishingRod)
		{
			List<Item> treasures = new();
			float chance = 1f;

			while (Game1.random.NextDouble() <= chance)
			{
				chance *= 0.4f;
				if (Game1.currentSeason.Equals("spring") && fishingRod.getLastFarmerToUse().currentLocation is not Beach && Game1.random.NextDouble() < 0.1)
				{
					treasures.Add(new Object("273", Game1.random.Next(2, 6) + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), false, -1, 0));
				}
				if (fishingRod.numberOfFishCaught > 1 && Game1.random.NextDouble() < 0.5)
				{
					treasures.Add(new Object("774", 2 + ((Game1.random.NextDouble() < 0.25) ? 2 : 0), false, -1, 0));
				}
				switch (Game1.random.Next(4))
				{
					case 0:
						if (fishingRod.clearWaterDistance >= 5 && Game1.random.NextDouble() < 0.03)
						{
							treasures.Add(new Object("386", Game1.random.Next(1, 3), false, -1, 0));
						}
						else
						{
							List<string> possibles = new();

							if (fishingRod.clearWaterDistance >= 4)
							{
								possibles.Add("384");
							}
							if (fishingRod.clearWaterDistance >= 3 && (possibles.Count == 0 || Game1.random.NextDouble() < 0.6))
							{
								possibles.Add("380");
							}
							if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
							{
								possibles.Add("378");
							}
							if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
							{
								possibles.Add("388");
							}
							if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
							{
								possibles.Add("390");
							}
							possibles.Add("382");
							treasures.Add(new Object(possibles.ElementAt(Game1.random.Next(possibles.Count)), Game1.random.Next(2, 7) * ((Game1.random.NextDouble() < 0.05 + fishingRod.getLastFarmerToUse().LuckLevel * 0.015) ? 2 : 1), false, -1, 0));
							if (Game1.random.NextDouble() < 0.05 + fishingRod.getLastFarmerToUse().LuckLevel * 0.03)
							{
								treasures.Last<Item>().Stack *= 2;
							}
						}
						break;
					case 1:
						if (fishingRod.clearWaterDistance >= 4 && Game1.random.NextDouble() < 0.1 && fishingRod.getLastFarmerToUse().FishingLevel >= 6)
						{
							treasures.Add(new Object("687", 1, false, -1, 0));
						}
						else if (Game1.random.NextDouble() < 0.25 && fishingRod.getLastFarmerToUse().craftingRecipes.ContainsKey("Wild Bait"))
						{
							treasures.Add(new Object("774", 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), false, -1, 0));
						}
						else if (fishingRod.getLastFarmerToUse().FishingLevel >= 6)
						{
							treasures.Add(new Object("685", 1, false, -1, 0));
						}
						else
						{
							treasures.Add(new Object("685", 10, false, -1, 0));
						}
						break;
					case 2:
						if (Game1.random.NextDouble() < 0.1 && Game1.netWorldState.Value.LostBooksFound < 21 && fishingRod.getLastFarmerToUse() != null && fishingRod.getLastFarmerToUse().hasOrWillReceiveMail("lostBookFound"))
						{
							treasures.Add(new Object("102", 1, false, -1, 0));
						}
						else if (fishingRod.getLastFarmerToUse().archaeologyFound.Count() > 0)
						{
							if (Game1.random.NextDouble() < 0.25 && fishingRod.getLastFarmerToUse().FishingLevel > 1)
							{
								treasures.Add(new Object(Game1.random.Next(585, 588).ToString(), 1, false, -1, 0));
							}
							else if (Game1.random.NextDouble() < 0.5 && fishingRod.getLastFarmerToUse().FishingLevel > 1)
							{
								treasures.Add(new Object(Game1.random.Next(103, 120).ToString(), 1, false, -1, 0));
							}
							else
							{
								treasures.Add(new Object("535", 1, false, -1, 0));
							}
						}
						else
						{
							treasures.Add(new Object("382", Game1.random.Next(1, 3), false, -1, 0));
						}
						break;
					case 3:
						switch (Game1.random.Next(3))
						{
							case 0:
								if (fishingRod.clearWaterDistance >= 4)
								{
									treasures.Add(new Object((537 + ((Game1.random.NextDouble() < 0.4) ? Game1.random.Next(-2, 0) : 0)).ToString(), Game1.random.Next(1, 4), false, -1, 0));
								}
								else if (fishingRod.clearWaterDistance >= 3)
								{
									treasures.Add(new Object((536 + ((Game1.random.NextDouble() < 0.4) ? -1 : 0)).ToString(), Game1.random.Next(1, 4), false, -1, 0));
								}
								else
								{
									treasures.Add(new Object("535", Game1.random.Next(1, 4), false, -1, 0));
								}
								if (Game1.random.NextDouble() < 0.05 + fishingRod.getLastFarmerToUse().LuckLevel * 0.03)
								{
									treasures.Last<Item>().Stack *= 2;
								}
								break;
							case 1:
								if (fishingRod.getLastFarmerToUse().FishingLevel < 2)
								{
									treasures.Add(new Object("382", Game1.random.Next(1, 4), false, -1, 0));
								}
								else
								{
									if (fishingRod.clearWaterDistance >= 4)
									{
										treasures.Add(new Object(((Game1.random.NextDouble() < 0.3) ? 82 : ((Game1.random.NextDouble() < 0.5) ? 64 : 60)).ToString(), Game1.random.Next(1, 3), false, -1, 0));
									}
									else if (fishingRod.clearWaterDistance >= 3)
									{
										treasures.Add(new Object(((Game1.random.NextDouble() < 0.3) ? 84 : ((Game1.random.NextDouble() < 0.5) ? 70 : 62)).ToString(), Game1.random.Next(1, 3), false, -1, 0));
									}
									else
									{
										treasures.Add(new Object(((Game1.random.NextDouble() < 0.3) ? 86 : ((Game1.random.NextDouble() < 0.5) ? 66 : 68)).ToString(), Game1.random.Next(1, 3), false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.028 * (fishingRod.clearWaterDistance / 5f))
									{
										treasures.Add(new Object("72", 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.05)
									{
										treasures.Last<Item>().Stack *= 2;
									}
								}
								break;
							case 2:
								if (fishingRod.getLastFarmerToUse().FishingLevel < 2)
								{
									treasures.Add(new Object("770", Game1.random.Next(1, 4), false, -1, 0));
								}
								else
								{
									float luckModifier = (1f + (float)fishingRod.getLastFarmerToUse().DailyLuck) * (fishingRod.clearWaterDistance / 5f);

									if (Game1.random.NextDouble() < 0.05 * luckModifier && !fishingRod.getLastFarmerToUse().specialItems.Contains("14"))
									{
										treasures.Add(new MeleeWeapon("14")
										{
											specialItem = true
										});
									}
									if (Game1.random.NextDouble() < 0.05 * luckModifier && !fishingRod.getLastFarmerToUse().specialItems.Contains("51"))
									{
										treasures.Add(new MeleeWeapon("51")
										{
											specialItem = true
										});
									}
									if (Game1.random.NextDouble() < 0.07 * luckModifier)
									{
										switch (Game1.random.Next(3))
										{
											case 0:
												treasures.Add(new Ring((516 + ((Game1.random.NextDouble() < fishingRod.getLastFarmerToUse().LuckLevel / 11f) ? 1 : 0)).ToString()));
												break;
											case 1:
												treasures.Add(new Ring((518 + ((Game1.random.NextDouble() < fishingRod.getLastFarmerToUse().LuckLevel / 11f) ? 1 : 0)).ToString()));
												break;
											case 2:
												treasures.Add(new Ring(Game1.random.Next(529, 535).ToString()));
												break;
										}
									}
									if (Game1.random.NextDouble() < 0.02 * luckModifier)
									{
										treasures.Add(new Object("166", 1, false, -1, 0));
									}
									if (fishingRod.getLastFarmerToUse().FishingLevel > 5 && Game1.random.NextDouble() < 0.001 * luckModifier)
									{
										treasures.Add(new Object("74", 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Object("127", 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Object("126", 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Ring("527"));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Boots(Game1.random.Next(504, 514).ToString()));
									}
									if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Object("928", 1, false, -1, 0));
									}
									if (treasures.Count == 1)
									{
										treasures.Add(new Object("72", 1, false, -1, 0));
									}
								}
								break;
						}
						break;
				}
			}
			if (treasures.Count == 0)
			{
				treasures.Add(new Object("685", Game1.random.Next(1, 4) * 5, false, -1, 0));
			}
			return treasures;
		}
	}
}
