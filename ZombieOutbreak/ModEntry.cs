using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace ZombieOutbreak
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		internal static Dictionary<string, Texture2D> zombieNPCTextures = new();
		internal static Dictionary<string, Texture2D> zombieNPCPortraits = new();
		internal static Dictionary<long, Texture2D> zombieFarmerTextures = new();
		internal static List<string> curedNPCs = new();
		internal static List<long> curedFarmers = new();

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.GameLoop.Saving += GameLoop_Saving;
			Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
			Helper.Events.Display.MenuChanged += Display_MenuChanged;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(NPC), nameof(NPC.receiveGift)),
					prefix: new HarmonyMethod(typeof(NPC_receiveGift_Patch), nameof(NPC_receiveGift_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)),
					prefix: new HarmonyMethod(typeof(Farmer_eatObject_Patch), nameof(Farmer_eatObject_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(NPC), nameof(NPC.draw), new Type[] { typeof(SpriteBatch), typeof(float) }),
					prefix: new HarmonyMethod(typeof(NPC_draw_Patch), nameof(NPC_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawPortrait)),
					prefix: new HarmonyMethod(typeof(DialogueBox_drawPortrait_Patch), nameof(DialogueBox_drawPortrait_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(typeof(Farmer_draw_Patch), nameof(Farmer_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Dialogue), nameof(Dialogue.getCurrentDialogue)),
					postfix: new HarmonyMethod(typeof(Dialogue_getCurrentDialogue_Patch), nameof(Dialogue_getCurrentDialogue_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Dialogue), nameof(Dialogue.getResponseOptions)),
					postfix: new HarmonyMethod(typeof(Dialogue_getResponseOptions_Patch), nameof(Dialogue_getResponseOptions_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(NPC), nameof(NPC.showTextAboveHead)),
					prefix: new HarmonyMethod(typeof(NPC_showTextAboveHead_Patch), nameof(NPC_showTextAboveHead_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Constructor(typeof(ShopMenu), new Type[] {typeof(string), typeof(ShopData), typeof(ShopOwnerData), typeof(NPC), typeof(ShopMenu.OnPurchaseDelegate), typeof(Func<ISalable, bool>), typeof(bool) }),
					postfix: new HarmonyMethod(typeof(ShopMenu_Patch), nameof(ShopMenu_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Constructor(typeof(DialogueBox), new Type[] { typeof(string), typeof(Response[]), typeof(int) }),
					postfix: new HarmonyMethod(typeof(DialogueBox_Patch), nameof(DialogueBox_Patch.Postfix))
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

			if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add($"{ModManifest.UniqueID}_ZombieCure", $"768 5 167 1 420 1 422 1 382 10/Home/{ModManifest.UniqueID}_ZombieCure 5/false/none");
				});
			}
			if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, ObjectData> data = asset.AsDictionary<string, ObjectData>().Data;

					data.Add($"{ModManifest.UniqueID}_ZombieCure", new ObjectData() {
						Name = "Zombie cure",
						DisplayName = $"[{ModManifest.UniqueID}_i18n item.zombie-cure.name]",
						Description = $"[{ModManifest.UniqueID}_i18n item.zombie-cure.description]",
						Type = "Basic",
						Category = Object.artisanGoodsCategory,
						Price = 0,
						Texture = $"{SHelper.ModContent.GetInternalAssetName("assets/zombie-cure").Name.Replace('/', '\\')}",
						SpriteIndex = 0,
						Edibility = 0,
						IsDrink = false,
						Buffs = null,
						GeodeDropsDefaultItems = false,
						GeodeDrops = null,
						ArtifactSpotChances = null,
						ExcludeFromFishingCollection = false,
						ExcludeFromShippingCollection = false,
						ExcludeFromRandomSale = false,
						ContextTags = null,
						CustomFields = null
					});
				});
			}
			if (e.NameWithoutLocale.IsEquivalentTo("Data/NPCGiftTastes"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data["Universal_Hate"] += $" {ModManifest.UniqueID}_ZombieCure";
				});
			}
			if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data[$"{SModManifest.UniqueID}_ZombieCure"] = Helper.Translation.Get("mail.zombie-cure", new { craftingRecipe = $"%item craftingRecipe {ModManifest.UniqueID}_ZombieCure %%" });
				});
			}
		}

		private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady)
				return;

			if (e.OldMenu is CharacterCustomization && e.NewMenu is not CharacterCustomization && zombieFarmerTextures.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				MakeZombieFarmerTexture(Game1.player.UniqueMultiplayerID);
			}
		}

		private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady)
				return;

			CheckForInfection();
		}

		private void GameLoop_Saving(object sender, SavingEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			Helper.Data.WriteSaveData("zombiesNPC", zombieNPCTextures.Keys.ToList());
			Helper.Data.WriteSaveData("zombiesFarmers", zombieFarmerTextures.Keys.ToList());
			foreach (string npcName in zombieNPCTextures.Keys)
			{
				RemoveZombieNPC(npcName);
			}
			foreach (long farmerId in zombieFarmerTextures.Keys)
			{
				RemoveZombieFarmer(farmerId);
			}
			ClearAll();
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			List<string> zombiesNPC = Helper.Data.ReadSaveData<List<string>>("zombiesNPC") ?? new List<string>();
			List<long> zombiesFarmers = Helper.Data.ReadSaveData<List<long>>("zombiesFarmers") ?? new List<long>();

			Monitor.Log($"Loading zombie textures");
			ClearAll();
			foreach (string npcName in zombiesNPC)
			{
				AddZombieNPC(npcName);
			}
			foreach (long farmerId in zombiesFarmers)
			{
				AddZombieFarmer(farmerId);
			}
			Monitor.Log($"Got {zombieNPCTextures.Count} zombie NPC(s)");
			Monitor.Log($"Got {zombiesFarmers.Count} zombie farmer(s)");

			if (Game1.random.NextDouble() < Config.DailyZombificationChance / 100f)
			{
				MakeRandomZombie();
			}
			if (zombieNPCTextures.Count > 0 && !Game1.player.mailReceived.Contains($"{SModManifest.UniqueID}_ZombieCure"))
			{
				Game1.mailbox.Add($"{SModManifest.UniqueID}_ZombieCure");
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			RegisterConsoleCommands();
			TokensUtility.Register();

			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => {
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/Objects"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/NPCGiftTastes"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/mail"));
						Helper.WriteConfig(Config);
					}
				);

				// Main section
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => {
						if (Context.IsWorldReady && Config.ModEnabled && !value)
						{
							foreach (string npcName in zombieNPCTextures.Keys)
							{
								RemoveZombieNPC(npcName);
							}
							foreach (long farmerId in zombieFarmerTextures.Keys)
							{
								RemoveZombieFarmer(farmerId);
							}
							zombieNPCTextures.Clear();
							zombieNPCPortraits.Clear();
							zombieFarmerTextures.Clear();
							curedNPCs.Clear();
							curedFarmers.Clear();
							Helper.Data.WriteSaveData<List<string>>("zombiesNPC", null);
							Helper.Data.WriteSaveData<List<long>>("zombiesFarmers", null);
						}
						Config.ModEnabled = value;
					}
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.DailyZombificationChance.Name"),
					getValue: () => Config.DailyZombificationChance,
					setValue: value => Config.DailyZombificationChance = value,
					min: 0,
					max: 100
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.InfectionRadius.Name"),
					getValue: () => Config.InfectionRadius,
					setValue: value => Config.InfectionRadius = value,
					min: 0
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.InfectionChancePerSecond.Name"),
					getValue: () => Config.InfectionChancePerSecond,
					setValue: value => Config.InfectionChancePerSecond = value,
					min: 0,
					max: 100
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.GreenTint.Name"),
					getValue: () => Config.GreenTint,
					setValue: value => Config.GreenTint = value,
					min: 0,
					max: 100
				);
			}
		}
	}
}
