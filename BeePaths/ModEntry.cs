using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace BeePaths
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;
		internal static Dictionary<string, Dictionary<Vector2, HiveData>> hives = new();
		internal static Texture2D beeTexture;
		internal static ICue beeSound;

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
			Helper.Events.Multiplayer.ModMessageReceived += Multiplayer_OnModMessageReceived;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(OverlaidDictionary), nameof(OverlaidDictionary.OnValueAdded), new Type[] { typeof(Vector2), typeof(Object) }),
					postfix: new HarmonyMethod(typeof(OnValueAdded_Patch), nameof(OnValueAdded_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(OverlaidDictionary), nameof(OverlaidDictionary.OnValueRemoved), new Type[] { typeof(Vector2), typeof(Object) }),
					prefix: new HarmonyMethod(typeof(OnValueRemoved_Patch), nameof(OnValueRemoved_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.destroyCrop), new Type[] { typeof(bool) }),
					prefix: new HarmonyMethod(typeof(DestroyCrop_Patch), nameof(DestroyCrop_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.destroyCrop), new Type[] { typeof(bool) }),
					postfix: new HarmonyMethod(typeof(DestroyCrop_Patch), nameof(DestroyCrop_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Utility), nameof(Utility.findCloseFlower), new Type[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) }),
					prefix: new HarmonyMethod(typeof(FindCloseFlower_Patch), nameof(FindCloseFlower_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}

			beeTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
			beeTexture.SetData(new Color[] { Color.White });
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			if(!Config.ModEnabled)
				return;

			ResetHives();
		}

		private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsPlayerFree || (!Config.ShowWhenRaining && Game1.IsRainingHere(Game1.currentLocation)))
				return;

			bool buzzing = false;
			float buzzDistance = float.MaxValue;
			float maxSoundDistance = Config.MaxSoundDistance;

			if (hives.TryGetValue(Game1.currentLocation.NameOrUniqueName, out Dictionary<Vector2, HiveData> dictionary))
			{
				foreach (HiveData hiveData in dictionary.Values)
				{
					if (!IsOnScreen(hiveData.hiveTile * 64, hiveData.cropTile * 64, 64))
						continue;

					Vector2 offset;

					if (hiveData.isIndoorPot)
					{
						if (CompatibilityUtility.IsWallPlantersLoaded)
						{
							int wallPlantersOffset = 0;
							int wallPlantersInnerOffset = 0;

							if (Game1.currentLocation.getObjectAtTile((int)hiveData.cropTile.X, (int)hiveData.cropTile.Y) is IndoorPot indoorPot)
							{
								if (indoorPot.modData.ContainsKey(CompatibilityUtility.wallPlantersOffsetKey))
								{
									wallPlantersOffset = int.Parse(indoorPot.modData[CompatibilityUtility.wallPlantersOffsetKey]);
								}
								if (indoorPot.modData.ContainsKey(CompatibilityUtility.wallPlantersInnerOffsetKey))
								{
									wallPlantersInnerOffset = int.Parse(indoorPot.modData[CompatibilityUtility.wallPlantersInnerOffsetKey]);
								}
							}
							offset = new Vector2(0, -24 - wallPlantersOffset - wallPlantersInnerOffset);
						}
						else
						{
							offset = new Vector2(0, -24);
						}
					}
					else
					{
						offset = Vector2.Zero;
					}
					while (hiveData.bees.Count < Config.NumberBees)
					{
						hiveData.bees.Add(GetBee(hiveData.hiveTile, hiveData.cropTile, true, false));
					}
					for (int i = hiveData.bees.Count - 1; i >= 0; i--)
					{
						BeeData bee = hiveData.bees[i];
						Vector2 direction = Vector2.Normalize(new Vector2(-bee.position.Y, bee.position.X));
						Vector2 drawPosition = bee.position + direction * 5 * (float)Math.Sin(Vector2.Distance(bee.startPosition, bee.position) / 20);
						Vector2 adjustedEndPosition = bee.isGoingToFlower ? bee.endPosition + offset : bee.endPosition;
						Vector2 translation = adjustedEndPosition - drawPosition;

						e.SpriteBatch.Draw(beeTexture, Game1.GlobalToLocal(drawPosition), null, Config.BeeColor, -(float)Math.Atan2(translation.Y, translation.X), Vector2.Zero, Config.BeeScale, SpriteEffects.None, 1);
						if (Config.BeeDamage > 0 && Game1.random.Next(100) < Config.BeeStingChance)
						{
							foreach (Farmer farmer in Game1.currentLocation.farmers)
							{
								if (farmer.GetBoundingBox().Contains(bee.position + new Vector2(0, 32)))
									farmer.takeDamage(Config.BeeDamage, true, null);
							}
						}
						if (!string.IsNullOrEmpty(Config.BeeSound) && maxSoundDistance > 0f)
						{
							float playerBeeDistance = Vector2.Distance(Game1.player.Tile, bee.position / 64 + new Vector2(-0.5f, 0.5f));

							if (playerBeeDistance < maxSoundDistance && playerBeeDistance < buzzDistance)
							{
								buzzing = true;
								buzzDistance = playerBeeDistance;
							}
						}

						float beeDistanceToEndPosition = Vector2.Distance(adjustedEndPosition, bee.position);

						if (beeDistanceToEndPosition > Config.BeeSpeed)
						{
							bee.position = Vector2.Lerp(bee.position, adjustedEndPosition, Config.BeeSpeed / beeDistanceToEndPosition);
						}
						else
						{
							if (bee.isGoingToFlower)
							{
								bee.isGoingToFlower = !bee.isGoingToFlower;
								(bee.endPosition, bee.startPosition) = (bee.startPosition, bee.endPosition);
							}
							else
							{
								hiveData.bees.RemoveAt(i);
							}
						}
					}
				}
			}
			if (buzzing)
			{
				if (beeSound is null || !beeSound.Name.Equals(Config.BeeSound))
				{
					beeSound = Game1.soundBank.GetCue(Config.BeeSound);
				}
				beeSound.Pitch = 0;
				beeSound.SetVariable("Volume", 100 - 100 * buzzDistance / maxSoundDistance - 10);
				beeSound.SetVariable("Pitch", 0f);
				if (!beeSound.IsPlaying)
				{
					beeSound.Play();
				}
			}
			else
			{
				beeSound?.Stop(AudioStopOptions.AsAuthored);
			}
		}

		private void Multiplayer_OnModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
		{
			if (e.FromModID == ModManifest.UniqueID)
			{
				if (e.Type == "InvokeMethod.OnValueAdded_Patch.PostfixCore")
				{
					(Vector2 key, string locationNameOrUniqueName, string name) = e.ReadAs<(Vector2, string, string)>();
					OnValueAdded_Patch.PostfixCore(key, locationNameOrUniqueName, name, false);
				}
				if (e.Type == "InvokeMethod.OnValueRemoved_Patch.PrefixCore")
				{
					(Vector2 key, string locationNameOrUniqueName, string name) = e.ReadAs<(Vector2, string, string)>();
					OnValueRemoved_Patch.PrefixCore(key, locationNameOrUniqueName, name, false);
				}
				if (e.Type == "InvokeMethod.DestroyCrop_Patch.PostfixCore")
				{
					(Vector2 tilePosition, string locationNameOrUniqueName)= e.ReadAs<(Vector2, string)>();
					DestroyCrop_Patch.PostfixCore(tilePosition, locationNameOrUniqueName, false);
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (configMenu is null)
				return;

			static void ResetAllHiveDataBees()
			{
				foreach (Dictionary<Vector2, HiveData> dictionary in hives.Values)
				{
					foreach (HiveData hiveData in dictionary.Values)
					{
						ResetHiveDataBees(hiveData);
					}
				}
			}

			static void SetValueAndPerformFunctionIf<T>(Action<T> setValue, T value, Func<bool> condition, Action function)
			{
				bool shouldReset = false;

				if (Context.IsWorldReady && condition())
				{
					shouldReset = true;
				}
				setValue(value);
				if (shouldReset)
				{
					function();
				}
			}

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => {
					Config = new ModConfig();
					ResetHives();
				},
				save: () => Helper.WriteConfig(Config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => SetValueAndPerformFunctionIf((v) => Config.ModEnabled = v, value, () => !Config.ModEnabled && value, ResetHives)
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowWhenRaining.Name"),
				getValue: () => Config.ShowWhenRaining,
				setValue: value => Config.ShowWhenRaining = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FixFlowerFind.Name"),
				getValue: () => Config.FixFlowerFind,
				setValue: value => SetValueAndPerformFunctionIf((v) => Config.FixFlowerFind = v, value, () => Config.FixFlowerFind != value, ResetHives)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.NumberBees.Name"),
				getValue: () => Config.NumberBees,
				setValue: value => SetValueAndPerformFunctionIf((v) => Config.NumberBees = v, value, () => Config.NumberBees != value, ResetAllHiveDataBees)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeRange.Name"),
				getValue: () => Config.BeeRange,
				setValue: value => SetValueAndPerformFunctionIf((v) => Config.BeeRange = v, value, () => Config.BeeRange != value, ResetHives)
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeDamage.Name"),
				getValue: () => Config.BeeDamage,
				setValue: value => Config.BeeDamage = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeStingChance.Name"),
				getValue: () => Config.BeeStingChance,
				setValue: value => Config.BeeStingChance = value,
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeScale.Name"),
				getValue: () => Config.BeeScale,
				setValue: value => Config.BeeScale = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeSpeed.Name"),
				getValue: () => Config.BeeSpeed,
				setValue: value => Config.BeeSpeed = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeSound.Name"),
				getValue: () => Config.BeeSound,
				setValue: value => Config.BeeSound = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxSoundDistance.Name"),
				getValue: () => Config.MaxSoundDistance,
				setValue: value => Config.MaxSoundDistance = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeColorR.Name"),
				getValue: () => Config.BeeColor.R,
				setValue: value => Config.BeeColor = new Color(value, Config.BeeColor.G, Config.BeeColor.B, Config.BeeColor.A),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeColorG.Name"),
				getValue: () => Config.BeeColor.G,
				setValue: value => Config.BeeColor = new Color(Config.BeeColor.R, value, Config.BeeColor.B, Config.BeeColor.A),
				min: 0,
				max: 255
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BeeColorB.Name"),
				getValue: () => Config.BeeColor.B,
				setValue: value => Config.BeeColor = new Color(Config.BeeColor.R, Config.BeeColor.G, value, Config.BeeColor.A),
				min: 0,
				max: 255
			);
		}
	}
}
