using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;

namespace PersonalTravelingCart
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		public const string dataPath = "aedenthorn.PersonalTravellingCart/dictionary";
		public const string whichCartKey = "aedenthorn.PersonalTravellingCart/whichCart";
		public const string parkedKey = "aedenthorn.PersonalTravellingCart/parked";
		public const string parkedListKey = "aedenthorn.PersonalTravellingCart/parkedListKey";
		public const string outdoorsInfosKey = "aedenthorn.PersonalTravellingCart/outdoorsInfos";
		public const string defaultKey = "_default";
		private static Dictionary<string, TravelingCart> travelingCartDictionary = new();
		private static GameTime deltaTime;
		private static bool drawingExterior;

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
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			helper.Events.Multiplayer.PeerConnected += Multiplayer_PeerConnected;
			helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(SaveGame), nameof(SaveGame.loadDataToLocations)),
					prefix: new HarmonyMethod(typeof(SaveGame_loadDataToLocations_Patch), nameof(SaveGame_loadDataToLocations_Patch.Prefix)),
					postfix: new HarmonyMethod(typeof(SaveGame_loadDataToLocations_Patch), nameof(SaveGame_loadDataToLocations_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Horse), nameof(Horse.update), new Type[] { typeof(GameTime), typeof(GameLocation) }),
					prefix: new HarmonyMethod(typeof(Horse_update_Patch), nameof(Horse_update_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Horse), nameof(Horse.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(typeof(Horse_draw_Patch), nameof(Horse_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(typeof(Farmer_draw_Patch), nameof(Farmer_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }),
					postfix: new HarmonyMethod(typeof(Farmer_draw_Patch), nameof(Farmer_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }),
					transpiler: new HarmonyMethod(typeof(Farmer_draw_Patch), nameof(Farmer_draw_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Utility), nameof(Utility.canGrabSomethingFromHere)),
					prefix: new HarmonyMethod(typeof(Utility_canGrabSomethingFromHere_Patch), nameof(Utility_canGrabSomethingFromHere_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
					prefix: new HarmonyMethod(typeof(GameLocation_checkAction_Patch), nameof(GameLocation_checkAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) }),
					postfix: new HarmonyMethod(typeof(GameLocation_isCollidingPosition_Patch), nameof(GameLocation_isCollidingPosition_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw)),
					postfix: new HarmonyMethod(typeof(GameLocation_draw_Patch), nameof(GameLocation_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.drawBackground)),
					postfix: new HarmonyMethod(typeof(GameLocation_drawBackground_Patch), nameof(GameLocation_drawBackground_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.drawWeather)),
					prefix: new HarmonyMethod(typeof(Game1_drawWeather_Patch), nameof(Game1_drawWeather_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlaceThisFurnitureHere)),
					prefix: new HarmonyMethod(typeof(GameLocation_CanPlaceThisFurnitureHere_Patch), nameof(GameLocation_CanPlaceThisFurnitureHere_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Stable), nameof(Stable.grabHorse)),
					prefix: new HarmonyMethod(typeof(Stable_grabHorse_Patch), nameof(Stable_grabHorse_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.warpCharacter), new Type[] { typeof(NPC), typeof(GameLocation), typeof(Vector2) }),
					postfix: new HarmonyMethod(typeof(Game1_warpCharacter_Patch), nameof(Game1_warpCharacter_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), "startSleep"),
					prefix: new HarmonyMethod(typeof(GameLocation_startSleep_Patch), nameof(GameLocation_startSleep_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			LoadTravelingCarts();

			HashSet<string> locations = new();
			HashSet<DecoratableLocation> newLocations = new();

			Utility.ForEachLocation(location =>
			{
				locations.Add(location.NameOrUniqueName);
				return true;
			});
			Utility.ForEachCharacter(character =>
			{
				if (character is Horse horse)
				{
					string location = $"{SModManifest.UniqueID}/{horse.HorseId}";

					if (!locations.Contains(location))
					{
						newLocations.Add(new DecoratableLocation((horse.modData.TryGetValue(whichCartKey, out string which) && travelingCartDictionary.TryGetValue(which, out TravelingCart data) && data.mapPath is not null) ? data.mapPath : SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").Name, location));
					}
				}
				return true;
			});
			foreach (DecoratableLocation location in newLocations)
			{
				Game1.locations.Add(location);
			}
		}

		private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			Horse horse = Utility.findHorseForPlayer(Game1.player.UniqueMultiplayerID);

			if (horse is not null && horse.modData.ContainsKey(parkedKey))
			{
				bool foundParkedTravelingCart = false;

				Utility.ForEachLocation(location =>
				{
					if (Game1.getFarm().modData.TryGetValue($"{parkedListKey}/{location.NameOrUniqueName}", out string parkedString))
					{
						List<ParkedTravelingCart> parkedTravelingCarts = JsonConvert.DeserializeObject<List<ParkedTravelingCart>>(parkedString);

						for (int i = 0; i < parkedTravelingCarts.Count; i++)
						{
							ParkedTravelingCart parkedTravelingCart = parkedTravelingCarts[i];

							if (parkedTravelingCart.location == $"{SModManifest.UniqueID}/{horse.HorseId}")
							{
								SMonitor.Log($"Found parked traveling cart in {location}");
								foundParkedTravelingCart = true;
								return false;
							}
						}
					}
					return true;
				});
				if (!foundParkedTravelingCart)
				{
					horse.modData.Remove(parkedKey);
				}
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsPlayerFree || !Game1.player.isRidingHorse())
				return;

			Horse horse = Game1.player.mount;

			if (horse.modData.TryGetValue(whichCartKey, out string which) && travelingCartDictionary.TryGetValue(which, out TravelingCart data))
			{
				if (e.Button == Config.HitchButton)
				{
					HandleHitchButton(horse, which);
				}
				else if (e.Button == SButton.PageUp || e.Button == SButton.PageDown)
				{
					SwitchCart(horse, which, e.Button == SButton.PageUp ? -1 : 1);
				}
				else if (Config.Debug)
				{
					HandleDebug(which, data, e.Button);
				}
			}
		}

		private void Multiplayer_PeerConnected(object sender, PeerConnectedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsMainPlayer)
				return;

			if (!e.Peer.IsHost && e.Peer.HasSmapi)
			{
				List<(string, string)> serializedLocations = GetSerializedLocations();

				if (serializedLocations is not null && serializedLocations.Count > 0)
				{
					Helper.Multiplayer.SendMessage(serializedLocations, "InvokeMethod.LoadSerializedLocations", modIDs: new[] { ModManifest.UniqueID }, playerIDs: new[] { e.Peer.PlayerID });
				}
			}
		}

		private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (e.FromModID == ModManifest.UniqueID && e.Type == "InvokeMethod.LoadSerializedLocations")
			{
				LoadSerializedLocations(e.ReadAs<List<(string, string)>>());
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (e.NameWithoutLocale.IsEquivalentTo(dataPath))
			{
				e.LoadFrom(() => new Dictionary<string, TravelingCart>(), AssetLoadPriority.Exclusive);
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
					name: () => SHelper.Translation.Get("GMCM.DrawCartExterior.Name"),
					getValue: () => Config.DrawCartExterior,
					setValue: value => Config.DrawCartExterior = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.DrawCartExteriorWeather.Name"),
					getValue: () => Config.DrawCartExteriorWeather,
					setValue: value => Config.DrawCartExteriorWeather = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.CollisionsEnabled.Name"),
					getValue: () => Config.CollisionsEnabled,
					setValue: value => Config.CollisionsEnabled = value
				);
				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.HitchButton.Name"),
					getValue: () => Config.HitchButton,
					setValue: value => Config.HitchButton = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.WarpHorsesOnDayStart.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.WarpHorsesOnDayStart.Tooltip"),
					getValue: () => Config.WarpHorsesOnDayStart,
					setValue: value => Config.WarpHorsesOnDayStart = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Debug.Name"),
					getValue: () => Config.Debug,
					setValue: value => Config.Debug = value
				);
			}
		}
	}
}
