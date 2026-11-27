using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.IO;
using System.Threading.Tasks;

namespace MobileTelevision
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        internal static ModConfig Config;

        public static IMobilePhoneApi api;
        private MobileTV tv;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;
        }

        private void Display_RenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if(tv != null && api.GetPhoneOpened())
            {
                tv.draw(e.SpriteBatch, Game1.viewport.Width / 2, Game1.viewport.Height / 2);
            }
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
            tv = new MobileTV(1468, new Vector2((Game1.viewport.X + Game1.viewport.Width / 2 - Game1.tileSize) / Game1.tileSize,(Game1.viewport.Y + Game1.viewport.Height / 2 - Game1.tileSize) / Game1.tileSize));
            tv.checkForAction(Game1.player);
        }

    }
}
