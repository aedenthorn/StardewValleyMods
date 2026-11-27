using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;

namespace BuffFramework
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		internal const string dictionaryKey = "aedenthorn.BuffFramework/dictionary";
		internal const string healthRegenKey = "aedenthorn.BuffFramework.healthRegen";
		internal const string staminaRegenKey = "aedenthorn.BuffFramework.staminaRegen";
		internal const string soundKey = "aedenthorn.BuffFramework.sound";
		internal static Dictionary<string, Dictionary<string, object>> buffDictionary = new();
		internal static PerScreen<Dictionary<string, string>> healthRegenerationBuffs = new(() => new());
		internal static PerScreen<Dictionary<string, string>> staminaRegenerationBuffs = new(() => new());
		internal static PerScreen<Dictionary<string, string>> glowRateBuffs = new(() => new());
		internal static Dictionary<string, (string, ICue)> soundBuffs = new();
		internal static List<BuffFrameworkAPI> APIInstances = new();
		internal static bool invokeApplyBuffsOnEquipOnNextTick = false;
		internal static bool invokeUpdateBuffsOnNextTick = false;

		private static float healthRegenerationRemainder = 0f;

		internal static Dictionary<string, string> HealthRegenerationBuffs
		{
			get => healthRegenerationBuffs.Value;
			set => healthRegenerationBuffs.Value = value;
		}

		internal static Dictionary<string, string> StaminaRegenerationBuffs
		{
			get => staminaRegenerationBuffs.Value;
			set => staminaRegenerationBuffs.Value = value;
		}

		internal static Dictionary<string, string> GlowRateBuffs
		{
			get => glowRateBuffs.Value;
			set => glowRateBuffs.Value = value;
		}

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
			Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
			Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
			Helper.Events.Player.InventoryChanged += Player_InventoryChanged;
			Helper.Events.Player.Warped += Player_Warped;
			Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
			Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
			Helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.PropertySetter(typeof(Farmer), nameof(Farmer.CurrentToolIndex)),
					postfix: new HarmonyMethod(typeof(Farmer_ActiveItemSetter_Patch), nameof(Farmer_ActiveItemSetter_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.shiftToolbar)),
					postfix: new HarmonyMethod(typeof(Farmer_shiftToolbar_Patch), nameof(Farmer_shiftToolbar_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Tool), nameof(Tool.attach)),
					postfix: new HarmonyMethod(typeof(Tool_attach_Patch), nameof(Tool_attach_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), "farmerInit"),
					postfix: new HarmonyMethod(typeof(Farmer_farmerInit_Patch), nameof(Farmer_farmerInit_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doneEating)),
					prefix: new HarmonyMethod(typeof(Farmer_doneEating_Patch), nameof(Farmer_doneEating_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Buff), nameof(Buff.OnAdded)),
					postfix: new HarmonyMethod(typeof(Buff_OnAdded_Patch), nameof(Buff_OnAdded_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Buff), nameof(Buff.OnRemoved)),
					postfix: new HarmonyMethod(typeof(Buff_OnRemoved_Patch), nameof(Buff_OnRemoved_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(BuffManager), nameof(BuffManager.GetValues)),
					transpiler: new HarmonyMethod(typeof(BuffManager_GetValues_Patch), nameof(BuffManager_GetValues_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), "startEvent", new Type[] { typeof(Event) }),
					postfix: new HarmonyMethod(typeof(GameLocation_startEvent_Patch), nameof(GameLocation_startEvent_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		public override object GetApi(IModInfo mod)
		{
			BuffFrameworkAPI instance = new();

			APIInstances.Add(instance);
			return instance;
		}

		public void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			healthRegenerationRemainder = 0f;
			invokeUpdateBuffsOnNextTick = true;
		}

		public void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
		{
			invokeUpdateBuffsOnNextTick = true;
		}

		public void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !Game1.shouldTimePass())
				return;

			float totalHealthRegeneration = 0f;
			float totalStaminaRegeneration = 0f;

			foreach (string healthRegen in HealthRegenerationBuffs.Values)
			{
				totalHealthRegeneration += GetFloat(healthRegen);
			}
			foreach (string staminaRegen in StaminaRegenerationBuffs.Values)
			{
				totalStaminaRegeneration += GetFloat(staminaRegen);
			}
			totalHealthRegeneration += healthRegenerationRemainder;
			Game1.player.Stamina = Game1.player.Stamina + totalStaminaRegeneration;
			Game1.player.health = Math.Clamp(Game1.player.health + (int)totalHealthRegeneration, 0, Game1.player.maxHealth);
			healthRegenerationRemainder = totalHealthRegeneration % 1;
		}

		public void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (invokeApplyBuffsOnEquipOnNextTick)
			{
				ApplyBuffsOnEquip();
				invokeApplyBuffsOnEquipOnNextTick = false;
			}
			if (invokeUpdateBuffsOnNextTick)
			{
				UpdateBuffs();
				invokeUpdateBuffsOnNextTick = false;
			}
		}

		public void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
		{
			if (e.Player == Game1.player)
			{
				invokeApplyBuffsOnEquipOnNextTick = true;
			}
		}

		public void Player_Warped(object sender, WarpedEventArgs e)
		{
			if (!Game1.eventUp && !Game1.isFestival())
			{
				HandleEventAndFestivalFinished();
			}
			invokeUpdateBuffsOnNextTick = true;
		}

		public void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
		{
			ClearAll();
		}

		public void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
		{
			ClearAll();
		}

		public void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo(dictionaryKey))
			{
				e.LoadFrom(() => new Dictionary<string, Dictionary<string, object>>(), AssetLoadPriority.Exclusive);
			}
		}

		public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

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
					setValue: value => {
						if (Config.ModEnabled && !value)
						{
							ClearAll();
							Config.ModEnabled = value;
						}
						else if (!Config.ModEnabled && value)
						{
							Config.ModEnabled = value;
							invokeUpdateBuffsOnNextTick = true;
						}
					}
				);
			}
		}
	}
}
