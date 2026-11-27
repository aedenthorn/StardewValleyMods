using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace BirthdayFriendship
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static ModConfig Config;

        internal static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            Harmony harmony = new(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Billboard), nameof(Billboard.GetBirthdays)),
                postfix: new HarmonyMethod(typeof(Billboard_GetBirthdays_Patch), nameof(Billboard_GetBirthdays_Patch.Postfix))
            );
            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(NPC), nameof(NPC.Birthday_Season)),
                prefix: new HarmonyMethod(typeof(NPC_Birthday_Season_Patch), nameof(NPC_Birthday_Season_Patch.Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(ProfileMenu), nameof(ProfileMenu.draw), new Type[] { typeof(SpriteBatch) }),
                transpiler: new HarmonyMethod(typeof(ProfileMenu_draw_Patch), nameof(ProfileMenu_draw_Patch.Transpiler))
            );
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                    name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.Hearts.Name"),
                    getValue: () => Config.Hearts,
                    setValue: value => Config.Hearts = value
                );
            }
        }
    }
}
