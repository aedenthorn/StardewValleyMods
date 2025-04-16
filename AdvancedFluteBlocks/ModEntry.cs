using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using Object = StardewValley.Object;

namespace AdvancedFluteBlocks
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		const string advancedFluteBlocksKey = "aedenthorn.AdvancedFluteBlocks/tone";

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
			Helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Object), "CheckForActionOnFluteBlock"),
					transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Object_FluteBlock_Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.farmerAdjacentAction)),
					transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Object_FluteBlock_Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.pressSwitchToolButton)),
					prefix: new HarmonyMethod(typeof(ModEntry), nameof(Game1_pressSwitchToolButton_Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		public override object GetApi()
		{
			return new AdvancedFluteBlocksApi();
		}

		private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsPlayerFree || Config.ToneList.Length == 0 || Game1.soundBank == null || !Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) || !obj.Name.Equals("Flute Block"))
				return;

			if (Helper.Input.IsDown(Config.PitchModKey))
			{
				if (!int.TryParse(obj.preservedParentSheetIndex.Value, out int result))
				{
					result = (int)SoundsHelper.DefaultPitch;
				}
				result = (result + (e.Delta < 0 ? -Config.PitchStep : Config.PitchStep)) % (int)SoundsHelper.MaxPitch;
				if (result < 0)
				{
					result += (int)SoundsHelper.MaxPitch;
				}
				obj.preservedParentSheetIndex.Value = result.ToString();
				obj.internalSound?.Stop(AudioStopOptions.Immediate);
				obj.lastNoteBlockSoundTime = 0;
				obj.farmerAdjacentAction(Game1.player, true);
			}
			if (Helper.Input.IsDown(Config.ToneModKey))
			{
				string[] tones = Config.ToneList.Split(',');
				string result = null;

				if (!obj.modData.TryGetValue(advancedFluteBlocksKey, out string tone))
				{
					tone = tones[0];
				}
				for (int i = 0; i < tones.Length; i++)
				{
					if (tone == tones[i])
					{
						int resultIndex = (i + (e.Delta < 0 ? -1 : 1)) % tones.Length;

						if (resultIndex < 0)
						{
							resultIndex += tones.Length;
						}
						result = tones[resultIndex];
					}
				}
				if (result != null)
				{
					obj.modData[advancedFluteBlocksKey] = result;
					obj.internalSound?.Stop(AudioStopOptions.Immediate);
					obj.lastNoteBlockSoundTime = 0;
					obj.farmerAdjacentAction(Game1.player);
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
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
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM_SectionTitle_KeyBinds.Text")
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.PitchModKey.Name"),
				getValue: () => Config.PitchModKey,
				setValue: value => Config.PitchModKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ToneModKey.Name"),
				getValue: () => Config.ToneModKey,
				setValue: value => Config.ToneModKey = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.PitchStep.Name"),
				getValue: () => Config.PitchStep,
				setValue: value => Config.PitchStep = value
			);
		}
	}
}
