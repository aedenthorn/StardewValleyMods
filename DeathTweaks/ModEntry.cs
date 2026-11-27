using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace DeathTweaks
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string modKey = "aedenthorn.DeathTweaks";

        private static PerScreen<DeathData> deathData = new();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Player.Warped += Player_Warped;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled || !Context.CanPlayerMove)
                return;
            if (Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out var obj) && obj.modData.TryGetValue(modKey, out var name))
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, string.Format(Helper.Translation.Get("tombstone"), name), Game1.dialogueFont);
            }
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!Config.ModEnabled || deathData.Value is null || !e.Player.IsLocalPlayer || e.NewLocation is not MineShaft || (deathData.Value.location as MineShaft).mineLevel != (e.NewLocation as MineShaft).mineLevel)
                return;
            if ((e.NewLocation as MineShaft).mineLevel > 120)
            {
                float distance = float.MaxValue;
                Vector2 newPosition = deathData.Value.position;
                List<Vector2> list = new();
                for (int j = 0; j < e.NewLocation.map.GetLayer("Back").LayerWidth; j++)
                {
                    for (int k = 0; k < e.NewLocation.map.GetLayer("Back").LayerHeight; k++)
                    {
                        if ((e.NewLocation as MineShaft).isTileClearForMineObjects(j, k) && Vector2.Distance(deathData.Value.position, new Vector2(j, k)) < distance)
                        {
                            newPosition = new Vector2(j, k);
                        }
                    }
                }
                deathData.Value.position = newPosition;
            }
            e.NewLocation.objects[deathData.Value.position] = deathData.Value.chest;
            deathData.Value = null;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            deathData.Value = null;
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
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_DropEverything_Name"),
                    getValue: () => Config.DropEverything,
                    setValue: value => Config.DropEverything = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_DropNothing_Name"),
                    getValue: () => Config.DropNothing,
                    setValue: value => Config.DropNothing = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_CreateChest_Name"),
                    getValue: () => Config.CreateTombstone,
                    setValue: value => Config.CreateTombstone = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_MoneyLostMult_Name"),
                    getValue: () => Config.MoneyLostMult,
                    setValue: value => Config.MoneyLostMult = value
                );
            }
        }
    }
}