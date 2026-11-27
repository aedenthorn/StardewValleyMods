using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.TerrainFeatures;

namespace BlinkTeleport
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

        internal static IManaBarApi manaBarApi = null;

        internal static PerScreen<ICue> blinkSound = new();
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

		}

		private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.CanPlayerMove || !Config.BlinkKey.JustPressed() || (Config.OutdoorsOnly && !Game1.player.currentLocation.IsOutdoors))
				return;

            foreach (var kb in Config.BlinkKey.Keybinds)
            {
                foreach (var s in kb.Buttons)
                {
                    SHelper.Input.Suppress(s);
                }
            }

            var tile = Game1.currentCursorTile;
            if (Game1.currentLocation.isCollidingPosition(new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64), Game1.viewport, true, 0, false, Game1.player))
            {
                SMonitor.Log($"Can't blink to {tile.X}, {tile.Y} because it's blocked.");
                return;
            }
            if (manaBarApi is not null && Config.UseMana)
            {
                if (manaBarApi.GetMana(Game1.player) >= Config.StaminaUse)
                {
                    manaBarApi.AddMana(Game1.player, -Config.StaminaUse);
                    DoBlink(tile);
                }
            }
            else
            {
                if (Game1.player.Stamina >= Config.StaminaUse)
                {
                    Game1.player.Stamina = Math.Max(0.1f, Game1.player.Stamina - Config.StaminaUse);
                    DoBlink(tile);
                    return;
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			if (CompatibilityUtility.IsManaBarLoaded)
			{
				manaBarApi = context.Helper.ModRegistry.GetApi<IManaBarApi>("spacechase0.ManaBar");
			}

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
                    name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.OutdoorsOnly.Name"),
                    getValue: () => Config.OutdoorsOnly,
                    setValue: value => Config.OutdoorsOnly = value
                );
                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.BlinkKey.Name"),
                    getValue: () => Config.BlinkKey,
                    setValue: value => Config.BlinkKey = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => CompatibilityUtility.IsManaBarLoaded ? SHelper.Translation.Get("GMCM.StaminaManaUse.Name") : SHelper.Translation.Get("GMCM.StaminaUse.Name"),
                    getValue: () => Config.StaminaUse,
                    setValue: value => Config.StaminaUse = value
                );

                if (CompatibilityUtility.IsManaBarLoaded)
                {
                    configMenu.AddBoolOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get("GMCM.UseMana.Name"),
                        tooltip: () => SHelper.Translation.Get("GMCM.UseMana.Tooltip"),
                        getValue: () => Config.UseMana,
                        setValue: value => Config.UseMana = value
                    );
                }

                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.BlinkSound.Name"),
                    getValue: () => Config.BlinkSound,
                    setValue: value => Config.BlinkSound = value
                );

            }

		}
	}
}
