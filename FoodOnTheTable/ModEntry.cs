using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace FoodOnTheTable
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static IFurnitureDisplayFrameworkAPI fdfAPI;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.performTenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(NPC_performTenMinuteUpdate_Postfix))
            );
           harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.dayUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(NPC_dayUpdate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.updateEvenIfFarmerIsntHere)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(FarmHouse_updateEvenIfFarmerIsntHere_Postfix))
            );
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
					name: () => "Mod Enabled?",
					getValue: () => Config.EnableMod,
					setValue: value => Config.EnableMod = value
				);

				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => "Minutes To Hungry",
					tooltip: () => "Minutes since last meal",
					getValue: () => Config.MinutesToHungry,
					setValue: value => Config.MinutesToHungry = value
				);
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => "Points Mult",
					tooltip: () => "Friendship point multiplier for spouses and roommates",
					getValue: () => "" + Config.PointsMult,
					setValue: delegate (string value) { try { Config.PointsMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
				);
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => "Move To Food % Chance",
					tooltip: () => "Percent chance per tick to move to food if hungry",
					getValue: () => "" + Config.MoveToFoodChance,
					setValue: delegate (string value) { try { Config.MoveToFoodChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
				);
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => "Max Distance to Eat",
					tooltip: () => "Max distance in tiles from food to eat it",
					getValue: () => "" + Config.MaxDistanceToEat,
					setValue: delegate (string value) { try { Config.MaxDistanceToEat = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
				);
				configMenu.AddBoolOption(
					mod: ModManifest,
					name: () => "Count as Fed Spouse",
					tooltip: () => "For Another Hunger Mod",
					getValue: () => Config.CountAsFedSpouse,
					setValue: value => Config.CountAsFedSpouse = value
				);
				configMenu.AddBoolOption(
					mod: ModManifest,
					name: () => "Only In Farmhouse",
					getValue: () => Config.OnlyInFarmhouse,
					setValue: value => Config.OnlyInFarmhouse = value
				);
			}
			try
			{
                fdfAPI = SHelper.ModRegistry.GetApi<IFurnitureDisplayFrameworkAPI>("aedenthorn.FurnitureDisplayFramework");
            }
			catch { }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
        }


	}

}