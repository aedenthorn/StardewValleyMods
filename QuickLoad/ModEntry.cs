using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace QuickLoad
{
    public class ModEntry : Mod
    {
        public static ModEntry context;
        private static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;


            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            if (Config.UseLastLoaded)
            {
                Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            }
            else
            {
                Helper.Events.GameLoop.Saved += GameLoop_Saved;
            }
        }

        private void GameLoop_Saved(object sender, StardewModdingAPI.Events.SavedEventArgs e)
        {
            SetSaveFolder();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            SetSaveFolder();
        }

        private void SetSaveFolder()
        {
            Config.SaveFolder = Constants.SaveFolderName;
            Helper.WriteConfig<ModConfig>(Config);
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == Config.Hotkey)
            {
                try
                {
                    if (Game1.activeClickableMenu is TitleMenu)
                    {
                        Game1.activeClickableMenu.exitThisMenu(false);
                        SaveGame.Load(Config.SaveFolder);
                    }

                }
                catch(Exception ex)
                {
                    Monitor.Log($"Error loading save!\r\n\r\n{ex}", LogLevel.Warn);
                }
            }
        }
    }
}
