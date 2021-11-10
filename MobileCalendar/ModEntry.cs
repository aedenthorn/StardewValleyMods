using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.IO;

namespace MobileCalendar
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        internal static ModConfig Config;

        public static IMobilePhoneApi api;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon;
                bool success;
                appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                success = api.AddApp(Helper.ModRegistry.ModID, Helper.Translation.Get("calendar"), OpenApp, appIcon);
                Monitor.Log($"loaded app successfully: {success}", LogLevel.Debug);
                
            }
        }

        private void OpenApp()
        {
            Monitor.Log("Opening App");
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked; 

        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
            Monitor.Log("Really opening app");
            Game1.activeClickableMenu = new Billboard(false);
        }
    }
}
