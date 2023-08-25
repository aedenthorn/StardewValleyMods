using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FarmerPortraits
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private static Texture2D portraitTexture;
        private static Texture2D backgroundTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            helper.Events.Display.MenuChanged += Display_MenuChanged;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.OemCloseBrackets)
            {
                Game1.drawDialogueNoTyping(Game1.getCharacterFromName("Lewis"), "Farts are people too.");
            }
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            ReloadTextures();
        }

        private static void ReloadTextures()
        {
            try
            {
                portraitTexture = SHelper.GameContent.Load<Texture2D>("aedenthorn.FarmerPortraits/portrait");
            }
            catch
            {
                portraitTexture = null;
            }
            try
            {
                backgroundTexture = SHelper.GameContent.Load<Texture2D>("aedenthorn.FarmerPortraits/background");
            }
            catch
            {
                backgroundTexture = null;
            }
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
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show With NPCs",
                getValue: () => Config.ShowWithNPCPortrait,
                setValue: value => Config.ShowWithNPCPortrait = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show With Questions",
                getValue: () => Config.ShowWithQuestions,
                setValue: value => Config.ShowWithQuestions = value
            );;
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Otherwise",
                tooltip: () => "Show for dialogue boxes that are neither questions nor have NPC portraits",
                getValue: () => Config.ShowMisc,
                setValue: value => Config.ShowMisc = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show During Events",
                getValue: () => Config.ShowWithEvents,
                setValue: value => Config.ShowWithEvents = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Facing Front",
                tooltip: () => "If not set, the portrait will face right (only meaningful if there is no custom portrait)",
                getValue: () => Config.FacingFront,
                setValue: value => Config.FacingFront = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Use Custom Portrait",
                tooltip: () => "If a custom portrait png is loaded, use it for the portrait",
                getValue: () => Config.UseCustomPortrait,
                setValue: value => Config.UseCustomPortrait = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Use Custom Background",
                tooltip: () => "If a custom background png is loaded, use it for the background",
                getValue: () => Config.UseCustomBackground,
                setValue: value => Config.UseCustomBackground = value
            );
        }
    }
}