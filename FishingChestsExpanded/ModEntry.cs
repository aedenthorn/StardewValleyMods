using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace FishingChestsExpanded
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        private static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private static List<object> treasuresList = new List<object>();
        private static IAdvancedLootFrameworkApi advancedLootFrameworkApi;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            ConstructorInfo ci = typeof(ItemGrabMenu).GetConstructor(new Type[] { typeof(IList<Item>), typeof(object) });
            harmony.Patch(
				original: ci,
				prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ItemGrabMenu_Prefix))
            );
            harmony.Patch(
				original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.startMinigameEndFunction)),
				prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.startMinigameEndFunction_Prefix))
			);

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi != null)
            {
                Monitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
            }
            treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
            Monitor.Log($"Got {treasuresList.Count} possible treasures");
        }

        private static void startMinigameEndFunction_Prefix() 
        {
            if(Config.ChanceForTreasureChest >= 0)
                FishingRod.baseChanceForTreasure = Config.ChanceForTreasureChest / 100f;
        }

        private static void ItemGrabMenu_Prefix(ref IList<Item> inventory, object context)
        {
            if (!(context is FishingRod))
                return;

            FishingRod fr = context as FishingRod;
            int fish = SHelper.Reflection.GetField<int>(fr, "whichFish").GetValue();
            bool treasure = false;
            foreach (Item item in inventory)
            {
                if (item.parentSheetIndex != fish)
                    treasure = true;
            }
            if (!treasure)
                return;

            Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            int difficulty = 5;
            if(data.ContainsKey(fish))
                int.TryParse(data[fish].Split('/')[1], out difficulty);

            int coins = advancedLootFrameworkApi.GetChestCoins(difficulty, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax);

			IList<Item> items = advancedLootFrameworkApi.GetChestItems(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, difficulty, Config.IncreaseRate, Config.ItemsBaseMaxValue);

			bool vanilla = Game1.random.NextDouble() < Config.VanillaLootChance / 100f;
			foreach (Item item in inventory)
			{
				if (item.parentSheetIndex == fish || vanilla) 
                    items.Add(item);
            }

			if (Game1.random.NextDouble() <= 0.33 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
			{
				items.Add(new Object(890, Game1.random.Next(1, 3) + ((Game1.random.NextDouble() < 0.25) ? 2 : 0), false, -1, 0));
			}

			inventory = items;
            Game1.player.Money += coins;
            SMonitor.Log($"chest contains {coins} gold");
        }


        private static IList<Item> GetVanillaLoot(FishingRod fr)
		{
			float chance = 1f;

			int clearWaterDistance = SHelper.Reflection.GetField<int>(fr, "clearWaterDistance").GetValue();
			List<Item> treasures = new List<Item>();

			while (Game1.random.NextDouble() <= chance)
			{
				chance *= 0.4f;
				if (Game1.currentSeason.Equals("spring") && !(fr.getLastFarmerToUse().currentLocation is Beach) && Game1.random.NextDouble() < 0.1)
				{
					treasures.Add(new Object(273, Game1.random.Next(2, 6) + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), false, -1, 0));
				}
				if (fr.caughtDoubleFish && Game1.random.NextDouble() < 0.5)
				{
					treasures.Add(new Object(774, 2 + ((Game1.random.NextDouble() < 0.25) ? 2 : 0), false, -1, 0));
				}
				switch (Game1.random.Next(4))
				{
					case 0:
						if (clearWaterDistance >= 5 && Game1.random.NextDouble() < 0.03)
						{
							treasures.Add(new Object(386, Game1.random.Next(1, 3), false, -1, 0));
						}
						else
						{
							List<int> possibles = new List<int>();
							if (clearWaterDistance >= 4)
							{
								possibles.Add(384);
							}
							if (clearWaterDistance >= 3 && (possibles.Count == 0 || Game1.random.NextDouble() < 0.6))
							{
								possibles.Add(380);
							}
							if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
							{
								possibles.Add(378);
							}
							if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
							{
								possibles.Add(388);
							}
							if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
							{
								possibles.Add(390);
							}
							possibles.Add(382);
							treasures.Add(new Object(possibles.ElementAt(Game1.random.Next(possibles.Count)), Game1.random.Next(2, 7) * ((Game1.random.NextDouble() < 0.05 + fr.getLastFarmerToUse().luckLevel * 0.015) ? 2 : 1), false, -1, 0));
							if (Game1.random.NextDouble() < 0.05 + fr.getLastFarmerToUse().LuckLevel * 0.03)
							{
								treasures.Last<Item>().Stack *= 2;
							}
						}
						break;
					case 1:
						if (clearWaterDistance >= 4 && Game1.random.NextDouble() < 0.1 && fr.getLastFarmerToUse().FishingLevel >= 6)
						{
							treasures.Add(new Object(687, 1, false, -1, 0));
						}
						else if (Game1.random.NextDouble() < 0.25 && fr.getLastFarmerToUse().craftingRecipes.ContainsKey("Wild Bait"))
						{
							treasures.Add(new Object(774, 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), false, -1, 0));
						}
						else if (fr.getLastFarmerToUse().FishingLevel >= 6)
						{
							treasures.Add(new Object(685, 1, false, -1, 0));
						}
						else
						{
							treasures.Add(new Object(685, 10, false, -1, 0));
						}
						break;
					case 2:
						if (Game1.random.NextDouble() < 0.1 && Game1.netWorldState.Value.LostBooksFound < 21 && fr.getLastFarmerToUse() != null && fr.getLastFarmerToUse().hasOrWillReceiveMail("lostBookFound"))
						{
							treasures.Add(new Object(102, 1, false, -1, 0));
						}
						else if (fr.getLastFarmerToUse().archaeologyFound.Count() > 0)
						{
							if (Game1.random.NextDouble() < 0.25 && fr.getLastFarmerToUse().FishingLevel > 1)
							{
								treasures.Add(new Object(Game1.random.Next(585, 588), 1, false, -1, 0));
							}
							else if (Game1.random.NextDouble() < 0.5 && fr.getLastFarmerToUse().FishingLevel > 1)
							{
								treasures.Add(new Object(Game1.random.Next(103, 120), 1, false, -1, 0));
							}
							else
							{
								treasures.Add(new Object(535, 1, false, -1, 0));
							}
						}
						else
						{
							treasures.Add(new Object(382, Game1.random.Next(1, 3), false, -1, 0));
						}
						break;
					case 3:
						switch (Game1.random.Next(3))
						{
							case 0:
								if (clearWaterDistance >= 4)
								{
									treasures.Add(new Object(537 + ((Game1.random.NextDouble() < 0.4) ? Game1.random.Next(-2, 0) : 0), Game1.random.Next(1, 4), false, -1, 0));
								}
								else if (clearWaterDistance >= 3)
								{
									treasures.Add(new Object(536 + ((Game1.random.NextDouble() < 0.4) ? -1 : 0), Game1.random.Next(1, 4), false, -1, 0));
								}
								else
								{
									treasures.Add(new Object(535, Game1.random.Next(1, 4), false, -1, 0));
								}
								if (Game1.random.NextDouble() < 0.05 + fr.getLastFarmerToUse().LuckLevel * 0.03)
								{
									treasures.Last<Item>().Stack *= 2;
								}
								break;
							case 1:
								if (fr.getLastFarmerToUse().FishingLevel < 2)
								{
									treasures.Add(new Object(382, Game1.random.Next(1, 4), false, -1, 0));
								}
								else
								{
									if (clearWaterDistance >= 4)
									{
										treasures.Add(new Object((Game1.random.NextDouble() < 0.3) ? 82 : ((Game1.random.NextDouble() < 0.5) ? 64 : 60), Game1.random.Next(1, 3), false, -1, 0));
									}
									else if (clearWaterDistance >= 3)
									{
										treasures.Add(new Object((Game1.random.NextDouble() < 0.3) ? 84 : ((Game1.random.NextDouble() < 0.5) ? 70 : 62), Game1.random.Next(1, 3), false, -1, 0));
									}
									else
									{
										treasures.Add(new Object((Game1.random.NextDouble() < 0.3) ? 86 : ((Game1.random.NextDouble() < 0.5) ? 66 : 68), Game1.random.Next(1, 3), false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.028 * (clearWaterDistance / 5f))
									{
										treasures.Add(new Object(72, 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.05)
									{
										treasures.Last<Item>().Stack *= 2;
									}
								}
								break;
							case 2:
								if (fr.getLastFarmerToUse().FishingLevel < 2)
								{
									treasures.Add(new Object(770, Game1.random.Next(1, 4), false, -1, 0));
								}
								else
								{
									float luckModifier = (1f + (float)fr.getLastFarmerToUse().DailyLuck) * (clearWaterDistance / 5f);
									if (Game1.random.NextDouble() < 0.05 * luckModifier && !fr.getLastFarmerToUse().specialItems.Contains(14))
									{
										treasures.Add(new MeleeWeapon(14)
										{
											specialItem = true
										});
									}
									if (Game1.random.NextDouble() < 0.05 * luckModifier && !fr.getLastFarmerToUse().specialItems.Contains(51))
									{
										treasures.Add(new MeleeWeapon(51)
										{
											specialItem = true
										});
									}
									if (Game1.random.NextDouble() < 0.07 * luckModifier)
									{
										switch (Game1.random.Next(3))
										{
											case 0:
												treasures.Add(new Ring(516 + ((Game1.random.NextDouble() < fr.getLastFarmerToUse().LuckLevel / 11f) ? 1 : 0)));
												break;
											case 1:
												treasures.Add(new Ring(518 + ((Game1.random.NextDouble() < fr.getLastFarmerToUse().LuckLevel / 11f) ? 1 : 0)));
												break;
											case 2:
												treasures.Add(new Ring(Game1.random.Next(529, 535)));
												break;
										}
									}
									if (Game1.random.NextDouble() < 0.02 * luckModifier)
									{
										treasures.Add(new Object(166, 1, false, -1, 0));
									}
									if (fr.getLastFarmerToUse().FishingLevel > 5 && Game1.random.NextDouble() < 0.001 * luckModifier)
									{
										treasures.Add(new Object(74, 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Object(127, 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Object(126, 1, false, -1, 0));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Ring(527));
									}
									if (Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Boots(Game1.random.Next(504, 514)));
									}
									if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && Game1.random.NextDouble() < 0.01 * luckModifier)
									{
										treasures.Add(new Object(928, 1, false, -1, 0));
									}
									if (treasures.Count == 1)
									{
										treasures.Add(new Object(72, 1, false, -1, 0));
									}
								}
								break;
						}
						break;
				}
			}
			if (treasures.Count == 0)
			{
				treasures.Add(new Object(685, Game1.random.Next(1, 4) * 5, false, -1, 0));
			}
			return treasures;
		}
    }
}
