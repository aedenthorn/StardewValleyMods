using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomBackpack
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;


        public static string dictPath = "aedenthorn.CustomBackpack/dictionary";

        public static Dictionary<int, BackPackData> dataDict = new Dictionary<int, BackPackData>();
        public static int IDOffset = 0;
        public static Texture2D scrollTexture;
        public static Texture2D handleTexture;

        public static PerScreen<IClickableMenu> lastMenu = new PerScreen<IClickableMenu>();
        public static PerScreen<int> pressTime = new PerScreen<int>();

        public static PerScreen<int> oldScrollValue = new PerScreen<int>();
        public static PerScreen<int> oldCapacity = new PerScreen<int>();
        public static PerScreen<int> oldRows = new PerScreen<int>();
        public static PerScreen<int> oldScrolled = new PerScreen<int>();
        public static PerScreen<int> scrolled = new PerScreen<int>();
        public static PerScreen<int> scrollChange = new PerScreen<int>();
        public static PerScreen<int> scrollWidth = new PerScreen<int>(() => 4);
        public static PerScreen<bool> scrolling = new PerScreen<bool>();
        public static PerScreen<Rectangle> scrollArea = new PerScreen<Rectangle>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked; ;

            helper.ConsoleCommands.Add("custombackpack", "Manually set backpack slots. Usage: custombackpack <slotnumber>", SetSlots);

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            scrollTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            scrollTexture.SetData(new Color[] { Config.BackgroundColor });
            handleTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            handleTexture.SetData(new Color[] { Config.HandleColor });
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if(scrolling.Value && (Game1.activeClickableMenu is null || Game1.input.GetMouseState().LeftButton != ButtonState.Pressed))
            {
                scrolling.Value = false;
            }
            var newScrollValue = Game1.input.GetMouseState().ScrollWheelValue;
            if (oldScrollValue.Value > newScrollValue)
                scrollChange.Value = 1;
            else if (oldScrollValue.Value < newScrollValue)
                scrollChange.Value = -1;
            else
                scrollChange.Value = 0;
            oldScrollValue.Value = newScrollValue;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(Game1.activeClickableMenu is not null && e.Button == SButton.MouseLeft && scrollArea.Value.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                scrolling.Value = true;
            }
        }

        public override object GetApi()
        {
            return new CustomBackpackApi();
        }
        private void SetSlots(string arg1, string[] arg2)
        {
            if(arg2.Length == 1 && int.TryParse(arg2[0], out int slots))
            {
                SetPlayerSlots(slots);
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            LoadDict();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LoadDict();
        }

        private void LoadDict()
        {
            dataDict = Game1.content.Load<Dictionary<int, BackPackData>>(dictPath);
            foreach (var key in dataDict.Keys.ToArray())
            {
                dataDict[key].texture = Helper.GameContent.Load<Texture2D>(dataDict[key].texturePath);
            }
            SHelper.GameContent.InvalidateCache("String/UI");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<int, BackPackData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("String/UI"))
            {
                e.Edit(EditStrings);
            }
        }

        private void EditStrings(IAssetData obj)
        {
            var editor = obj.AsDictionary<string, string>();
            foreach (var key in dataDict.Keys.ToArray())
            {
                editor.Data[$"Chat_CustomBackpack_{key}"] = string.Format(SHelper.Translation.Get("farmer-bought-x"));
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowArrows_Name"),
                getValue: () => Config.ShowArrows,
                setValue: value => Config.ShowArrows = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowRowNumbers_Name"),
                getValue: () => Config.ShowRowNumbers,
                setValue: value => Config.ShowRowNumbers = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_MinHandleHeight_Name"),
                getValue: () => Config.MinHandleHeight,
                setValue: value => Config.MinHandleHeight = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowExpandedButton_Name"),
                getValue: () => Config.ShowExpandedButton,
                setValue: value => Config.ShowExpandedButton = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShiftRows_Name"),
                getValue: () => Config.ShiftRows,
                setValue: value => Config.ShiftRows = value
            );
        }
    }
}