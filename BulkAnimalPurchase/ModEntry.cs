using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Machines;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace BulkAnimalPurchase
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static ClickableTextureComponent minusButton;
		private static ClickableTextureComponent plusButton;
		private static int animalsToBuy;

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
			helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<Object>), typeof(GameLocation) }),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_Patch), nameof(PurchaseAnimalsMenu_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.drawDialogueBox), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool),typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) }),
					prefix: new HarmonyMethod(typeof(Game1_drawDialogueBox_Patch), nameof(Game1_drawDialogueBox_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.draw), new Type[] { typeof(SpriteBatch) }),
					transpiler: new HarmonyMethod(typeof(PurchaseAnimalsMenu_draw_Patch), nameof(PurchaseAnimalsMenu_draw_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.performHoverAction)),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_performHoverAction_Patch), nameof(PurchaseAnimalsMenu_performHoverAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal)),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_setUpForReturnAfterPurchasingAnimal_Patch), nameof(PurchaseAnimalsMenu_setUpForReturnAfterPurchasingAnimal_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick)),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_receiveLeftClick_Patch), nameof(PurchaseAnimalsMenu_receiveLeftClick_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick)),
					postfix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_receiveLeftClick_Patch), nameof(PurchaseAnimalsMenu_receiveLeftClick_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.salePrice)),
					postfix: new HarmonyMethod(typeof(Item_salePrice_Patch), nameof(Item_salePrice_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawStringWithScrollBackground)),
					prefix: new HarmonyMethod(typeof(SpriteText_drawStringWithScrollBackground_Patch), nameof(SpriteText_drawStringWithScrollBackground_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(AnimalHouse), nameof(AnimalHouse.addNewHatchedAnimal), new Type[] { typeof(string) }),
					transpiler: new HarmonyMethod(typeof(AnimalHouse_addNewHatchedAnimal_Patch), nameof(AnimalHouse_addNewHatchedAnimal_Patch.Transpiler))
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
			if (!Config.EnableMod)
				return;

			if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, MachineData> data = asset.AsDictionary<string, MachineData>().Data;

					foreach (MachineOutputRule machineOutputRule in data["(BC)101"].OutputRules)
					{
						if (machineOutputRule.Id.Equals("Default"))
						{
							machineOutputRule.MinutesUntilReady = Config.DefaultIncubatorTime;
						}
					}
					foreach (MachineOutputRule machineOutputRule in data["(BC)254"].OutputRules)
					{
						if (machineOutputRule.Id.Equals("Default"))
						{
							machineOutputRule.MinutesUntilReady = Config.DefaultOstrichIncubatorTime;
						}
					}
					foreach (MachineOutputRule machineOutputRule in data["(BC)156"].OutputRules)
					{
						if (machineOutputRule.Id.Equals("Default"))
						{
							machineOutputRule.MinutesUntilReady = Config.DefaultSlimeIncubatorTime;
						}
					}
				});
			}
			if (e.NameWithoutLocale.IsEquivalentTo("Data/FarmAnimals"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, FarmAnimalData> data = asset.AsDictionary<string, FarmAnimalData>().Data;

					data["White Chicken"].IncubationTime = Config.ChickenIncubationTime;
					data["Brown Chicken"].IncubationTime = Config.ChickenIncubationTime;
					data["Blue Chicken"].IncubationTime = Config.ChickenIncubationTime;
					data["Void Chicken"].IncubationTime = Config.ChickenIncubationTime;
					data["Golden Chicken"].IncubationTime = Config.ChickenIncubationTime;
					data["Duck"].IncubationTime = Config.DuckIncubationTime;
					data["Dinosaur"].IncubationTime = Config.DinosaurIncubationTime;
					data["Ostrich"].IncubationTime = Config.OstrichIncubationTime;
				});
			}
		}

		private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
		}

		private static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			UpdateMachinesRules();
			SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => {
					Config.DefaultIncubatorTime = Math.Max(Math.Min(Math.Min(Config.ChickenIncubationTime, Config.DuckIncubationTime), Config.DinosaurIncubationTime), 10);
					Config.DefaultOstrichIncubatorTime = Math.Max(Config.OstrichIncubationTime, 10);
					Config.DefaultSlimeIncubatorTime = Math.Max(Math.Min(Math.Min(Math.Min(Math.Min(Config.GreenSlimeIncubationTime, Config.BlueSlimeIncubationTime), Config.RedSlimeIncubationTime), Config.PurpleSlimeIncubationTime), Config.TigerSlimeIncubationTime), 10);
					SHelper.GameContent.InvalidateCache("Data/Machines");
					SHelper.GameContent.InvalidateCache("Data/FarmAnimals");
					UpdateMachinesRules();
					Helper.WriteConfig(Config);
				}
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.EnableMod,
				setValue: value => Config.EnableMod = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.InitialFriendship.Name"),
				getValue: () => Config.InitialFriendship,
				setValue: value => Config.InitialFriendship = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AdultAnimals.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.AdultAnimals.Tooltip"),
				getValue: () => Config.AdultAnimals,
				setValue: value => Config.AdultAnimals = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.Price.Text")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Chicken.Name"),
				getValue: () => Config.ChickenPrice,
				setValue: value => Config.ChickenPrice = Math.Max(value, 0)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Duck"),
				getValue: () => Config.DuckPrice,
				setValue: value => Config.DuckPrice = Math.Max(value, 0)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Rabbit"),
				getValue: () => Config.RabbitPrice,
				setValue: value => Config.RabbitPrice = Math.Max(value, 0)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Cow.Name"),
				getValue: () => Config.CowPrice,
				setValue: value => Config.CowPrice = Math.Max(value, 0)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Goat"),
				getValue: () => Config.GoatPrice,
				setValue: value => Config.GoatPrice = Math.Max(value, 0)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Sheep"),
				getValue: () => Config.SheepPrice,
				setValue: value => Config.SheepPrice = Math.Max(value, 0)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Pig"),
				getValue: () => Config.PigPrice,
				setValue: value => Config.PigPrice = Math.Max(value, 0)
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.IncubationTime.Name")
			);
			configMenu.AddParagraph(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.IncubationTime.Desc")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Chicken.Name"),
				getValue: () => Config.ChickenIncubationTime,
				setValue: value => Config.ChickenIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Duck"),
				getValue: () => Config.DuckIncubationTime,
				setValue: value => Config.DuckIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Dinosaur"),
				getValue: () => Config.DinosaurIncubationTime,
				setValue: value => Config.DinosaurIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Game1.content.LoadString("Strings\\FarmAnimals:DisplayType_Ostrich"),
				getValue: () => Config.OstrichIncubationTime,
				setValue: value => Config.OstrichIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.GreenSlime.Name"),
				getValue: () => Config.GreenSlimeIncubationTime,
				setValue: value => Config.GreenSlimeIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BlueSlime.Name"),
				getValue: () => Config.BlueSlimeIncubationTime,
				setValue: value => Config.BlueSlimeIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RedSlime.Name"),
				getValue: () => Config.RedSlimeIncubationTime,
				setValue: value => Config.RedSlimeIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.PurpleSlime.Name"),
				getValue: () => Config.PurpleSlimeIncubationTime,
				setValue: value => Config.PurpleSlimeIncubationTime = Math.Max(value, 10)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.TigerSlime.Name"),
				getValue: () => Config.TigerSlimeIncubationTime,
				setValue: value => Config.TigerSlimeIncubationTime = Math.Max(value, 10)
			);
		}
	}
}
