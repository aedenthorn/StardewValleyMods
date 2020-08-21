using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.IO;
using System.Threading.Tasks;

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
                Game1.chatBox.chatBox.OnEnterPressed += ChatBox_OnEnterPressed;
            }
        }

        private void ChatBox_OnEnterPressed(TextBox sender)
        {
            Monitor.Log($"text: {sender.Text}");
        }

        private void OpenApp()
        {
            Monitor.Log("Opening App");
            DelayedOpen();
            
        }

        private async void DelayedOpen()
        {
            await Task.Delay(50);
            Monitor.Log("Really opening app");
            Game1.activeClickableMenu = new Billboard(false);
        }

    }
}
