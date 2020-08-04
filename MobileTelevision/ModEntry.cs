using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace MobileTelevision
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        internal static ModConfig Config;

        private IMobilePhoneApi api;

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
                success = api.AddApp(Helper.ModRegistry.ModID, Helper.Translation.Get("television"), OpenTelevision, appIcon);
                Monitor.Log($"loaded app successfully: {success}", LogLevel.Debug);
            }
        }

        private void OpenTelevision()
        {
            Monitor.Log("Opening Television");
            DelayedOpen();
            
        }

        private async void DelayedOpen()
        {
            await Task.Delay(50);
            Monitor.Log("Really opening television");
            TV tv = new TV();
            tv.checkForAction(Game1.player);
        }

    }
}
