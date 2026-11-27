using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuestHelper
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }


        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (false && e.Button == SButton.Home)
			{
                Game1.drawObjectDialogue(GetItemInfo("(O)688"));
                return;
                Game1.playSound("newArtifact", null);
                Game1.player.acceptedDailyQuest.Value = false;
                double d = Game1.random.NextDouble();
                Quest quest;
                if (d < 0.08)
                {
                    quest = new ResourceCollectionQuest();
                }
                else if (d < 0.2 && MineShaft.lowestLevelReached > 0 && Game1.stats.DaysPlayed > 5U)
                {
                    quest = new SlayMonsterQuest
                    {
                        ignoreFarmMonsters = { true }
                    };
                }
                else if (d < 0.6)
                {
                    quest = new FishingQuest();
                }
                else if (d < 0.66 && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Mon"))
                {
                    bool foundOne = false;
                    foreach (Farmer farmer in Game1.getAllFarmers())
                    {
                        using (NetList<Quest, NetRef<Quest>>.Enumerator enumerator2 = farmer.questLog.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (enumerator2.Current is SocializeQuest)
                                {
                                    foundOne = true;
                                    break;
                                }
                            }
                        }
                        if (foundOne)
                        {
                            break;
                        }
                    }
                    if (!foundOne)
                    {
                        quest = new SocializeQuest();
                    }
                    else
                    {
                        quest = new ItemDeliveryQuest();
                    }
                }
                else
                {
                    quest = new ItemDeliveryQuest();
                }
                Game1.netWorldState.Value.SetQuestOfTheDay(quest);
            }
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
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowQuestMarkers.Name"),
					getValue: () => Config.ShowQuestMarkers,
					setValue: value => Config.ShowQuestMarkers = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowQuestDetails.Name"),
					getValue: () => Config.ShowQuestDetails,
					setValue: value => Config.ShowQuestDetails = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.RebuildSocializeQuest.Name"),
					getValue: () => false,
					setValue: value => ResetIntroQuests()
				);
			}
		}
	}
}
